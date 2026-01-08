using Cysharp.Threading.Tasks;
using LiteUI.Tweening.Config;
using UnityEngine;
using UnityEngine.Events;

namespace LiteUI.Tweening
{
    /// <summary>
    /// Компонент для анимации любого UI элемента.
    /// Добавь на любой GameObject с RectTransform и настрой конфиги в инспекторе.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UITweenAnimator : MonoBehaviour
    {
        [Header("Анимации")]
        [Tooltip("Анимация показа")]
        [SerializeField] private UITweenConfig _showConfig;
        
        [Tooltip("Анимация скрытия")]
        [SerializeField] private UITweenConfig _hideConfig;
        
        [Tooltip("Анимация при наведении")]
        [SerializeField] private UITweenConfig _hoverConfig;
        
        [Tooltip("Анимация при клике")]
        [SerializeField] private UITweenConfig _clickConfig;
        
        [Tooltip("Кастомные анимации (вызываются по имени)")]
        [SerializeField] private CustomAnimation[] _customAnimations;

        [Header("Настройки")]
        [Tooltip("Автоматически проиграть Show при включении")]
        [SerializeField] private bool _playShowOnEnable = true;
        
        [Tooltip("Скрыть объект после Hide анимации")]
        [SerializeField] private bool _deactivateAfterHide = true;
        
        [Tooltip("Начальное состояние — скрыт")]
        [SerializeField] private bool _startHidden;

        [Header("События")]
        [SerializeField] private UnityEvent _onShowComplete;
        [SerializeField] private UnityEvent _onHideComplete;

        private UITweenPlayer _player;
        private bool _isVisible = true;

        public bool IsVisible => _isVisible;
        public bool IsAnimating => _player?.IsPlaying ?? false;

        private void Awake()
        {
            _player = new UITweenPlayer(gameObject);
            
            if (_startHidden)
            {
                _isVisible = false;
                var cg = GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0;
            }
        }

        private void OnEnable()
        {
            if (_playShowOnEnable && !_startHidden)
            {
                Show().Forget();
            }
            _startHidden = false; // Сбрасываем после первого включения
        }

        /// <summary>
        /// Показать элемент с анимацией
        /// </summary>
        public async UniTask Show()
        {
            if (_showConfig == null)
            {
                _isVisible = true;
                return;
            }

            gameObject.SetActive(true);
            await _player.Play(_showConfig);
            _isVisible = true;
            _onShowComplete?.Invoke();
        }

        /// <summary>
        /// Скрыть элемент с анимацией
        /// </summary>
        public async UniTask Hide()
        {
            if (_hideConfig == null)
            {
                _isVisible = false;
                if (_deactivateAfterHide) gameObject.SetActive(false);
                return;
            }

            await _player.Play(_hideConfig);
            _isVisible = false;
            _onHideComplete?.Invoke();
            
            if (_deactivateAfterHide)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Анимация при наведении
        /// </summary>
        public async UniTask PlayHover()
        {
            if (_hoverConfig != null)
            {
                await _player.Play(_hoverConfig);
            }
        }

        /// <summary>
        /// Анимация при клике
        /// </summary>
        public async UniTask PlayClick()
        {
            if (_clickConfig != null)
            {
                await _player.Play(_clickConfig);
            }
        }

        /// <summary>
        /// Воспроизвести кастомную анимацию по имени
        /// </summary>
        public async UniTask PlayCustom(string animationName)
        {
            if (_customAnimations == null) return;

            foreach (var custom in _customAnimations)
            {
                if (custom.Name == animationName && custom.Config != null)
                {
                    await _player.Play(custom.Config);
                    return;
                }
            }
            
            Debug.LogWarning($"[UITweenAnimator] Custom animation '{animationName}' not found on {gameObject.name}");
        }

        /// <summary>
        /// Воспроизвести произвольный конфиг
        /// </summary>
        public async UniTask Play(UITweenConfig config)
        {
            if (config != null)
            {
                await _player.Play(config);
            }
        }

        /// <summary>
        /// Остановить текущую анимацию
        /// </summary>
        public void Stop(bool complete = false)
        {
            _player?.Stop(complete);
        }

        /// <summary>
        /// Переключить видимость
        /// </summary>
        public async UniTask Toggle()
        {
            if (_isVisible)
            {
                await Hide();
            }
            else
            {
                await Show();
            }
        }

        // Для вызова из UnityEvents в инспекторе
        public void ShowFireAndForget() => Show().Forget();
        public void HideFireAndForget() => Hide().Forget();
        public void ToggleFireAndForget() => Toggle().Forget();
        public void PlayHoverFireAndForget() => PlayHover().Forget();
        public void PlayClickFireAndForget() => PlayClick().Forget();
        public void PlayCustomFireAndForget(string name) => PlayCustom(name).Forget();

        [System.Serializable]
        public class CustomAnimation
        {
            public string Name;
            public UITweenConfig Config;
        }
    }
}

