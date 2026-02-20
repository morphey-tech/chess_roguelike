using UnityEngine;
using UnityEngine.UI;

namespace Project.Unity.UI.Components.Game
{
    public class HealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _mainFill;
        [SerializeField] private Image _damageFill;
        [SerializeField] private Transform _dividerContainer;
        [SerializeField] private Image _dividerTemplate;

        [Header("Settings")]
        [SerializeField] private float _dividerWidth = 2f;

        private Image[] _dividers = new Image[0];
        private float _current;
        private float _currentView;
        private float _damagePreview;
        private float _maxHp;
        private bool _hasDamagePreview;

        public void Init(float current, float max, Color mainFillColor)
        {
            _current = current;
            _currentView = current;
            _maxHp = max;
            _mainFill.color = mainFillColor;
            _hasDamagePreview = false;

            CreateDividers((int)max);
            SyncView();
        }

        private void CreateDividers(int maxHp)
        {
            if (_dividerContainer == null || _dividerTemplate == null)
            {
                Debug.LogWarning("[HealthBar] Container or Template is null!");
                return;
            }

            // Очищаем контейнер
            for (int i = _dividerContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_dividerContainer.GetChild(i).gameObject);
            }

            _dividerTemplate.gameObject.SetActive(false);
            _dividers = new Image[maxHp - 1];

            RectTransform containerRect = _dividerContainer.GetComponent<RectTransform>();
            float containerWidth = containerRect.rect.width;
            float segmentWidth = containerWidth / maxHp;

            Debug.Log($"[HealthBar] Container: {containerWidth}px, Segments: {maxHp}, SegmentWidth: {segmentWidth}px");

            for (int i = 1; i < maxHp; i++)
            {
                Image divider = Instantiate(_dividerTemplate, _dividerContainer);
                divider.gameObject.SetActive(true);

                RectTransform dividerRect = divider.rectTransform;

                // === ВАЖНО: Якоря в ЦЕНТР ===
                dividerRect.anchorMin = new Vector2(0.5f, 0.5f);
                dividerRect.anchorMax = new Vector2(0.5f, 0.5f);
                dividerRect.pivot = new Vector2(0.5f, 0.5f);

                // Позиция от центра контейнера
                float xPos = (i * segmentWidth) - (containerWidth / 2f);
                
                dividerRect.anchoredPosition = new Vector2(xPos, 0);
                dividerRect.sizeDelta = new Vector2(_dividerWidth, containerRect.rect.height);

                _dividers[i - 1] = divider;

                if (i == 1)
                {
                    Debug.Log($"[HealthBar] FIRST divider: i={i}, xPos={xPos}, segmentWidth={segmentWidth}");
                }
            }
        }

        private void SyncView()
        {
            if (_mainFill != null)
            {
                _mainFill.fillAmount = Mathf.Clamp01(_currentView / _maxHp);
            }
            if (_damageFill != null)
            {
                if (_hasDamagePreview && _current > 0)
                {
                    // Показываем урон как красную полоску ПОВЕРХ зеленого
                    // Красная полоска должна перекрывать правую часть зеленого бара
                    // currentPercent = текущее HP / макс HP
                    // damagePercent = урон / макс HP
                    float currentPercent = _current / _maxHp;
                    float damagePercent = _damagePreview / _maxHp;
                    
                    // Сбрасываем offsets и fillAmount для корректной работы
                    _damageFill.rectTransform.offsetMin = Vector2.zero;
                    _damageFill.rectTransform.offsetMax = Vector2.zero;
                    _damageFill.fillAmount = 1f;
                    
                    // Обрезаем красный бар так, чтобы он показывался только в зоне урона
                    // Левый край на позиции (currentPercent - damagePercent), правый на currentPercent
                    RectTransform mainRect = _mainFill.rectTransform;
                    float containerWidth = mainRect.rect.width;
                    
                    float leftPercent = currentPercent - damagePercent;
                    float rightPercent = currentPercent;
                    
                    _damageFill.rectTransform.offsetMin = new Vector2(containerWidth * leftPercent, 0);
                    _damageFill.rectTransform.offsetMax = new Vector2(-containerWidth * (1 - rightPercent), 0);
                    
                    _damageFill.gameObject.SetActive(true);
                }
                else
                {
                    _damageFill.rectTransform.offsetMin = Vector2.zero;
                    _damageFill.rectTransform.offsetMax = Vector2.zero;
                    _damageFill.gameObject.SetActive(false);
                }
            }
        }

        public void SetHp(float statsCurrentHp)
        {
            _current = statsCurrentHp;
            _currentView = _current;
            SyncView();
        }

        public void SetDamagePreview(float damage)
        {
            _damagePreview = damage;
            _hasDamagePreview = true;
            SyncView();
        }

        public void ClearDamagePreview()
        {
            _hasDamagePreview = false;
            SyncView();
        }
    }
}