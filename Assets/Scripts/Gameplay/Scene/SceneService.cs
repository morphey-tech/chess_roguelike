using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Core.Scene;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Scene
{
    public sealed class SceneService : ISceneService, IDisposable
    {
        public bool IsLoading { get; private set; }
        public string CurrentScene { get; private set; }

        public IObservable<SceneLoadProgress> OnLoadProgress => _progress;

        private readonly SceneTransitionService _transitionService;
        private readonly ILogger _logger;

        private readonly Subject<SceneLoadProgress> _progress = new();
        private CancellationTokenSource? _loadCts;
        private bool _disposed;

        private const string BootstrapSceneName = "Bootstrap";

        public SceneService(
            SceneTransitionService transitionService,
            ILogService logService)
        {
            _transitionService = transitionService;
            _logger = logService.CreateLogger<SceneService>();
            CurrentScene = SceneManager.GetActiveScene().name;
        }

        public async UniTask LoadAsync(
            string targetScene,
            SceneLoadParams loadParams,
            SceneTransitionData transitionData)
        {
            ThrowIfDisposed();

            if (IsLoading)
            {
                _logger.Warning("Scene load already in progress");
                return;
            }

            IsLoading = true;

            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();

            try
            {
                string fromScene = SceneManager.GetActiveScene().name;
                _logger.Debug($"Loading: {fromScene} → {targetScene}");

                bool useBootstrap = SceneExists(BootstrapSceneName) 
                                    && targetScene != BootstrapSceneName;

                if (useBootstrap)
                {
                    transitionData.Set(targetScene);
                    await LoadInternalAsync(
                        fromScene,
                        BootstrapSceneName,
                        loadParams,
                        transitionData,
                        _loadCts.Token);
                }
                else
                {
                    await LoadInternalAsync(
                        fromScene,
                        targetScene,
                        loadParams,
                        transitionData,
                        _loadCts.Token);
                }

                CurrentScene = targetScene;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async UniTask LoadInternalAsync(
            string fromScene,
            string targetScene,
            SceneLoadParams loadParams,
            SceneTransitionData transitionData,
            CancellationToken token)
        {
            await _transitionService.ExecuteAsync(
                fromScene: fromScene,
                toScene: targetScene,
                loadParams: loadParams,
                transitionData: transitionData,
                progress: _progress,
                cancellationToken: token);
        }

        private static bool SceneExists(string sceneName)
        {
            return Application.CanStreamedLevelBeLoaded(sceneName);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SceneService));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _loadCts?.Cancel();
            _loadCts?.Dispose();

            _progress.OnCompleted();
            _progress.Dispose();
        }
    }
}

