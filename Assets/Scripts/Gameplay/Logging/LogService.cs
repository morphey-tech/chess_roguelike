using System;
using Microsoft.Extensions.Logging;
using Project.Core.Logging;
using ZLogger.Unity;
using ILogger = Project.Core.Logging.ILogger;
using LogLevel = Project.Core.Logging.LogLevel;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Project.Gameplay.Logging
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
            if (level < MinLevel) return;
            
            var logger = _loggerFactory.CreateLogger(category);
            logger.Log(ToMsLogLevel(level), "[{Category}] {Message}", category, message);
        }
        
        public void Log(LogLevel level, string category, string message, Exception exception)
        {
            if (level < MinLevel) return;
            
            var logger = _loggerFactory.CreateLogger(category);
            logger.Log(ToMsLogLevel(level), exception, "[{Category}] {Message}", category, message);
        }
        
        public ILogger CreateLogger(string category)
        {
            return new ZLoggerWrapper(_loggerFactory.CreateLogger(category), category);
        }
        
        public Core.Logging.ILogger<T> CreateLogger<T>()
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
    
    /// <summary>
    /// Обёртка над ZLogger для нашего интерфейса.
    /// Использует стандартные методы логирования (без C# 10 interpolation).
    /// </summary>
    public class ZLoggerWrapper : ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        
        public string Category { get; }
        
        public ZLoggerWrapper(Microsoft.Extensions.Logging.ILogger logger, string category)
        {
            _logger = logger;
            Category = category;
        }
        
        public void Trace(string message) => _logger.LogTrace("[{Category}] {Message}", Category, message);
        public void Debug(string message) => _logger.LogDebug("[{Category}] {Message}", Category, message);
        public void Info(string message) => _logger.LogInformation("[{Category}] {Message}", Category, message);
        public void Warning(string message) => _logger.LogWarning("[{Category}] {Message}", Category, message);
        public void Error(string message) => _logger.LogError("[{Category}] {Message}", Category, message);
        public void Error(string message, Exception ex) => _logger.LogError(ex, "[{Category}] {Message}", Category, message);
        public void Fatal(string message) => _logger.LogCritical("[{Category}] {Message}", Category, message);
        public void Fatal(string message, Exception ex) => _logger.LogCritical(ex, "[{Category}] {Message}", Category, message);
    }
    
    /// <summary>
    /// Типизированная обёртка
    /// </summary>
    public class ZLoggerWrapper<T> : ZLoggerWrapper, Core.Logging.ILogger<T>
    {
        public ZLoggerWrapper(Microsoft.Extensions.Logging.ILogger<T> logger) 
            : base(logger, typeof(T).Name)
        {
        }
    }
}
