using System;

namespace LiteUI.Common.Model
{
    public class Timer
    {
        private readonly TimeSpan _duration;
        private DateTime _completeTime;
        private bool _started;

        public Timer(TimeSpan duration)
        {
            _duration = duration;
        }

        public void Start()
        {
            _started = true;
            _completeTime = DateTime.Now.Add(_duration);
        }

        public bool IsElapsed()
        {
            if (!_started) {
                return true;
            }
            if (DateTime.Now > _completeTime) {
                _started = false;
                return true;
            }
            return false;
        }
    }
}
