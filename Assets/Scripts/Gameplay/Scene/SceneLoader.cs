using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Logging;
using Project.Core.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Scene
{
    public sealed class SceneLoader
    {
        private readonly ILogger _logger;

        [Inject]
        private SceneLoader(ILogService logService)
        {
            _logger = logService.CreateLogger<SceneLoader>();
        }

        public async UniTask LoadAdditiveAsync(
            string sceneName,
            IObserver<SceneLoadProgress> progress,
            CancellationToken cancellationToken)
        {
            _logger.Debug($"Load additive: {sceneName}");

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
                ?? throw new InvalidOperationException($"Failed to start loading scene {sceneName}");

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                cancellationToken.ThrowIfCancellationRequested();

                float value = Mathf.Lerp(0.1f, 0.6f, operation.progress / 0.9f);
                progress.OnNext(new SceneLoadProgress(sceneName, value, SceneLoadPhase.Loading));

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            progress.OnNext(new SceneLoadProgress(sceneName, 0.65f, SceneLoadPhase.Activating));

            operation.allowSceneActivation = true;
            await operation.ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask UnloadAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);

            _logger.Debug($"Trying to unload: '{sceneName}', isValid={scene.IsValid()}, isLoaded={scene.isLoaded}");
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                _logger.Debug($"  Loaded scene [{i}]: '{s.name}' (isLoaded={s.isLoaded})");
            }

            if (!scene.isLoaded)
            {
                _logger.Warning($"Scene not loaded, skip unload: '{sceneName}'");
                return;
            }

            _logger.Info($"Unloading scene: '{sceneName}'");

            AsyncOperation operation =
                SceneManager.UnloadSceneAsync(scene)
                ?? throw new InvalidOperationException($"Failed to start unloading scene {sceneName}");

            await operation.ToUniTask(cancellationToken: cancellationToken);
            
            _logger.Info($"Scene unloaded: '{sceneName}'");
        }

        public static void SetActive(string sceneName)
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.isLoaded)
            {
                throw new InvalidOperationException($"Scene {sceneName} is not loaded");
            }

            SceneManager.SetActiveScene(scene);
        }

        public bool IsLoaded(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).isLoaded;
        }
    }
}