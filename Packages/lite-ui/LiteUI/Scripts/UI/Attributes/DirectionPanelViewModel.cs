using LiteUI.Common.Model;
using UnityEngine;

namespace LiteUI.UI.Attributes
{
    public class DirectionPanelViewModel : MonoBehaviour
    {
        private const float DEFAULT_TWEEN_DURATION = 0.4f;
        
        [field: SerializeField]
        public Direction Direction { get; private set; }
        [field: SerializeField]
        public float ShowOffset { get; private set; }
        [field: SerializeField]
        public float Duration { get; private set; } = DEFAULT_TWEEN_DURATION;
        [field: SerializeField, HideInInspector]
        public RectTransform RectTransform { get; private set; } = null!;

        private void OnValidate()
        {
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
