using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Grid;
using Project.Core.Core.Configs.Visual;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    public sealed class ProjectilePresenter : IProjectilePresenter, IGameShutdownCleanup
    {
        private const float DEFAULT_SPEED = 8f;
        private const float DEFAULT_DURATION_SECONDS = 0.25f;
        private const float BEAM_DURATION_SECONDS = 0.12f;
        private const float BEAM_WIDTH = 0.05f;

        private readonly IAssetService _assetService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<ProjectilePresenter> _logger;

        private ProjectileConfigRepository? _repo;
        private readonly HashSet<GameObject> _active = new();
        private GameObject _lastFlownProjectile;
        private bool _lastShouldRelease;

        [Inject]
        private ProjectilePresenter(
            IAssetService assetService,
            IWorldRoot worldRoot,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _assetService = assetService;
            _worldRoot = worldRoot;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<ProjectilePresenter>();
        }

        public async UniTask FlyProjectileAsync(ProjectileVisualContext ctx)
        {
            ProjectileConfig? cfg = TryGetConfig(ctx.ProjectileConfigId);
            Vector3 from = GetCellTopPosition(ctx.From);
            Vector3 to = GetCellTopPosition(ctx.To);

            if (cfg != null)
            {
                from.y += cfg.HeightOffset;
                to.y += cfg.HeightOffset;
            }

            (GameObject projectile, bool shouldRelease) = await SpawnProjectileAsync(cfg, from);
            if (projectile == null)
            {
                _logger.Warning($"Projectile spawn failed for config '{ctx.ProjectileConfigId}'");
                return;
            }

            _active.Add(projectile);
            _lastFlownProjectile = projectile;
            _lastShouldRelease = shouldRelease;

            float speed = cfg?.Speed > 0f ? cfg.Speed : DEFAULT_SPEED;
            float duration = speed > 0f ? Vector3.Distance(from, to) / speed : DEFAULT_DURATION_SECONDS;

            await MoveOverTime(projectile, from, to, duration);
        }

        public async UniTask PlayImpactAtAsync(GridPosition position, string impactFxId = null)
        {
            if (string.IsNullOrWhiteSpace(impactFxId))
            {
                return;
            }
            Vector3 worldPos = GetCellTopPosition(position);
            GameObject fx = await _assetService.InstantiateAsync(impactFxId, worldPos, Quaternion.identity, _worldRoot.EffectsRoot);
            if (fx == null)
            {
                return;
            }
            await UniTask.Delay(500);
            _assetService.Release(fx);
        }

        public async UniTask CleanupLastProjectileAsync()
        {
            if (_lastFlownProjectile == null)
            {
                await UniTask.CompletedTask;
                return;
            }
            CleanupProjectile(_lastFlownProjectile, _lastShouldRelease);
            _lastFlownProjectile = null;
            await UniTask.CompletedTask;
        }

        public async UniTask PlayBeamAsync(BeamVisualContext ctx)
        {
            Vector3 from = GetCellTopPosition(ctx.From);
            Vector3 to = GetCellTopPosition(ctx.To);
            await PlayLineAsync(from, to, BEAM_DURATION_SECONDS, BEAM_WIDTH);
        }

        public async UniTask PlayWaveAsync(WaveVisualContext ctx)
        {
            Vector3 from = GetCellTopPosition(ctx.From);
            Vector3 to = GetCellTopPosition(ctx.To);
            await PlayLineAsync(from, to, BEAM_DURATION_SECONDS * 1.5f, BEAM_WIDTH * 1.4f);
        }

        public void Clear()
        {
            _lastFlownProjectile = null;
            foreach (GameObject go in _active)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _active.Clear();
        }

        void IGameShutdownCleanup.Cleanup()
        {
            Clear();
        }

        private ProjectileConfig? TryGetConfig(string projectileConfigId)
        {
            if (string.IsNullOrWhiteSpace(projectileConfigId))
                return null;

            try
            {
                _repo ??= _configProvider.GetSync<ProjectileConfigRepository>("projectiles_conf");
                return _repo.Get(projectileConfigId);
            }
            catch
            {
                _logger.Warning("Projectile configs not loaded (projectiles_conf)");
                return null;
            }
        }

        private async UniTask<(GameObject projectile, bool shouldRelease)> SpawnProjectileAsync(ProjectileConfig? cfg, Vector3 from)
        {
            if (cfg != null && !string.IsNullOrWhiteSpace(cfg.AssetKey))
            {
                GameObject prefab = await _assetService.InstantiateAsync(cfg.AssetKey, from, Quaternion.identity, _worldRoot.EffectsRoot);
                if (prefab != null)
                {
                    return (prefab, true);
                }
            }

            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallback.transform.SetParent(_worldRoot.EffectsRoot, false);
            fallback.transform.position = from;
            fallback.transform.localScale = Vector3.one * 0.2f;
            return (fallback, false);
        }

        private static async UniTask MoveOverTime(GameObject projectile, Vector3 from,
            Vector3 to, float duration)
        {
            if (projectile == null)
                return;

            if (duration <= 0f)
            {
                projectile.transform.position = to;
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration && projectile != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                projectile.transform.position = Vector3.Lerp(from, to, t);
                await UniTask.Yield();
            }

            if (projectile != null)
                projectile.transform.position = to;
        }

        private async UniTask PlayLineAsync(Vector3 from, Vector3 to, float durationSeconds, float width)
        {
            GameObject go = new("BeamEffect");
            go.transform.SetParent(_worldRoot.EffectsRoot, false);

            LineRenderer? line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startWidth = width;
            line.endWidth = width;
            line.material = new Material(Shader.Find("Sprites/Default"));

            await UniTask.Delay((int)(durationSeconds * 1000f));
            Object.Destroy(go);
        }

        private void CleanupProjectile(GameObject projectile, bool shouldRelease)
        {
            _active.Remove(projectile);

            if (projectile == null)
                return;

            if (shouldRelease)
            {
                _assetService.Release(projectile);
            }
            else
            {
                Object.Destroy(projectile);
            }
        }

        private static Vector3 GetCellTopPosition(GridPosition gridPos)
        {
            const float surfaceY = 0f;

            Vector3 rayOrigin = new(gridPos.Column, PhysicsSettings.CellRaycastHeight, gridPos.Row);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                PhysicsSettings.CellRaycastHeight * 2f, PhysicsSettings.CellLayerMask))
            {
                return hit.point;
            }

            return new Vector3(gridPos.Column, surfaceY, gridPos.Row);
        }
    }
}
