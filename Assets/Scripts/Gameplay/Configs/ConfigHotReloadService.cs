using System;
using System.IO;
using System.Threading;
using Project.Core.Core.Logging;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Configs
{
    public sealed class ConfigHotReloadService : IStartable, IDisposable
    {
        private readonly ConfigProvider _provider;
        private readonly ILogger _logger;

        private FileSystemWatcher _watcher;
        private int _dirty;

        public bool IsDirty => Volatile.Read(ref _dirty) == 1;

        [Inject]
        private ConfigHotReloadService(
            ConfigProvider provider,
            ILogService log)
        {
            _provider = provider;
            _logger = log.CreateLogger<ConfigHotReloadService>();
        }

        void IStartable.Start()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif

            string path = Path.Combine(
                Application.dataPath,
                "Content/Configs"
            );

            if (!Directory.Exists(path))
            {
                _logger.Warning($"Config dir not found: {path}");
                return;
            }

            _watcher = new FileSystemWatcher(path, "*.json")
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite |
                               NotifyFilters.FileName |
                               NotifyFilters.Size
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.Renamed += OnRenamed;

            _watcher.EnableRaisingEvents = true;

            _logger.Info($"Config watcher started: {path}");
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            MarkDirty(e.Name);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            MarkDirty(e.Name);
        }

        private void MarkDirty(string fileName)
        {
            Interlocked.Exchange(ref _dirty, 1);
            _logger.Info($"Config changed: {fileName}");
        }

        public void ReloadIfDirty()
        {
            if (Interlocked.Exchange(ref _dirty, 0) == 0)
            {
                return;
            }

            _provider.ReloadAll();
            _logger.Info("Configs hot-reload requested");
        }

        public void Dispose()
        {
            if (_watcher == null)
            {
                return;
            }

            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnChanged;
            _watcher.Deleted -= OnChanged;
            _watcher.Renamed -= OnRenamed;
            _watcher.Dispose();
            _watcher = null;
        }
    }
}
