using UnityEngine.UI;

namespace LiteUI.Element.Progress
{
    public class TimeProgressElement : LeftTimeCounter
    {
        private Image _progressBar = null!;

        protected override void DoAwake()
        {
            _progressBar = GetComponentInChildren<Image>();

            OnEndTimeEvent += () => gameObject.SetActive(false);
        }

        protected override void UpdateTime()
        {
            _progressBar.fillAmount = Progress;
        }
    }
}
