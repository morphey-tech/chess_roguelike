using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Gameplay.Gameplay.Board;
using UnityEngine;

namespace Project.Unity.Unity.Views.Components
{
    /// <summary>
    /// Default cell visual component. Attach to cell prefabs.
    /// Uses DOTween for animations.
    /// </summary>
    public class BoardCellView : MonoBehaviour, IBoardCellView
    {
        [Header("Appear")]
        [SerializeField] private float _appearDuration = 0.2f;
        [SerializeField] private Ease _appearEase = Ease.OutBack;
        
        [Header("Hit")]
        [SerializeField] private float _hitDuration = 0.15f;
        [SerializeField] private float _hitShake = 0.05f;
        
        [Header("Highlight")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _highlightColor = new(1f, 1f, 0.5f, 1f);
        [SerializeField] private Color _moveTargetColor = new(0.5f, 1f, 0.5f, 1f);
        [SerializeField] private Color _attackTargetColor = new(1f, 0.5f, 0.5f, 1f);
        
        private Color _originalColor;
        private Material _material;

        private void Awake()
        {
            if (_renderer != null)
            {
                _material = _renderer.material;
                _originalColor = _material.color;
            }
        }

        public async UniTask PlayAppearAsync()
        {
            Vector3 targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            
            await transform
                .DOScale(targetScale, _appearDuration)
                .SetEase(_appearEase)
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

        public void SetMoveTarget(bool enabled)
        {
            if (_material != null)
            {
                _material.color = enabled ? _moveTargetColor : _originalColor;
            }
        }

        public void SetAttackTarget(bool enabled)
        {
            if (_material != null)
            {
                _material.color = enabled ? _attackTargetColor : _originalColor;
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
