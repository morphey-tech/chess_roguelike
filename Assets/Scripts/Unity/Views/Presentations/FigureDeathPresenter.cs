using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public sealed class FigureDeathPresenter : MonoBehaviour
    {
        [Header("Death")]
        [SerializeField] private float _deathDuration = 0.3f;

        public async UniTask PlayDeathAsync()
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
