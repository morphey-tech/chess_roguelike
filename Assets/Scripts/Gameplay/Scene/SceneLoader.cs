using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Core.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Scene
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

        public async UniTask UnloadAsync(
            UnityEngine.SceneManagement.Scene scene,
            CancellationToken cancellationToken)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                _logger.Warning($"Scene not loaded, skip unload: '{scene.name}'");
                return;
            }

            _logger.Info($"Unloading scene: '{scene.name}'");

            AsyncOperation operation =
                SceneManager.UnloadSceneAsync(scene)
                ?? throw new InvalidOperationException($"Failed to start unloading scene {scene.name}");

            await operation.ToUniTask(cancellationToken: cancellationToken);

            _logger.Info($"Scene unloaded: '{scene.name}'");
        }

        public async UniTask UnloadAllExcept(
            UnityEngine.SceneManagement.Scene keepScene,
            CancellationToken cancellationToken)
        {
            var toUnload = new List<UnityEngine.SceneManagement.Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }
                if (scene.handle == keepScene.handle)
                {
                    continue;
                }
                if (scene.name == "DontDestroyOnLoad")
                {
                    continue;
                }

                toUnload.Add(scene);
            }

            foreach (UnityEngine.SceneManagement.Scene scene in toUnload)
            {
                await UnloadAsync(scene, cancellationToken);
            }
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