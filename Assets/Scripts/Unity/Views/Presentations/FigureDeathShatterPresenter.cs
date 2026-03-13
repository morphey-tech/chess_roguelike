using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Logging;
using Project.Core.Core.Random;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views.Presentations
{
    /// <summary>
    /// Презентер смерти фигуры с использованием OpenFracture.
    /// Разрушает меш на части при смерти через упрощённый бокс.
    /// </summary>
    public sealed class FigureDeathShatterPresenter : MonoBehaviour
    {
        // Кэшированный бокс меш (общий для всех инстансов)
        private static Mesh? _cachedBoxMesh;
        private static readonly int Cull = Shader.PropertyToID("_Cull");

        [Header("Debug")]
        [SerializeField] private bool _enableLogging = true;

        private bool _isDying;
        private readonly List<Material> _createdMaterials = new();
        private readonly List<MeshFilter> _meshFilterCache = new();
        private readonly List<(MeshFilter mf, MeshRenderer mr)> _validMeshesCache = new();

        private IRandomService _random = null!;
        private IAssetService _assetService = null!;
        private ILogger<FigureDeathShatterPresenter> _logger = null!;
        
        private FigureShatterConfig? _config;

        [Inject]
        private void Construct(IRandomService random, IAssetService assetService, ILogService logService)
        {
            _random = random;
            _assetService = assetService;
            _logger = logService.CreateLogger<FigureDeathShatterPresenter>();
        }

        public void SetConfig(FigureShatterConfig config)
        {
            _config = config;
            _logger.Info($"[SetConfig] proxyScale={config.ProxyScale}, fragmentScale={config.FragmentScale}, shards={config.MinShards}-{config.MaxShards}");
        }

        public async UniTask PlayDeathAsync()
        {
            if (_isDying || _config == null || _random == null || _assetService == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _isDying = true;

            try
            {
                await PlayDeathInternalAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"[FigureDeath] Error: {ex.Message}");
                gameObject.SetActive(false);
            }
            finally
            {
                CleanupCreatedResources();
            }
        }

        private async UniTask PlayDeathInternalAsync()
        {
            _meshFilterCache.Clear();
            GetComponentsInChildren(false, _meshFilterCache);

            _validMeshesCache.Clear();
            foreach (MeshFilter mf in _meshFilterCache)
            {
                if (!mf.gameObject.activeSelf || mf.sharedMesh == null || mf.gameObject == gameObject)
                {
                    continue;
                }

                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || !mr.enabled)
                {
                    continue;
                }

                _validMeshesCache.Add((mf, mr));
            }

            if (_validMeshesCache.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(false);
            GameObject fragmentRoot = new($"{gameObject.name}_Fragments");
            fragmentRoot.transform.position = transform.position;
            fragmentRoot.transform.rotation = transform.rotation;
            fragmentRoot.transform.SetParent(transform.parent);

            int totalFragmentCount = Mathf.Max(3, _random.Range(_config.MinShards, _config.MaxShards));
            int fragmentsPerMesh = Mathf.Max(2, totalFragmentCount / _validMeshesCache.Count);

            if (_enableLogging)
            {
                _logger.Info($"[FigureDeath] {_validMeshesCache.Count} meshes, total={totalFragmentCount}, per mesh={fragmentsPerMesh}");
            }

            Material? fallbackMaterial = null;
            if (!_config.UseOriginalMaterials && !string.IsNullOrEmpty(_config.FallbackMaterialKey))
            {
                fallbackMaterial = await _assetService.LoadAsync<Material>(_config.FallbackMaterialKey);
            }

            foreach ((MeshFilter mf, MeshRenderer mr) in _validMeshesCache)
            {
                GameObject proxyMesh = CreateProxyMesh(mf, mr, _config.ProxyScale, fallbackMaterial);
                Rigidbody tempRb = proxyMesh.GetComponent<Rigidbody>();
                if (tempRb == null)
                {
                    tempRb = proxyMesh.AddComponent<Rigidbody>();
                    tempRb.isKinematic = true;
                }

                GameObject fragmentTemplate = CreateFragmentTemplate(mr.sharedMaterials, fallbackMaterial);

                FractureOptions options = new()
                {
                    fragmentCount = fragmentsPerMesh,
                    asynchronous = _config.AsyncFracture,
                    detectFloatingFragments = false,
                    xAxis = true,
                    yAxis = true,
                    zAxis = true,
                    textureScale = Vector2.one,
                    textureOffset = Vector2.zero
                };

                Fragmenter.Fracture(proxyMesh, options, fragmentTemplate, fragmentRoot.transform, saveToDisk: false);

                // Применяем масштаб к фрагментам (не умножаем, а устанавливаем)
                foreach (Transform child in fragmentRoot.transform)
                {
                    child.localScale = Vector3.one * _config.FragmentScale;
                }

                Destroy(fragmentTemplate);
                Destroy(proxyMesh);
            }

            ApplyPhysics(fragmentRoot);
        }

        private GameObject CreateProxyMesh(MeshFilter originalMf, MeshRenderer originalMr, float scaleMultiplier, Material? fallbackMaterial)
        {
            GameObject proxy = new($"{originalMf.gameObject.name}_proxy");
            proxy.transform.position = originalMf.transform.position;
            proxy.transform.rotation = originalMf.transform.rotation;
            proxy.transform.localScale = Vector3.one * scaleMultiplier;
            proxy.transform.SetParent(originalMf.transform.parent);

            MeshFilter proxyMf = proxy.AddComponent<MeshFilter>();
            proxyMf.mesh = GetCachedBoxMesh(originalMf.sharedMesh.bounds);

            MeshRenderer proxyMr = proxy.AddComponent<MeshRenderer>();

            if (_config!.UseOriginalMaterials && originalMr.sharedMaterials.Length > 0)
            {
                Material? activeMaterial = null;
                foreach (Material mat in originalMr.sharedMaterials)
                {
                    if (mat != null)
                    {
                        activeMaterial = mat;
                        break;
                    }
                }

                if (activeMaterial != null)
                {
                    if (_config.DoubleSided)
                    {
                        Material doubleSidedMat = new(activeMaterial);
                        doubleSidedMat.SetInt("_Cull", 0);
                        doubleSidedMat.EnableKeyword("_DOUBLESIDED_ON");
                        doubleSidedMat.DisableKeyword("_CULL_ON");
                        _createdMaterials.Add(doubleSidedMat);
                        proxyMr.materials = new[] { doubleSidedMat };
                    }
                    else
                    {
                        proxyMr.materials = new[] { activeMaterial };
                    }
                }
            }
            else if (fallbackMaterial != null)
            {
                proxyMr.material = fallbackMaterial;
            }

            return proxy;
        }

        private static Mesh GetCachedBoxMesh(Bounds bounds)
        {
            if (_cachedBoxMesh == null)
            {
                _cachedBoxMesh = CreateBoxMesh();
                _cachedBoxMesh.name = "ProxyBoxTemplate";
            }

            Mesh instance = Instantiate(_cachedBoxMesh);
            instance.bounds = bounds;
            return instance;
        }

        private static Mesh CreateBoxMesh()
        {
            Mesh boxMesh = new();
            boxMesh.name = "ProxyBox";

            // 24 вершины (4 на каждую из 6 граней)
            Vector3[] vertices = new Vector3[24];
            Vector2[] uv = new Vector2[24];

            // Вершины для каждой грани
            // Front (z = min)
            vertices[0] = new Vector3(-1, -1, -1);
            vertices[1] = new Vector3(1, -1, -1);
            vertices[2] = new Vector3(1, 1, -1);
            vertices[3] = new Vector3(-1, 1, -1);
            // Back (z = max)
            vertices[4] = new Vector3(-1, -1, 1);
            vertices[5] = new Vector3(1, -1, 1);
            vertices[6] = new Vector3(1, 1, 1);
            vertices[7] = new Vector3(-1, 1, 1);
            // Left (x = min)
            vertices[8] = new Vector3(-1, -1, -1);
            vertices[9] = new Vector3(-1, -1, 1);
            vertices[10] = new Vector3(-1, 1, 1);
            vertices[11] = new Vector3(-1, 1, -1);
            // Right (x = max)
            vertices[12] = new Vector3(1, -1, -1);
            vertices[13] = new Vector3(1, -1, 1);
            vertices[14] = new Vector3(1, 1, 1);
            vertices[15] = new Vector3(1, 1, -1);
            // Top (y = max)
            vertices[16] = new Vector3(-1, 1, -1);
            vertices[17] = new Vector3(1, 1, -1);
            vertices[18] = new Vector3(1, 1, 1);
            vertices[19] = new Vector3(-1, 1, 1);
            // Bottom (y = min)
            vertices[20] = new Vector3(-1, -1, -1);
            vertices[21] = new Vector3(1, -1, -1);
            vertices[22] = new Vector3(1, -1, 1);
            vertices[23] = new Vector3(-1, -1, 1);

            // UV для каждой грани
            for (int i = 0; i < 24; i += 4)
            {
                uv[i] = new Vector2(0, 0);
                uv[i + 1] = new Vector2(1, 0);
                uv[i + 2] = new Vector2(1, 1);
                uv[i + 3] = new Vector2(0, 1);
            }

            // 36 треугольников (6 граней * 2 треугольника * 3 индекса)
            int[] triangles = new int[36]
            {
                // Front
                0, 2, 1, 0, 3, 2,
                // Back
                4, 5, 6, 4, 6, 7,
                // Left
                8, 10, 9, 8, 11, 10,
                // Right
                12, 14, 13, 12, 15, 14,
                // Top
                16, 18, 17, 16, 19, 18,
                // Bottom
                20, 22, 21, 20, 23, 22
            };

            boxMesh.vertices = vertices;
            boxMesh.uv = uv;
            boxMesh.triangles = triangles;
            boxMesh.RecalculateNormals();

            return boxMesh;
        }

        private GameObject CreateFragmentTemplate(Material[] materials, Material? fallbackMaterial)
        {
            GameObject template = new("FragmentTemplate");
            template.AddComponent<MeshFilter>();

            MeshRenderer mr = template.AddComponent<MeshRenderer>();
            if (_config!.UseOriginalMaterials && materials.Length > 0)
            {
                mr.materials = materials;
            }
            else if (fallbackMaterial != null)
            {
                mr.material = fallbackMaterial;
            }

            // Масштаб осколка
            template.transform.localScale = Vector3.one * _config.FragmentScale;

            Rigidbody rb = template.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.useGravity = true;
            rb.linearDamping = _config.Drag;
            rb.angularDamping = _config.AngularDrag;

            MeshCollider cl = template.AddComponent<MeshCollider>();
            cl.convex = true;

            if (_config.Lifetime > 0)
            {
                FragmentLifetime lifetime = template.AddComponent<FragmentLifetime>();
                lifetime.lifetime = _config.Lifetime;
            }

            return template;
        }

        private void ApplyPhysics(GameObject fragmentRoot)
        {
            Rigidbody[] rigidbodies = fragmentRoot.GetComponentsInChildren<Rigidbody>();
            float scatterMin = _config!.ScatterForceMin;
            float scatterMax = _config!.ScatterForceMax;
            float upwardMin = _config!.UpwardForceMin;
            float upwardMax = _config!.UpwardForceMax;
            float forceMult = _config!.ForceMultiplier;

            foreach (Rigidbody rb in rigidbodies)
            {
                Vector3 scatterForce = new Vector3(
                    _random!.Range(-1f, 1f),
                    _random!.Range(upwardMin, upwardMax),
                    _random!.Range(-1f, 1f)
                ).normalized * _random!.Range(scatterMin, scatterMax) * forceMult;

                Vector3 torque = new(
                    _random!.Range(-0.5f, 0.5f),
                    _random!.Range(-0.5f, 0.5f),
                    _random!.Range(-0.5f, 0.5f)
                );

                rb.AddForce(scatterForce, ForceMode.Impulse);
                rb.AddTorque(torque, ForceMode.Impulse);
            }
        }

        private void CleanupCreatedResources()
        {
            foreach (Material mat in _createdMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
            _createdMaterials.Clear();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            CleanupCreatedResources();
        }
    }
}
