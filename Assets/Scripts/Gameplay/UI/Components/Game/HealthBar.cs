using UnityEngine;
using UnityEngine.UI;

namespace Project.Unity.UI.Components.Game
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _mainFill;
        [SerializeField] private Image _damageFill;
        [SerializeField] private Transform _dividerContainer;
        [SerializeField] private Image _dividerTemplate;

        private Image[] _dividers = new Image[0];
        private float _current;
        private float _currentView;
        private float _maxHp;

        public void Init(float current, float max, Color mainFillColor)
        {
            _current = current;
            _currentView = _current;
            _maxHp = max;
            _mainFill.color = mainFillColor;
            
            CreateDividers((int)max);
            SyncView();
        }

        private void CreateDividers(int maxHp)
        {
            if (_dividerContainer != null)
            {
                foreach (Transform child in _dividerContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            _dividers = new Image[maxHp - 1];
            
            if (_dividerTemplate == null || _dividerContainer == null || _mainFill == null)
                return;
            
            RectTransform mainFillRect = _mainFill.rectTransform;
            float barWidth = mainFillRect.rect.width;
            float barHeight = mainFillRect.rect.height;
            
            RectTransform dividerTemplateRect = _dividerTemplate.rectTransform;
            float dividerWidth = dividerTemplateRect.rect.width;
            
            for (int i = 1; i < maxHp; i++)
            {
                Image? divider = Instantiate(_dividerTemplate, _dividerContainer);
                float normalizedPos = (float)i / maxHp;
                float xPos = (normalizedPos * barWidth) - (barWidth / 2f) + (dividerWidth / 2f);
                
                RectTransform dividerRect = divider.rectTransform;
                dividerRect.localPosition = new Vector2(xPos, 0);
                dividerRect.sizeDelta = new Vector2(dividerWidth, barHeight);
                
                _dividers[i - 1] = divider;
            }
        }

        private void SyncView()
        {
            _mainFill.fillAmount = GetMainFillValue();
        }

        private float GetMainFillValue()
        {
            return _currentView / (float)_maxHp;
        }

        public void SetHp(float statsCurrentHp)
        {
            _current = statsCurrentHp;
            _currentView = _current;
            SyncView();
        }
    }
}