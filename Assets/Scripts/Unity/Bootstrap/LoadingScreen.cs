using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Scene;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Unity.Unity.Bootstrap
{
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _percentText;

        [Header("Анимация")]
        [SerializeField] private float _fadeSpeed = 2f;
        [SerializeField] private float _minDisplayTime = 0.5f;

        private ISceneService _sceneService;
        private CompositeDisposable _disposables = new();

        private bool _isVisible;
        private float _showTime;

        [Inject]
        public void Construct(ISceneService sceneService)
        {
            _sceneService = sceneService;
        }

        private void Start()
        {
            Hide(true);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _sceneService.OnLoadProgress
                .Subscribe(OnLoadProgress)
                .AddTo(_disposables);
        }

        private void OnLoadProgress(SceneLoadProgress progress)
        {
            if (progress.Phase == SceneLoadPhase.Starting)
            {
                Show().Forget();
            }

            UpdateProgress(progress);

            if (progress.Phase == SceneLoadPhase.Completed)
            {
                HideWithDelay().Forget();
            }
        }

        private void UpdateProgress(SceneLoadProgress progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress.Progress;
            }

            if (_percentText != null)
            {
                _percentText.text = $"{Mathf.RoundToInt(progress.Progress * 100)}%";
            }

            if (_statusText != null)
            {
                _statusText.text = GetPhaseText(progress.Phase);
            }
        }

        private string GetPhaseText(SceneLoadPhase phase)
        {
            return phase switch
            {
                SceneLoadPhase.Starting => "Подготовка...",
                SceneLoadPhase.UnloadingPrevious => "Выгрузка...",
                SceneLoadPhase.Loading => "Загрузка...",
                SceneLoadPhase.Activating => "Активация...",
                SceneLoadPhase.Initializing => "Инициализация...",
                SceneLoadPhase.Completed => "Готово!",
                _ => ""
            };
        }

        public async UniTaskVoid Show()
        {
            if (_isVisible) return;

            _isVisible = true;
            _showTime = Time.realtimeSinceStartup;
            _canvasGroup.blocksRaycasts = true;

            while (_canvasGroup.alpha < 1f)
            {
                _canvasGroup.alpha += _fadeSpeed * Time.unscaledDeltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 1f;
        }

        private async UniTaskVoid HideWithDelay()
        {
            float elapsed = Time.realtimeSinceStartup - _showTime;
            if (elapsed < _minDisplayTime)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_minDisplayTime - elapsed), ignoreTimeScale: true);
            }

            Hide(false).Forget();
        }

        public async UniTaskVoid Hide(bool instant = false)
        {
            if (!_isVisible && !instant) return;

            _isVisible = false;
            _canvasGroup.blocksRaycasts = false;

            if (instant)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            while (_canvasGroup.alpha > 0f)
            {
                _canvasGroup.alpha -= _fadeSpeed * Time.unscaledDeltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}

