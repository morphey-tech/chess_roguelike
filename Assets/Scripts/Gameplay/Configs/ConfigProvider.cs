using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Gameplay.Configs
{
    public class ConfigProvider : ConfigProviderBase
    {
        private readonly Dictionary<string, object> _configsCache = new();
        private readonly Dictionary<string, UniTask> _preloadTasks = new();

        public async UniTask<T> Get<T>(string key, CancellationToken cancellationToken) where T : class
        {
            if (_configsCache.TryGetValue(key, out object? cachedConfig))
            {
                return cachedConfig as T;
            }
            T? config = await Download<T>(key, null, cancellationToken);
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


        public async UniTask<T> Get<T>(CancellationToken cancellationToken) where T : class
        {
            string key = GetConfigKey<T>();
            return await Get<T>(key, cancellationToken);
        }

        public T GetSync<T>() where T : class
        {
            string key = GetConfigKey<T>();
            return GetSync<T>(key);
        }

        private static string GetConfigKey<T>()
        {
            return $"conf_{typeof(T).Name.ToLower()}";
        }

        public async UniTask PreloadAllConfigs(IEnumerable<string> keys, CancellationToken cancellationToken)
        {
            List<UniTask> preloadTasks = new List<UniTask>();
            foreach (string? key in keys)
            {
                preloadTasks.Add(PreloadConfigAsync(key, cancellationToken));
            }

            await UniTask.WhenAll(preloadTasks);
        }

        public async UniTask PreloadConfigAsync(string key, CancellationToken cancellationToken)
        {
            if (_preloadTasks.ContainsKey(key))
            {
                return;
            }

            _preloadTasks[key] = InnerPreloadConfigAsync(key, cancellationToken);
            await _preloadTasks[key];
        }

        private async UniTask InnerPreloadConfigAsync(string key, CancellationToken cancellationToken)
        {
            if (_configsCache.ContainsKey(key))
            {
                return;
            }

            try
            {
                object? config = await Download<object>(key, null, cancellationToken);
                _configsCache[key] = config;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error preloading config {key}", ex);
            }
        }

        public void ClearCache()
        {
            _configsCache.Clear();
            _preloadTasks.Clear();
        }

        protected override async UniTask<T> InnerDownload<T>(string key, T storage, CancellationToken cancellationToken)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Config file for key {key} not found at {filePath}");
                }

                string json = await File.ReadAllTextAsync(filePath, cancellationToken);
                T config = JsonConvert.DeserializeObject<T>(json, JsonDeserializerSettings);
                return config;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error downloading config {key}", ex);
            }
        }

        private static string GetFilePath(string key)
        {
            return Path.Combine("Configs", $"{key}.json");
        }

        protected override async UniTask InnerUpload(string key, object config, CancellationToken cancellationToken)
        {
            string filePath = GetFilePath(key);
            string json = JsonConvert.SerializeObject(config, JsonSerializerSettings);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
    }
}