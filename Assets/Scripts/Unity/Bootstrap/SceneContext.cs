using System;
using Cysharp.Threading.Tasks;
using Project.Core.Logging;
using UnityEngine;
using UnityEngine.Events;
using VContainer;
using VContainer.Unity;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Bootstrap
{
    public class SceneContext : MonoBehaviour
    {
        [Header("Идентификация")]
        [SerializeField] private string _sceneId;
        [SerializeField] private SceneType _sceneType = SceneType.Gameplay;

        [Header("Настройки")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private float _initDelay = 0f;

        [Header("События")]
        [SerializeField] private UnityEvent _onSceneReady;
        [SerializeField] private UnityEvent _onSceneUnloading;

        private ILogger _logger;
        private LifetimeScope _scope;
        private bool _isReady;

        public string SceneId => _sceneId;
        public SceneType Type => _sceneType;
        public bool IsReady => _isReady;
        public LifetimeScope Scope => _scope;

        public event Action OnReady;
        public event Action OnUnloading;

        [Inject]
        public void Construct(ILogService logService)
        {
            _logger = logService.CreateLogger<SceneContext>();
        }

        private async void Start()
        {
            _scope = GetComponentInParent<LifetimeScope>();

            if (_autoInitialize)
            {
                await InitializeAsync();
            }
        }

        public async UniTask InitializeAsync()
        {
            if (_isReady)
            {
                _logger?.Warning($"Scene already initialized: {_sceneId}");
                return;
            }

            _logger?.Info($"Initializing scene: {_sceneId}");

            if (_initDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_initDelay));
            }

            await OnInitializeAsync();

            _isReady = true;
            _onSceneReady?.Invoke();
            OnReady?.Invoke();

            _logger?.Info($"Scene ready: {_sceneId}");
        }

        protected virtual UniTask OnInitializeAsync()
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnSceneWillUnload()
        {
        }

        private void OnDestroy()
        {
            if (_isReady)
            {
                _logger?.Debug($"Scene unloading: {_sceneId}");
                OnSceneWillUnload();
                _onSceneUnloading?.Invoke();
                OnUnloading?.Invoke();
            }
        }

        private void Reset()
        {
            _sceneId = gameObject.scene.name;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_sceneId))
            {
                _sceneId = gameObject.scene.name;
            }
        }
    }
}

