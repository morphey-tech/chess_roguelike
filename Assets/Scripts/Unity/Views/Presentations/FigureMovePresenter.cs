using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public sealed class FigureMovePresenter : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveDuration = 0.25f;
        [SerializeField] private Ease _moveEase = Ease.InOutQuad;
        [SerializeField] private float _jumpHeight = 0.3f;

        public async UniTask PlayMoveAsync(Vector3 targetPosition)
        {
            await transform
                .DOJump(targetPosition, _jumpHeight, 1, _moveDuration)
                .SetEase(_moveEase)
                .AsyncWaitForCompletion();
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
