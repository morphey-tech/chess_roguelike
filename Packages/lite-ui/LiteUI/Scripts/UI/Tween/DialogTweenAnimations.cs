using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace LiteUI.UI.Tween
{
    public class DialogTweenAnimations : MonoBehaviour
    {
        [SerializeField]
        private DOTweenAnimation _showTween = null!;
        [SerializeField]
        private DOTweenAnimation _hideTween = null!;
        [SerializeField]
        private DOTweenAnimation _overlapTween = null!;

        public DOTweenAnimation ShowTween => _showTween;
        public DOTweenAnimation HideTween => _hideTween;
        public DOTweenAnimation OverlapTween => _overlapTween;
    }
}
