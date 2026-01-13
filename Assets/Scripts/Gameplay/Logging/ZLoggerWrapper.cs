using System;
using Project.Core.Core.Logging;
using LoggerExtensions = Microsoft.Extensions.Logging.LoggerExtensions;

namespace Project.Gameplay.Gameplay.Logging
{
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
        
        public void Trace(string message) => LoggerExtensions.LogTrace(_logger, "[{Category}] {Message}", Category, message);
        public void Debug(string message) => LoggerExtensions.LogDebug(_logger, "[{Category}] {Message}", Category, message);
        public void Info(string message) => LoggerExtensions.LogInformation(_logger, "[{Category}] {Message}", Category, message);
        public void Warning(string message) => LoggerExtensions.LogWarning(_logger, "[{Category}] {Message}", Category, message);
        public void Error(string message) => LoggerExtensions.LogError(_logger, "[{Category}] {Message}", Category, message);
        public void Error(string message, Exception ex) => LoggerExtensions.LogError(_logger, ex, "[{Category}] {Message}", Category, message);
        public void Fatal(string message) => LoggerExtensions.LogCritical(_logger, "[{Category}] {Message}", Category, message);
        public void Fatal(string message, Exception ex) => LoggerExtensions.LogCritical(_logger, ex, "[{Category}] {Message}", Category, message);
    }
}