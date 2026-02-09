using UnityEngine;
using UnityEngine.UI;

namespace Project.Unity.UI.Components.Game
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _mainFill;
        [SerializeField] private Image _damageFill;
        
        private int _current;
        private int _currentView;
        private int _maxHp;
        
        public void Init(int current, int max, Color mainFillColor)
        {
            _current = current;
            _currentView = _current;
            _maxHp = max;
            _mainFill.color = mainFillColor;
            SyncView();
        }

        private void SyncView()
        {
            _mainFill.fillAmount = GetMainFillValue();
        }

        private float GetMainFillValue()
        {
            return _currentView / (float)_maxHp;
        }

        public void SetHp(int statsCurrentHp)
        {
            _current = statsCurrentHp;
            _currentView = _current;
            SyncView();
        }
    }
}