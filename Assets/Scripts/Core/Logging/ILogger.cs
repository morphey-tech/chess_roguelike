using System;

namespace Project.Core.Core.Logging
{
    /// <summary>
    /// Типизированный логгер
    /// </summary>
    public interface ILogger<T> : ILogger
    {
    }

    /// <summary>
    /// Интерфейс логгера для категории
    /// </summary>
    public interface ILogger
    {
        string Category { get; }

        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Error(string message, Exception exception);
        void Fatal(string message);
        void Fatal(string message, Exception exception);
    }
}