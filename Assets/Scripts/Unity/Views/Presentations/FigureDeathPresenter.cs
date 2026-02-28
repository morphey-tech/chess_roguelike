using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    /// <summary>
    /// Презентер смерти фигуры. Поддерживает два режима:
    /// 1. Shatter — разрушение на осколки через OpenFracture (рекомендуется)
    /// 2. Scale — старая анимация исчезновения (fallback)
    /// </summary>
    public sealed class FigureDeathPresenter : MonoBehaviour
    {
        [Header("Death Mode")]
        [SerializeField] private bool _useShatter = true;

        [Header("Scale Death Settings")]
        [SerializeField] private float _deathDuration = 0.3f;

        [Header("Shatter Settings")]
        [SerializeField] private FigureDeathShatterPresenter? _shatterPresenter;

        public async UniTask PlayDeathAsync()
        {
            if (_useShatter && _shatterPresenter != null)
            {
                await _shatterPresenter.PlayDeathAsync();
            }
            else
            {
                await PlayScaleDeathAsync();
            }
        }

        private async UniTask PlayScaleDeathAsync()
        {
            await transform
                .DOScale(Vector3.zero, _deathDuration)
                .SetEase(Ease.InBack)
                .AsyncWaitForCompletion();
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
