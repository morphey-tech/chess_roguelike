using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Bootstrap;
using Project.Core.Logging;
using Project.Core.Scene;
using UniRx;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Bootstrap
{
    public class BootstrapService : IBootstrapService, IDisposable
    {
        public GameState CurrentState { get; private set; } = GameState.None;
        public IObservable<GameState> OnStateChanged => _onStateChanged;
        public IObservable<float> OnInitProgress => _onInitProgress;
        
        private readonly ISceneService _sceneService;
        private readonly ILogger _logger;
        private readonly List<IInitializable> _initializables = new();

        private readonly Subject<GameState> _onStateChanged = new();
        private readonly Subject<float> _onInitProgress = new();

        private bool _disposed;

        [Inject]
        public BootstrapService(ISceneService sceneService, ILogService logService)
        {
            _sceneService = sceneService;
            _logger = logService.CreateLogger<BootstrapService>();
            _logger.Info("Initialized");
        }

        public void RegisterInitializable(IInitializable initializable)
        {
            _initializables.Add(initializable);
        }

        public async UniTask InitializeAsync()
        {
            ThrowIfDisposed();

            if (CurrentState != GameState.None)
            {
                _logger.Warning("Already initialized");
                return;
            }

            SetState(GameState.Initializing);
            _logger.Info("Starting initialization...");

            float totalSteps = _initializables.Count + 1;
            int currentStep = 0;

            foreach (IInitializable initializable in _initializables)
            {
                try
                {
                    await initializable.InitializeAsync();
                    currentStep++;
                    _onInitProgress.OnNext(currentStep / totalSteps);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to initialize: {initializable.GetType().Name}", ex);
                    throw;
                }
            }

            _onInitProgress.OnNext(1f);
            _logger.Info("Initialization completed");

            SetState(GameState.MainMenu);
        }

        public async UniTask StartGameAsync()
        {
            ThrowIfDisposed();

            if (CurrentState != GameState.MainMenu)
            {
                _logger.Warning($"Cannot start game from state: {CurrentState}");
                return;
            }

            SetState(GameState.Loading);
            _logger.Info("Starting game...");

            await _sceneService.LoadAsync(
                SceneNames.Game,
                SceneLoadParams.Default,
                new SceneTransitionData());

            SetState(GameState.Playing);
            _logger.Info("Game started");
        }

        public async UniTask ReturnToMainMenuAsync()
        {
            ThrowIfDisposed();

            if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
            {
                _logger.Warning($"Cannot return to menu from state: {CurrentState}");
                return;
            }

            SetState(GameState.Loading);
            _logger.Info("Returning to main menu...");

            await _sceneService.LoadAsync(
                SceneNames.MainMenu,
                SceneLoadParams.Default,
                new SceneTransitionData());

            SetState(GameState.MainMenu);
            _logger.Info("Returned to main menu");
        }

        public async UniTask QuitGameAsync()
        {
            ThrowIfDisposed();

            SetState(GameState.Quitting);
            _logger.Info("Quitting game...");

            await UniTask.Delay(100);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetPaused(bool paused)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
            {
                return;
            }

            SetState(paused ? GameState.Paused : GameState.Playing);
            Time.timeScale = paused ? 0f : 1f;

            _logger.Debug($"Game {(paused ? "paused" : "resumed")}");
        }

        private void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState previousState = CurrentState;
            CurrentState = newState;

            _logger.Debug($"State: {previousState} -> {newState}");
            _onStateChanged.OnNext(newState);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BootstrapService));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _onStateChanged.Dispose();
            _onInitProgress.Dispose();
            _initializables.Clear();

            _logger.Info("Disposed");
        }
    }
}

