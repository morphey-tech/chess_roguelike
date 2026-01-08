using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Logging;
using Project.Core.Scene;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace Project.Unity.Bootstrap
{
    /// <summary>
    /// Контроллер сцены инициализации — показывает логотипы и переходит в меню.
    /// </summary>
    public class InitSceneController : MonoBehaviour
    {
        [Header("Логотипы")]
        [SerializeField] private List<LogoData> _logos = new();
        [SerializeField] private CanvasGroup _logoCanvasGroup;
        [SerializeField] private Image _logoImage;

        [Header("Тайминги по умолчанию")]
        [SerializeField] private float _defaultFadeInDuration = 0.5f;
        [SerializeField] private float _defaultDisplayDuration = 2f;
        [SerializeField] private float _defaultFadeOutDuration = 0.5f;
        [SerializeField] private float _delayBetweenLogos = 0.2f;
        [SerializeField] private float _delayBeforeNextScene = 0.3f;

        [Header("Переход")]
        [SerializeField] private string _nextSceneName = "MenuScene";

        [Header("Управление")]
        [SerializeField] private bool _allowSkip = true;

        [Header("Аудио")]
        [SerializeField] private AudioSource _audioSource;

        private ISceneService _sceneService = null!;
        private ILogger<InitSceneController> _logger = null!;
        
        private bool _skipCurrentLogo;
        private bool _isShowingLogo;
        private int _currentLogoIndex;

        [Inject]
        private void Construct(ISceneService sceneService, ILogService logService)
        {
            _sceneService = sceneService;
            _logger = logService.CreateLogger<InitSceneController>();
        }

        private void Start()
        {
            _logger.Info("Init scene started");
            if (_logoCanvasGroup != null)
            {
                _logoCanvasGroup.alpha = 0f;
            }
            RunInitSequenceAsync().Forget();
        }

        private void Update()
        {
            if (!_allowSkip || !_isShowingLogo)
            {
                return;
            }
            bool keyPressed = Keyboard.current.anyKey.wasPressedThisFrame;
            if (keyPressed)
            {
                _skipCurrentLogo = true;
                _logger.Debug($"Logo {_currentLogoIndex + 1} skipped by user");
            }
        }

        private async UniTask RunInitSequenceAsync()
        {
            if (_logos.Count == 0)
            {
                _logger.Warning("No logos configured, proceeding to next scene");
                await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeNextScene));
            }
            else
            {
                for (int i = 0; i < _logos.Count; i++)
                {
                    _currentLogoIndex = i;
                    _skipCurrentLogo = false;

                    await ShowLogoAsync(_logos[i]);

                    if (i < _logos.Count - 1)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenLogos));
                    }
                }
                await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeNextScene));
            }

            _logger.Info($"Loading scene: {_nextSceneName}");
            await _sceneService.LoadAsync(
                _nextSceneName,
                SceneLoadParams.Instant,
                new SceneTransitionData());
        }

        private async UniTask ShowLogoAsync(LogoData logo)
        {
            if (_logoCanvasGroup == null || _logoImage == null)
            {
                _logger.Warning("Logo UI components not assigned");
                await UniTask.Delay(TimeSpan.FromSeconds(_defaultDisplayDuration));
                return;
            }

            _isShowingLogo = true;
            _logoImage.sprite = logo.Sprite;
            
            if (logo.OverrideColor)
            {
                _logoImage.color = logo.Color;
            }

            PlaySound(logo.Sound);

            float fadeIn = logo.OverrideTiming ? logo.FadeInDuration : _defaultFadeInDuration;
            float display = logo.OverrideTiming ? logo.DisplayDuration : _defaultDisplayDuration;
            float fadeOut = logo.OverrideTiming ? logo.FadeOutDuration : _defaultFadeOutDuration;

            _logger.Debug($"Showing logo {_currentLogoIndex + 1}/{_logos.Count}: {logo.Name}");
            await FadeAsync(0f, 1f, fadeIn);

            float elapsed = 0f;
            while (elapsed < display && !_skipCurrentLogo)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield();
            }

            await FadeAsync(1f, 0f, fadeOut);
            _isShowingLogo = false;
        }

        private async UniTask FadeAsync(float from, float to, float duration)
        {
            if (duration <= 0)
            {
                _logoCanvasGroup.alpha = to;
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float speed = _skipCurrentLogo ? 3f : 1f;
                elapsed += Time.deltaTime * speed;
                
                float t = Mathf.Clamp01(elapsed / duration);
                _logoCanvasGroup.alpha = Mathf.Lerp(from, to, t);
                await UniTask.Yield();
            }
            _logoCanvasGroup.alpha = to;
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
    }
}
