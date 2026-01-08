using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Assets;
using Project.Core.Config;
using Project.Core.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;
using Object = UnityEngine.Object;

namespace Project.Gameplay.Config
{
    /// <summary>
    /// Загружает конфиги из Addressables и регистрирует их в ConfigService.
    /// Конфиги должны быть ScriptableObject с адресом = имя типа (например "GameConfig").
    /// </summary>
    public class ConfigProvider : IConfigProvider, IDisposable
    {
        private readonly IAssetService _assetService;
        private readonly IConfigService _configService;
        private readonly ILogger _logger;
        private readonly Dictionary<Type, AsyncOperationHandle> _loadedHandles = new();
        private bool _disposed;
        
        [Inject]
        public ConfigProvider(
            IAssetService assetService,
            IConfigService configService,
            ILogService logService)
        {
            _assetService = assetService;
            _configService = configService;
            _logger = logService.CreateLogger<ConfigProvider>();
        }
        
        public async UniTask<T> LoadAsync<T>(string address) where T : Object
        {
            ThrowIfDisposed();
            
            if (string.IsNullOrEmpty(address))
            {
                _logger.Error("Config address is null or empty");
                return null;
            }
            
            Type type = typeof(T);
            
            // Проверяем не загружен ли уже
            if (_loadedHandles.TryGetValue(type, out var existingHandle) && 
                existingHandle.IsValid() && 
                existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return existingHandle.Result as T;
            }
            
            try
            {
                _logger.Debug($"Loading config: {address} ({type.Name})");
                
                var handle = Addressables.LoadAssetAsync<T>(address);
                await handle.ToUniTask();
                
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    _logger.Error($"Failed to load config: {address}");
                    return null;
                }
                
                _loadedHandles[type] = handle;
                _logger.Info($"Config loaded: {address}");
                
                return handle.Result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception loading config {address}", ex);
                return null;
            }
        }
        
        public UniTask<T> LoadAsync<T>() where T : Object
        {
            // Адрес по умолчанию = имя типа без "Asset" суффикса
            string typeName = typeof(T).Name;
            string address = typeName.EndsWith("Asset") 
                ? typeName[..^5]  // Убираем "Asset"
                : typeName;
            
            return LoadAsync<T>(address);
        }
        
        public async UniTask LoadAllByLabelAsync(string label)
        {
            ThrowIfDisposed();
            
            _logger.Info($"Loading all configs with label: {label}");
            
            try
            {
                var handle = Addressables.LoadAssetsAsync<ScriptableObject>(
                    label, 
                    config =>
                    {
                        if (config != null)
                        {
                            RegisterConfig(config);
                        }
                    });
                
                await handle.ToUniTask();
                
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _logger.Info($"Loaded {handle.Result.Count} configs with label '{label}'");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception loading configs by label {label}", ex);
            }
        }
        
        public async UniTask<T> LoadAndRegisterAsync<T>(string address = null) where T : Object
        {
            T config = string.IsNullOrEmpty(address) 
                ? await LoadAsync<T>() 
                : await LoadAsync<T>(address);
            
            if (config != null)
            {
                _configService.Register(config);
            }
            
            return config;
        }
        
        public bool IsLoaded<T>() where T : class
        {
            return _configService.TryGet<T>(out _);
        }
        
        private void RegisterConfig(ScriptableObject config)
        {
            // Регистрируем по типу самого конфига
            Type configType = config.GetType();
            
            // Используем reflection чтобы вызвать Register<T> с правильным типом
            var registerMethod = typeof(IConfigService)
                .GetMethod(nameof(IConfigService.Register))
                ?.MakeGenericMethod(configType);
            
            registerMethod?.Invoke(_configService, new object[] { config });
            
            _logger.Debug($"Registered config: {configType.Name}");
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ConfigProvider));
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            foreach (var handle in _loadedHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadedHandles.Clear();
            
            _logger.Debug("Disposed");
        }
    }
}

