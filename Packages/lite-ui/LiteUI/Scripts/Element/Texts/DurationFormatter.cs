using System;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Element.Texts
{
    [PublicAPI, RequireComponent(typeof(UILabel))]
    public class DurationFormatter : MonoBehaviour
    {
        [SerializeField]
        private string? _timerTextDays = "{0}";
        [SerializeField]
        private string? _timerTextHours = "{1:D2}";
        [SerializeField]
        private string? _timerTextMinutes = "{2:D2}";
        [SerializeField]
        private string? _timerTextSeconds = "{3:D2}";
        [SerializeField]
        private bool _showTrailingZeroParts = true;
        [SerializeField]
        private string? _prefix;
        [SerializeField]
        private string _splitter = ":";
        [SerializeField]
        private int _minPartCount = 2;
        [SerializeField]
        private int _maxPartCount = 3;

        private UILabel? _label;
        private TimeSpan _duration;

        public void SetDuration(TimeSpan duration)
        {
            if (Mathf.CeilToInt((float) _duration.TotalSeconds) == Mathf.CeilToInt((float) duration.TotalSeconds)) {
                return;
            }
            _duration = duration;
            Render();
        }

        public void SetDuration(float duration)
        {
            SetDuration(TimeSpan.FromSeconds(duration));
        }

        public DurationFormatter ShowZeroParts(bool showZeroParts)
        {
            _showTrailingZeroParts = showZeroParts;
            return this;
        }
        
        public DurationFormatter Prefix(string? prefix)
        {
            _prefix = prefix;
            return this;
        }

        public DurationFormatter PartsCount(int partsCount)
        {
            _maxPartCount = partsCount;
            return this;
        }
        
        public DurationFormatter Format(string? daysText, string? hoursText, string? minutesText, string? secondsText)
        {
            _timerTextDays = daysText;
            _timerTextHours = hoursText;
            _timerTextMinutes = minutesText;
            _timerTextSeconds = secondsText;
            return this;
        }

        private void Render()
        {
            TimeSpan duration = _duration.Milliseconds == 0 ? _duration : _duration.Add(TimeSpan.FromSeconds(1));
            int[] parts = { duration.Days, duration.Hours, duration.Minutes, duration.Seconds };
            string?[] formats = { _timerTextDays, _timerTextHours, _timerTextMinutes, _timerTextSeconds };
            StringBuilder result = new();
            if (!string.IsNullOrEmpty(_prefix)) {
                result.Append(_prefix).Append(' ');
            }
            
            bool show = false;
            int partsAdded = 0;
            for (int i = 0; i < parts.Length; i++) {
                show |= parts[i] > 0 || parts.Length - i <= _minPartCount;
                if (show && !string.IsNullOrEmpty(formats[i]) && (parts[i] > 0 || _showTrailingZeroParts)) {
                    if (partsAdded != 0 && !string.IsNullOrEmpty(_splitter)) {
                        result.Append(_splitter);
                    } 
                    result.Append(formats[i]);
                    partsAdded++;
                    if (partsAdded >= _maxPartCount) {
                        break;
                    }
                }
            }

            string formattedString = string.Format(result.ToString(), duration.Days, duration.Hours, duration.Minutes, duration.Seconds);
            Label.Text = formattedString;
        }

        private UILabel Label
        {
            get 
            {
                if (_label == null) {
                    _label = GetComponent<UILabel>();
                }
                return _label;
            }
        }
    }
}
