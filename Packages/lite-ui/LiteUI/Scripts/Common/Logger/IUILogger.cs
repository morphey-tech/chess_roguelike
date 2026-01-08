using System;

namespace LiteUI.Common.Logger
{
    public interface IUILogger
    {
        void Trace(string message);
        void Debug(string message);
        void Debug(string message, Exception ex);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(string message, Exception ex);
    }

    public static class LoggerFactory
    {
        public static IUILogger GetLogger<T>() => new UnityLogger(typeof(T).Name);
        public static IUILogger GetLogger(string name) => new UnityLogger(name);
    }

    public class UnityLogger : IUILogger
    {
        private readonly string _name;

        public UnityLogger(string name) => _name = name;

        public void Trace(string message) => UnityEngine.Debug.Log($"[{_name}] [TRACE] {message}");
        public void Debug(string message) => UnityEngine.Debug.Log($"[{_name}] {message}");
        public void Debug(string message, Exception ex) => UnityEngine.Debug.Log($"[{_name}] {message}: {ex}");
        public void Info(string message) => UnityEngine.Debug.Log($"[{_name}] {message}");
        public void Warn(string message) => UnityEngine.Debug.LogWarning($"[{_name}] {message}");
        public void Error(string message) => UnityEngine.Debug.LogError($"[{_name}] {message}");
        public void Error(string message, Exception ex) => UnityEngine.Debug.LogError($"[{_name}] {message}: {ex}");
    }
}

