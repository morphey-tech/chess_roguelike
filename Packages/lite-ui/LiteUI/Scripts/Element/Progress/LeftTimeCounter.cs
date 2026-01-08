using System;
using LiteUI.Common.Utils;
using UnityEngine;

namespace LiteUI.Element.Progress
{
    public abstract class LeftTimeCounter : MonoBehaviour
    {
        private float _duration;
        private DateTime _startDate;
        private int _prevElapsed = int.MaxValue;
        private bool _active;
        private DateTime? _pauseDate;
        private float _pauseDuration;
        
        public event Action? OnEndTimeEvent;

        private void Awake()
        {
            Active = true;
            DoAwake();
        }

        protected abstract void DoAwake();

        private void Update()
        {
            if (LeftTime <= 0 || !Active) {
                return;
            }
            
            int elapsed = (int) ((DateTime.Now - _startDate).TotalSeconds - _pauseDuration);
            if (_prevElapsed == elapsed) {
                return;
            }
            
            _prevElapsed = elapsed;
            if (LeftTime <= 0) {
                OnEndTimeEvent?.Invoke();
                return;
            }

            UpdateTime();
        }

        public virtual void ResetTimer()
        {
            _duration = 0;
            _prevElapsed = 0;
            _startDate = DateTime.Now;
            gameObject.SetActive(true);
            _pauseDuration = 0;
            _pauseDate = null;
        }

        public void SetTimer(DateTime startTime, float duration)
        {
            _duration = duration;
            if (MathUtils.IsFloatEquals(_duration, 0)) {
                OnEndTimeEvent?.Invoke();
            } else {
                _prevElapsed = 0;
                _startDate = startTime;
                gameObject.SetActive(true);
                UpdateTime();
            }
            _pauseDuration = 0;
            _pauseDate = null;
        }

        public void SetTimer(float duration)
        {
            SetTimer(DateTime.Now, duration);
        }

        protected abstract void UpdateTime();

        public float LeftTime => _duration - _prevElapsed;

        public float Progress => _prevElapsed / _duration;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                
                if (!_active) {
                    _pauseDate = DateTime.Now;
                } else {
                    if (_pauseDate.HasValue) {
                        _pauseDuration += (float) (DateTime.Now - _pauseDate.Value).TotalSeconds;
                    }
                    
                    _pauseDate = null;
                }
            }
        }
    }
}
