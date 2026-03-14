using System;
using DG.Tweening;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Image _lethalIcon;

        [Header("Settings")]
        [SerializeField] private float _dividerWidth = 2f;

        [Header("Lethal Icon Settings")]
        [SerializeField] private float _lethalPulseDuration = 0.5f;
        [SerializeField] private float _lethalPulseScale = 1.2f;

        private Image[] _dividers;
        private float _current;
        private float _currentView;
        private float _damagePreview;
        private float _maxHp;
        private bool _hasDamagePreview;
        private Tween? _lethalPulseTween;

        public void Init(float current, float max, Color mainFillColor)
        {
            _current = current;
            _currentView = current;
            _maxHp = max;
            _mainFill.color = mainFillColor;
            _hasDamagePreview = false;

            if (_lethalPulseTween != null && _lethalPulseTween.IsActive())
            {
                _lethalPulseTween.Kill();
            }
            if (_lethalIcon != null)
            {
                _lethalIcon.transform.localScale = Vector3.one;
            }

            CreateDividers();
            SyncView();
        }

        private void CreateDividers()
        {
            if (_dividerContainer == null || _dividerTemplate == null)
            {
                Debug.LogWarning("[HealthBar] Container or Template is null!");
                return;
            }

            for (int i = _dividerContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_dividerContainer.GetChild(i).gameObject);
            }

            _dividerTemplate.gameObject.SetActive(false);
            
            // Создаем 3 разделителя: 25%, 50%, 75%
            float[] dividerPositions = { 0.25f, 0.50f, 0.75f };
            _dividers = new Image[dividerPositions.Length];

            RectTransform containerRect = _dividerContainer.GetComponent<RectTransform>();
            float containerWidth = containerRect.rect.width;

            for (int i = 0; i < dividerPositions.Length; i++)
            {
                Image divider = Instantiate(_dividerTemplate, _dividerContainer);
                divider.gameObject.SetActive(true);

                RectTransform dividerRect = divider.rectTransform;

                // === Якоря в ЦЕНТР ===
                dividerRect.anchorMin = new Vector2(0.5f, 0.5f);
                dividerRect.anchorMax = new Vector2(0.5f, 0.5f);
                dividerRect.pivot = new Vector2(0.5f, 0.5f);

                // Позиция от центра контейнера (процент от ширины)
                float xPos = (dividerPositions[i] * containerWidth) - (containerWidth / 2f);

                dividerRect.anchoredPosition = new Vector2(xPos, 0);
                dividerRect.sizeDelta = new Vector2(_dividerWidth, containerRect.rect.height);

                _dividers[i] = divider;
            }
        }

        private void SyncView()
        {
            _mainFill.fillAmount = Mathf.Clamp01(_currentView / _maxHp);

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

            bool isLethal = _hasDamagePreview && (_current - _damagePreview <= 0);
            _hpText.gameObject.SetActive(!isLethal);
            if (!isLethal)
            {
                _hpText.text = $"{Mathf.CeilToInt(_current)}/{Mathf.CeilToInt(_maxHp)}";
            }

            _lethalIcon.gameObject.SetActive(isLethal);
            if (isLethal)
            {
                if (_lethalPulseTween == null || !_lethalPulseTween.IsActive())
                {
                    _lethalPulseTween = _lethalIcon.transform
                        .DOScale(_lethalPulseScale, _lethalPulseDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetTarget(_lethalIcon);
                }
            }
            else
            {
                if (_lethalPulseTween != null && _lethalPulseTween.IsActive())
                {
                    _lethalPulseTween.Kill();
                    _lethalIcon.transform.localScale = Vector3.one;
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
            if (_lethalPulseTween != null && _lethalPulseTween.IsActive())
            {
                _lethalPulseTween.Kill();
                if (_lethalIcon != null)
                {
                    _lethalIcon.transform.localScale = Vector3.one;
                }
            }
            SyncView();
        }

        private void OnDestroy()
        {
            _lethalPulseTween?.Kill();
        }
    }
}