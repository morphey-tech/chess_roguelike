using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs;
using Project.Core.Core.Logging;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Configs
{
    public class ConfigProvider : ConfigProviderBase, IDisposable
    {
        /// <summary>
        /// Список ключей конфигов для предзагрузки при старте игры/уровня.
        /// </summary>
        public static readonly IReadOnlyList<string> PreloadConfigKeys = new[]
        {
            "runs_conf",
            "suites_conf",
            "figures_conf",
            "figure_descriptions_conf",
            "stats_conf",
            "passives_conf",
            "spawn_patterns_conf",
            "boards_conf",
            "cells_conf",
            "gameplay_conf",
            "projectiles_conf",
            "stages_conf",
            "conditions_conf",
            "turn_pattern_descriptions_conf",
            "turn_patterns_conf",
            "duels_conf",
            "items_conf",
            "resources_conf",
            "loot_tables_conf"
        };

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

        public async UniTask<T> Get<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (_configsCache.TryGetValue(key, out object? cached))
            {
                if (cached is T typed)
                    return typed;
                if (cached is string json)
                {
                    T config = JsonConvert.DeserializeObject<T>(json, JsonDeserializerSettings)!;
                    _configsCache[key] = config;
                    return config;
                }
            }

            T downloaded = await Download<T>(key, null, cancellationToken);
            _configsCache[key] = downloaded;
            return downloaded;
        }

        public T GetSync<T>(string key) where T : class
        {
            if (_configsCache.TryGetValue(key, out object? cached))
            {
                if (cached is T typed)
                    return typed;
                if (cached is string json)
                {
                    T config = JsonConvert.DeserializeObject<T>(json, JsonDeserializerSettings)!;
                    _configsCache[key] = config;
                    return config;
                }
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
                    _configsCache[key] = json;
                    _logger.Debug($"Config preloaded from Addressables: {key}");
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

        /// <summary>
        /// Предзагружает все конфиги из списка <see cref="PreloadConfigKeys"/>.
        /// </summary>
        public async UniTask PreloadAllAsync(CancellationToken cancellationToken = default)
        {
            await UniTask.WhenAll(PreloadConfigKeys.Select(key 
                => PreloadConfigAsync(key, cancellationToken).AsAsyncUnitUniTask()));
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

        public void ReloadAll()
        {
            ClearCache();
            PreloadAllAsync().Forget();
            _logger.Info("Config cache cleared, preload started");
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