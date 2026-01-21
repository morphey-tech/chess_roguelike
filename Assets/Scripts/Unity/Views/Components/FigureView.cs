using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Gameplay.Gameplay.Figures;
using UnityEngine;

namespace Project.Unity.Unity.Views.Components
{
    /// <summary>
    /// Default figure visual component. Attach to figure prefabs.
    /// Uses DOTween for animations.
    /// </summary>
    public class FigureView : MonoBehaviour, IFigureView
    {
        [Header("Movement")]
        [SerializeField] private float _moveDuration = 0.25f;
        [SerializeField] private Ease _moveEase = Ease.InOutQuad;
        [SerializeField] private float _jumpHeight = 0.3f;
        
        [Header("Attack")]
        [SerializeField] private float _attackDuration = 0.15f;
        [SerializeField] private float _attackDistance = 0.3f;
        
        [Header("Death")]
        [SerializeField] private float _deathDuration = 0.3f;
        
        [Header("Hit")]
        [SerializeField] private float _hitDuration = 0.1f;
        [SerializeField] private float _hitShake = 0.1f;
        
        [Header("Highlight")]
        [SerializeField] private Renderer _highlightRenderer;
        [SerializeField] private Color _highlightColor = Color.yellow;
        
        private Color _originalColor;
        private Material _material;

        private void Awake()
        {
            if (_highlightRenderer != null)
            {
                _material = _highlightRenderer.material;
                _originalColor = _material.color;
            }
        }
        
        public async UniTask PlayMoveAsync(Vector3 targetPosition)
        {
            await transform
                .DOJump(targetPosition, _jumpHeight, 1, _moveDuration)
                .SetEase(_moveEase)
                .AsyncWaitForCompletion();
        }

        public async UniTask PlayAttackAsync(Vector3 targetPosition)
        {
            Vector3 originalPos = transform.position;
            Vector3 direction = (targetPosition - originalPos).normalized;
            Vector3 attackPos = originalPos + direction * _attackDistance;
            
            // Lunge forward
            await transform
                .DOMove(attackPos, _attackDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();
            
            // Return
            await transform
                .DOMove(originalPos, _attackDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();
        }

        public async UniTask PlayDeathAsync()
        {
            await transform
                .DOScale(Vector3.zero, _deathDuration)
                .SetEase(Ease.InBack)
                .AsyncWaitForCompletion();
        }

        public async UniTask PlayHitAsync()
        {
            await transform
                .DOShakePosition(_hitDuration, _hitShake)
                .AsyncWaitForCompletion();
        }

        public void SetHighlight(bool enabled)
        {
            if (_material != null)
            {
                _material.color = enabled ? _highlightColor : _originalColor;
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
