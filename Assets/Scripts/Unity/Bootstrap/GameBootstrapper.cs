using Cysharp.Threading.Tasks;
using Project.Core.Bootstrap;
using Project.Core.Logging;
using Project.Core.Scene;
using UniRx;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private string _mainMenuScene = "MainMenu";
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private float _splashDuration = 1f;

        [Header("UI")]
        [SerializeField] private CanvasGroup _splashScreen;
        [SerializeField] private UnityEngine.UI.Slider _progressBar;

        private IBootstrapService _bootstrapService;
        private ISceneService _sceneService;
        private ILogger _logger;
        private CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(
            IBootstrapService bootstrapService,
            ISceneService sceneService,
            ILogService logService)
        {
            _bootstrapService = bootstrapService;
            _sceneService = sceneService;
            _logger = logService.CreateLogger<GameBootstrapper>();
        }

        private void Start()
        {
            if (_autoStart)
            {
                StartBootstrap().Forget();
            }

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _bootstrapService.OnInitProgress
                .Subscribe(UpdateProgress)
                .AddTo(_disposables);

            _bootstrapService.OnStateChanged
                .Subscribe(OnStateChanged)
                .AddTo(_disposables);

            _sceneService.OnLoadProgress
                .Subscribe(OnSceneLoadProgress)
                .AddTo(_disposables);
        }

        public async UniTaskVoid StartBootstrap()
        {
            _logger.Info("Bootstrap starting...");

            ShowSplash(true);

            await UniTask.Delay(System.TimeSpan.FromSeconds(_splashDuration));

            await _bootstrapService.InitializeAsync();

            await _sceneService.LoadAsync(
                _mainMenuScene,
                SceneLoadParams.Default,
                new SceneTransitionData());

            ShowSplash(false);

            _logger.Info("Bootstrap completed");
        }

        private void UpdateProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }
        }

        private void OnStateChanged(GameState state)
        {
            _logger.Debug($"Game state: {state}");
        }

        private void OnSceneLoadProgress(SceneLoadProgress progress)
        {
            if (_progressBar != null && _splashScreen != null && _splashScreen.alpha > 0)
            {
                _progressBar.value = progress.Progress;
            }
        }

        private void ShowSplash(bool show)
        {
            if (_splashScreen != null)
            {
                _splashScreen.alpha = show ? 1f : 0f;
                _splashScreen.blocksRaycasts = show;
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}

