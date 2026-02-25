using System;

namespace Project.Core.Core.Logging
{
    public interface ILogService
    {
        LogLevel MinLevel { get; set; }
        
        void Log(LogLevel level, string category, string message);
        void Log(LogLevel level, string category, string message, Exception exception);
        
        ILogger CreateLogger(string category);
        ILogger<T> CreateLogger<T>();
    }
}


