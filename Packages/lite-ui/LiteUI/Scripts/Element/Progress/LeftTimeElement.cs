using System;
using LiteUI.Binding.Attributes;
using LiteUI.Common.Extensions;
using LiteUI.Element.Texts;

namespace LiteUI.Element.Progress
{
    [UIController("LeftTimeElement")]
    public class LeftTimeElement : LeftTimeCounter
    {
        private DurationFormatter? _durationFormatter;

        public event Action? OnTimeEndedEvent;
        
        protected override void DoAwake()
        {
            OnEndTimeEvent += OnEndTime;
        }

        protected override void UpdateTime()
        {
            DurationFormatter.SetDuration(LeftTime);
        }

        private void OnEndTime()
        {
            if (this.IsDestroyed()) {
                return;
            }
            gameObject.SetActive(false);
            OnTimeEndedEvent?.Invoke();
        }
        private DurationFormatter DurationFormatter
        {
            get
            {
                if (_durationFormatter != null) {
                    return _durationFormatter;
                }
                _durationFormatter = GetComponentInChildren<DurationFormatter>(true);
                return _durationFormatter;
            }
        }
    }
}
