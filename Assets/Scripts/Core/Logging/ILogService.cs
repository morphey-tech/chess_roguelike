using System;

namespace Project.Core.Logging
{
    /// <summary>
    /// Уровень логирования
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    }
    
    /// <summary>
    /// Интерфейс сервиса логирования
    /// </summary>
    public interface ILogService
    {
        LogLevel MinLevel { get; set; }
        
        void Log(LogLevel level, string category, string message);
        void Log(LogLevel level, string category, string message, Exception exception);
        
        ILogger CreateLogger(string category);
        ILogger<T> CreateLogger<T>();
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
    
    /// <summary>
    /// Типизированный логгер
    /// </summary>
    public interface ILogger<T> : ILogger
    {
    }
}


