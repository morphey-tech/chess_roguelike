using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Project.Core.Assets;
using Project.Core.Core.Configs;
using Project.Core.Logging;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Configs
{
    public class ConfigProvider : ConfigProviderBase, IDisposable
    {
        private readonly Dictionary<string, object> _configsCache = new();
        private readonly HashSet<string> _preloadedAddresses = new();
        private readonly IAssetService _assetService;
        private readonly ILogger _logger;
        private bool _disposed;

        [Inject]
        public ConfigProvider(IAssetService assetService, ILogService logService)
        {
            _assetService = assetService;
            _logger = logService.CreateLogger<ConfigProvider>();
        }

        public async UniTask<T> Get<T>(string key, CancellationToken cancellationToken) where T : class
        {
            if (_configsCache.TryGetValue(key, out object? cachedConfig))
            {
                return cachedConfig as T;
            }

            T config = await Download<T>(key, null, cancellationToken);
            _configsCache[key] = config;
            return config;
        }

        public T GetSync<T>(string key) where T : class
        {
            if (_configsCache.TryGetValue(key, out object? cachedConfig))
            {
                return cachedConfig as T;
            }
            throw new ApplicationException($"Config for key {key} not loaded yet.");
        }

        public async UniTask PreloadConfigAsync(string key, CancellationToken cancellationToken)
        {
            if (!_preloadedAddresses.Add(key))
            {
                return;
            }
            await LoadConfigAsync(key, cancellationToken);
        }

        private async UniTask LoadConfigAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                var textAsset = await _assetService.LoadAssetAsync<TextAsset>(key);
                if (textAsset != null)
                {
                    string json = textAsset.text;
                    var config = JsonConvert.DeserializeObject<object>(json, JsonDeserializerSettings);  // Применение конвертеров
                    _configsCache[key] = config;
                    _logger.Debug($"Config loaded from Addressables: {key}");
                }
                else
                {
                    _logger.Error($"Failed to load config from Addressables: {key}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading config {key} from Addressables", ex);
            }
        }

        protected override async UniTask<T> InnerDownload<T>(string key, T storage, CancellationToken cancellationToken)
        {
            try
            {
                TextAsset? textAsset = await _assetService.LoadAssetAsync<TextAsset>(key);
                if (textAsset != null)
                {
                    string json = textAsset.text;
                    T config = JsonConvert.DeserializeObject<T>(json, JsonDeserializerSettings);
                    return config;
                }

                throw new ApplicationException($"Failed to load config {key} from Addressables");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error downloading config {key}", ex);
            }
        }

        public void ClearCache()
        {
            _configsCache.Clear();
            _preloadedAddresses.Clear();
        }

        void IDisposable.Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            ClearCache();
            _logger.Info("Disposing ConfigProvider");
        }
    }
}