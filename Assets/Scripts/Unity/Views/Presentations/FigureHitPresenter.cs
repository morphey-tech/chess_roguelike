using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public sealed class FigureHitPresenter : MonoBehaviour
    {
        [Header("Hit")]
        [SerializeField] private float _hitDuration = 0.1f;
        [SerializeField] private float _hitShake = 0.1f;
        [SerializeField] private int _flashCount = 2;
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private Color _flashColor = Color.red;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int MainColorId = Shader.PropertyToID("_MainColor");

        public async UniTask PlayHitAsync()
        {
            Tween flash = CreateFlashTween();
            Tween shake = transform
                .DOShakePosition(_hitDuration, _hitShake)
                .SetEase(Ease.OutQuad);

            await DOTween.Sequence()
                .Join(flash)
                .Join(shake)
                .AsyncWaitForCompletion();
        }

        private Tween CreateFlashTween()
        {
            var entries = CollectFlashEntries();
            if (entries.Count == 0 || _flashCount <= 0)
                return DOTween.Sequence();

            float t = 0f;
            Tween tween = DOTween.To(
                    () => t,
                    v =>
                    {
                        t = v;
                        ApplyFlash(entries, t);
                    },
                    1f,
                    _flashDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(_flashCount * 2, LoopType.Yoyo)
                .OnComplete(() => ApplyFlash(entries, 0f));

            return tween;
        }

        private sealed class FlashEntry
        {
            public Renderer Renderer;
            public int ColorPropertyId;
            public Color Original;
        }

        private List<FlashEntry> CollectFlashEntries()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            var list = new List<FlashEntry>(renderers.Length);

            foreach (Renderer r in renderers)
            {
                if (r == null || r.sharedMaterial == null)
                    continue;

                int propertyId = GetColorPropertyId(r.sharedMaterial);
                if (propertyId == 0)
                    continue;

                list.Add(new FlashEntry
                {
                    Renderer = r,
                    ColorPropertyId = propertyId,
                    Original = r.sharedMaterial.GetColor(propertyId)
                });
            }

            return list;
        }

        private static int GetColorPropertyId(Material material)
        {
            if (material.HasProperty(BaseColorId))
                return BaseColorId;
            if (material.HasProperty(ColorId))
                return ColorId;
            if (material.HasProperty(MainColorId))
                return MainColorId;
            return 0;
        }

        private void ApplyFlash(List<FlashEntry> entries, float t)
        {
            foreach (FlashEntry entry in entries)
            {
                if (entry.Renderer == null)
                    continue;

                Color c = Color.Lerp(entry.Original, _flashColor, t);
                var block = new MaterialPropertyBlock();
                entry.Renderer.GetPropertyBlock(block);
                block.SetColor(entry.ColorPropertyId, c);
                entry.Renderer.SetPropertyBlock(block);
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
