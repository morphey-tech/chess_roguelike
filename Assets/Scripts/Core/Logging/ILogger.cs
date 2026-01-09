using System;

namespace Project.Core.Logging
{
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