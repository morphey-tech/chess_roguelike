using System;
using JetBrains.Annotations;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public interface ICallbackObject<in T>
    {
        void Invoke(T param);
    }

    [PublicAPI]
    public interface ICallbackObject<in T1, in T2>
    {
        void Invoke(T1 param1, T2 param2);
    }

    [PublicAPI]
    public interface ICallbackObject
    {
        void Invoke();
    }

    [PublicAPI]
    public class CallbackObject : ICallbackObject
    {
        private readonly Action _callback;

        public CallbackObject(Action callback)
        {
            _callback = callback;
        }

        public void Invoke()
        {
            _callback.Invoke();
        }
    }

    [PublicAPI]
    public class CallbackObject<T> : ICallbackObject<T>
    {
        private readonly Action<T?> _callback;

        public CallbackObject(Action<T?> callback)
        {
            _callback = callback;
        }

        public void Invoke(T? param)
        {
            _callback.Invoke(param);
        }
    }

    [PublicAPI]
    public class CallbackObject<T1, T2> : ICallbackObject<T1, T2>
    {
        private readonly Action<T1?, T2?> _callback;

        public CallbackObject(Action<T1?, T2?> callback)
        {
            _callback = callback;
        }

        public void Invoke(T1? param1, T2? param2)
        {
            _callback.Invoke(param1, param2);
        }
    }

    [PublicAPI]
    public class CallbackObjectParam<T1, T2> : ICallbackObject<T1>
    {
        private readonly Action<T1?, T2?> _callback;
        private readonly T2? _param;

        public CallbackObjectParam(Action<T1?, T2?> callback, T2? param)
        {
            _callback = callback;
            _param = param;
        }

        public void Invoke(T1? param)
        {
            _callback.Invoke(param, _param);
        }
    }
}
