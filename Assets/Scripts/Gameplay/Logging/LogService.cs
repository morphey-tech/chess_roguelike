using System;
using Microsoft.Extensions.Logging;
using Project.Core.Core.Logging;
using ZLogger.Unity;
using ILogger = Project.Core.Core.Logging.ILogger;
using LogLevel = Project.Core.Core.Logging.LogLevel;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Project.Gameplay.Gameplay.Logging
{
    /// <summary>
    /// Реализация сервиса логирования на ZLogger
    /// </summary>
    public class LogService : ILogService, IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        
        public LogLevel MinLevel { get; set; }

        public LogService(LogLevel minLevel = LogLevel.Debug)
        {
            MinLevel = minLevel;
            
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(ToMsLogLevel(minLevel));
                builder.AddZLoggerUnityDebug();
            });
        }
        
        public void Log(LogLevel level, string category, string message)
        {
            if (level < MinLevel)
            {
                return;
            }

            Microsoft.Extensions.Logging.ILogger logger = _loggerFactory.CreateLogger(category);
            logger.Log(ToMsLogLevel(level), "[{Category}] {Message}", category, message);
        }
        
        public void Log(LogLevel level, string category, string message, Exception exception)
        {
            if (level < MinLevel)
            {
                return;
            }

            Microsoft.Extensions.Logging.ILogger logger = _loggerFactory.CreateLogger(category);
            logger.Log(ToMsLogLevel(level), exception, "[{Category}] {Message}", category, message);
        }
        
        public ILogger CreateLogger(string category)
        {
            return new ZLoggerWrapper(_loggerFactory.CreateLogger(category), category);
        }
        
        public Core.Core.Logging.ILogger<T> CreateLogger<T>()
        {
            return new ZLoggerWrapper<T>(_loggerFactory.CreateLogger<T>());
        }

        private static MsLogLevel ToMsLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => MsLogLevel.Trace,
                LogLevel.Debug => MsLogLevel.Debug,
                LogLevel.Info => MsLogLevel.Information,
                LogLevel.Warning => MsLogLevel.Warning,
                LogLevel.Error => MsLogLevel.Error,
                LogLevel.Fatal => MsLogLevel.Critical,
                _ => MsLogLevel.None
            };
        }
        
        public void Dispose()
        {
            _loggerFactory?.Dispose();
        }
    }
}
