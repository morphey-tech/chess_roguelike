using System;
using System.Collections.Generic;
using Project.Core.Config;
using Project.Core.Logging;
using VContainer;

namespace Project.Gameplay.Config
{
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<Type, object> _configs = new();
        private readonly ILogger _logger;
        
        [Inject]
        public ConfigService(ILogService logService)
        {
            _logger = logService.CreateLogger<ConfigService>();
            _logger.Info("Initialized");
        }
        
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            if (_configs.TryGetValue(type, out object config))
            {
                return config as T;
            }
            
            _logger.Error($"Config not found: {type.Name}");
            return null;
        }
        
        public bool TryGet<T>(out T config) where T : class
        {
            Type type = typeof(T);
            
            if (_configs.TryGetValue(type, out object obj))
            {
                config = obj as T;
                return config != null;
            }
            
            config = null;
            return false;
        }
        
        public void Register<T>(T config) where T : class
        {
            Type type = typeof(T);
            _configs[type] = config;
            _logger.Debug($"Registered: {type.Name}");
        }
    }
}


