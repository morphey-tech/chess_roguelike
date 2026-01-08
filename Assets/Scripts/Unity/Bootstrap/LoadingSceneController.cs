using System;
using Cysharp.Threading.Tasks;
using Project.Core.Logging;
using Project.Core.Scene;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Bootstrap
{
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _percentText;
        [SerializeField] private Text _tipsText;

        [Header("Визуал")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite[] _backgroundVariants;
        [SerializeField] private CanvasGroup _contentGroup;

        [Header("Подсказки")]
        [SerializeField] private string[] _loadingTips;
        [SerializeField] private float _tipChangeInterval = 3f;

        [Header("Настройки")]
        [SerializeField] private float _minLoadingTime = 1.5f;
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.3f;

        private ISceneService _sceneService;
        private ILogger _logger;
        private CompositeDisposable _disposables = new();

        private float _currentProgress;
        private float _displayProgress;
        private float _loadStartTime;
        private bool _loadingComplete;

        [Inject]
        public void Construct(ISceneService sceneService, ILogService logService)
        {
            _sceneService = sceneService;
            _logger = logService.CreateLogger<LoadingSceneController>();
        }

        private async void Start()
        {
            _logger.Info("Loading scene started");
            _loadStartTime = Time.realtimeSinceStartup;

            SetupVisuals();
            SubscribeToProgress();
            StartTipsRotation();

            await FadeIn();
            await LoadGameSceneAsync();
        }

        private void SetupVisuals()
        {
            if (_backgroundImage != null && _backgroundVariants != null && _backgroundVariants.Length > 0)
            {
                _backgroundImage.sprite = _backgroundVariants[UnityEngine.Random.Range(0, _backgroundVariants.Length)];
            }

            if (_contentGroup != null)
            {
                _contentGroup.alpha = 0f;
            }

            UpdateProgressUI(0f, SceneLoadPhase.Starting);
        }

        private void SubscribeToProgress()
        {
            _sceneService.OnLoadProgress
                .Subscribe(OnLoadProgress)
                .AddTo(_disposables);
        }

        private void StartTipsRotation()
        {
            if (_tipsText == null || _loadingTips == null || _loadingTips.Length == 0) return;

            int tipIndex = 0;
            ShowTip(tipIndex);

            Observable.Interval(TimeSpan.FromSeconds(_tipChangeInterval))
                .Subscribe(_ =>
                {
                    tipIndex = (tipIndex + 1) % _loadingTips.Length;
                    ShowTip(tipIndex);
                })
                .AddTo(_disposables);
        }

        private void ShowTip(int index)
        {
            if (_tipsText != null && index < _loadingTips.Length)
            {
                _tipsText.text = _loadingTips[index];
            }
        }

        private void OnLoadProgress(SceneLoadProgress progress)
        {
            _currentProgress = progress.Progress;
            UpdateProgressUI(progress.Progress, progress.Phase);

            if (progress.Phase == SceneLoadPhase.Completed)
            {
                _loadingComplete = true;
            }
        }

        private void Update()
        {
            _displayProgress = Mathf.MoveTowards(_displayProgress, _currentProgress, Time.deltaTime * 2f);

            if (_progressBar != null)
            {
                _progressBar.value = _displayProgress;
            }
        }

        private void UpdateProgressUI(float progress, SceneLoadPhase phase)
        {
            if (_percentText != null)
            {
                _percentText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }

            if (_statusText != null)
            {
                _statusText.text = GetPhaseText(phase);
            }
        }

        private string GetPhaseText(SceneLoadPhase phase)
        {
            return phase switch
            {
                SceneLoadPhase.Starting => "Подготовка...",
                SceneLoadPhase.UnloadingPrevious => "Очистка памяти...",
                SceneLoadPhase.Loading => "Загрузка ресурсов...",
                SceneLoadPhase.Activating => "Активация сцены...",
                SceneLoadPhase.Initializing => "Инициализация...",
                SceneLoadPhase.Completed => "Готово!",
                _ => "Загрузка..."
            };
        }

        private async UniTask LoadGameSceneAsync()
        {
            SceneLoadParams loadParams = new SceneLoadParams(
                showLoadingScreen: false,
                unloadPrevious: false,
                minLoadTime: 0f
            );

            UniTask loadTask = _sceneService.LoadAsync(
                SceneNames.Game,
                loadParams,
                new SceneTransitionData());

            await loadTask;
            float elapsed = Time.realtimeSinceStartup - _loadStartTime;
            if (elapsed < _minLoadingTime)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_minLoadingTime - elapsed));
            }
            await FadeOut();
        }

        private async UniTask FadeIn()
        {
            if (_contentGroup == null)
            {
                return;
            }

            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _contentGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                await UniTask.Yield();
            }
            _contentGroup.alpha = 1f;
        }

        private async UniTask FadeOut()
        {
            if (_contentGroup == null)
            {
                return;
            }

            float elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _contentGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeOutDuration);
                await UniTask.Yield();
            }
            _contentGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}

