using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Bootstrap;
using Project.Core.Core.Logging;
using Project.Core.Core.Scene;
using Project.Gameplay.Gameplay.Memory;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Scene
{
    public sealed class SceneTransitionService
    {
        private readonly SceneLoader _sceneLoader;
        private readonly MemoryCleanService _memoryCleanService;
        private readonly IObjectResolver _resolver;
        private readonly ILogger _logger;

        public SceneTransitionService(
            SceneLoader sceneLoader,
            MemoryCleanService memoryCleanService,
            IObjectResolver resolver,
            ILogService logService)
        {
            _sceneLoader = sceneLoader;
            _memoryCleanService = memoryCleanService;
            _resolver = resolver;
            _logger = logService.CreateLogger<SceneTransitionService>();
        }

        public async UniTask ExecuteAsync(
            string fromScene,
            string toScene,
            SceneLoadParams loadParams,
            SceneTransitionData transitionData,
            IObserver<SceneLoadProgress> progress,
            CancellationToken cancellationToken)
        {
            _logger.Info($"Transition {fromScene} → {toScene}");

            float startTime = Time.realtimeSinceStartup;

            progress.OnNext(new SceneLoadProgress(toScene, 0f, SceneLoadPhase.Starting));
            await LoadSceneAsync(toScene, transitionData, progress, cancellationToken);
            // Active scene уже установлена в LoadSceneAsync

            if (loadParams.UnloadPrevious && fromScene != toScene)
            {
                progress.OnNext(new SceneLoadProgress(
                    toScene,
                    0.8f,
                    SceneLoadPhase.UnloadingPrevious));

                await _sceneLoader.UnloadAsync(fromScene, cancellationToken);

                if (loadParams.CanDoHeavyCleanup)
                {
                    await _memoryCleanService.CleanMemory();
                }
            }

            await EnsureMinLoadTime(startTime, loadParams.MinLoadTime, cancellationToken);
            progress.OnNext(new SceneLoadProgress(toScene, 1f, SceneLoadPhase.Completed));
        }

        private async UniTask LoadSceneAsync(
            string sceneName,
            SceneTransitionData transitionData,
            IObserver<SceneLoadProgress> progress,
            CancellationToken cancellationToken)
        {
            await _sceneLoader.LoadAdditiveAsync(sceneName, progress, cancellationToken);
            UnityEngine.SceneManagement.Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            
            SceneManager.SetActiveScene(loadedScene);
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            
            IObjectResolver? sceneResolver = FindLifetimeScopeInScene(loadedScene)?.Container;
            IObjectResolver resolverToUse = sceneResolver ?? _resolver;
            
            (ISceneBootstrap? bootstrap, Component? component) = FindBootstrapInScene(loadedScene);
            
            if (bootstrap != null)
            {
                _logger.Debug($"Found bootstrap: {bootstrap.GetType().Name}");
                
                if (component != null)
                {
                    resolverToUse.Inject(component);
                }
                
                try
                {
                    await bootstrap.BootstrapAsync(transitionData);
                }
                catch (Exception e)
                {
                    // Log but don't re-throw — let the transition service finish
                    // scene unloading. Re-throwing here would leave the previous
                    // scene loaded, causing scene stacking (overlapping UI, duplicate
                    // EventSystem / AudioListener, etc.).
                    _logger.Error($"Bootstrap failed: {bootstrap.GetType().Name}", e);
                }
            }
            else
            {
                _logger.Debug($"No bootstrap found in scene: {sceneName}");
            }
        }

        private static LifetimeScope? FindLifetimeScopeInScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (!scene.IsValid()) return null;
            
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                LifetimeScope? scope = root.GetComponentInChildren<LifetimeScope>(true);
                if (scope != null) return scope;
            }
            
            return null;
        }

        private static (ISceneBootstrap? bootstrap, Component? component) FindBootstrapInScene(
            UnityEngine.SceneManagement.Scene scene)
        {
            if (!scene.IsValid())
            {
                return (null, null);
            }
            
            foreach (GameObject? root in scene.GetRootGameObjects())
            {
                ISceneBootstrap? component = root.GetComponentInChildren<ISceneBootstrap>(true);
                if (component != null)
                {
                    return (component, component as Component);
                }
            }
            
            return (null, null);
        }

        private static async UniTask EnsureMinLoadTime(
            float startTime,
            float minLoadTime,
            CancellationToken token)
        {
            if (minLoadTime <= 0f)
            {
                return;
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < minLoadTime)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(minLoadTime - elapsed),
                    cancellationToken: token);
            }
        }
    }
}

