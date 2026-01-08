using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Element.Progress
{
    [ExecuteInEditMode]
    public class RadialProgress : MonoBehaviour
    {
        private const float MAX_DEGREE = 360;

        [SerializeField, Range(0, 1)]
        private float _value;
        [SerializeField]
        private Image? _fillImage;
        [SerializeField]
        private RectTransform? _rotation;

#if UNITY_EDITOR
        private void Update()
        {
            SetProgress(_value);
        }
#endif

        public void SetProgress(float progress)
        {
            _value = progress;
            if (_fillImage != null) {
                _fillImage.fillAmount = progress;
            }
            if (_rotation != null) {
                _rotation.eulerAngles = new Vector3(0, 0, MAX_DEGREE * -progress);
            }
        }
    }
}
