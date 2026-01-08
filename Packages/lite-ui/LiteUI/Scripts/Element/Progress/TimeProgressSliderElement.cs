using LiteUI.Binding.Attributes;
using UnityEngine.UI;

namespace LiteUI.Element.Progress
{
    [UIController("TimeProgressSliderElement")]
    public class TimeProgressSliderElement : LeftTimeCounter
    {
        private Slider _slider = null!;

        protected override void DoAwake()
        {
            _slider = GetComponentInChildren<Slider>();

            OnEndTimeEvent += () => gameObject.SetActive(false);
        }

        protected override void UpdateTime()
        {
            _slider.value = Progress;
        }
    }
}
