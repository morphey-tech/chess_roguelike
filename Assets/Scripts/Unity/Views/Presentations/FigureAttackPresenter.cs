using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public sealed class FigureAttackPresenter : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float _attackDuration = 0.15f;
        [SerializeField] private float _attackDistance = 0.3f;

        public async UniTask PlayAttackAsync(Vector3 targetPosition)
        {
            Vector3 originalPos = transform.position;
            Vector3 direction = (targetPosition - originalPos).normalized;
            Vector3 attackPos = originalPos + direction * _attackDistance;

            await transform
                .DOMove(attackPos, _attackDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();

            await transform
                .DOMove(originalPos, _attackDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
