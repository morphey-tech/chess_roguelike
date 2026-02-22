using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Presentations;
using Project.Gameplay.UI;
using Project.Unity.UI.Components.Game;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public sealed class FigureHealthPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField] private Color _playerTeamColor = Color.green;
        [SerializeField] private Color _enemyTeamColor =  Color.red;
        
        [SerializeField] private HealthBar _viewTemplate;

        private EntityLink _entityLink;
        private HealthBar _healthView;
        private Figure? _figure;
        private CanvasGroup? _canvasGroup;
        private FigureView _figure2;

        public void Init(EntityLink link)
        {
            // Таких преколов канешн не должно быть. Шо та с энтитей делать надо
            if(link.GetEntity() is not Figure figure)
                return;
            
            _entityLink = link; 
            _figure = figure;
            _healthView = Gameplay.Gameplay.UI.UIService.GetOrCreate<WorldUIWindow>().Add(_viewTemplate, transform);

            var color = _figure.Team == Team.Player ? _playerTeamColor : _enemyTeamColor;
            _healthView.Init(_figure.Stats.CurrentHp, _figure.Stats.MaxHp, color);
        }

        public void Init2(FigureView figure)
        {
            _figure2 = figure;
            var color = _figure2.Team == Team.Player ? _playerTeamColor : _enemyTeamColor;
            _healthView = Gameplay.Gameplay.UI.UIService.GetOrCreate<WorldUIWindow>().Add(_viewTemplate, transform);
            _healthView.Init(_figure2.CurrentHp, _figure2.MaxHp, color); 
        }

        private void Update()
        {
            if (_figure2 == null || _healthView == null)
                return;

            /*
            if (_figure2 == n)
            {
                RemoveBar();
                return;
            }*/

            _healthView.SetHp(_figure2.CurrentHp);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            if (_healthView != null)
            {
                CanvasGroup? canvasGroup = GetOrCreateCanvasGroup();
                if (canvasGroup == null)
                    return;

                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        private CanvasGroup? GetOrCreateCanvasGroup()
        {
            if (_healthView == null)
                return null;

            if (_canvasGroup == null || _canvasGroup.gameObject != _healthView.gameObject)
            {
                _canvasGroup = _healthView.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = _healthView.gameObject.AddComponent<CanvasGroup>();
            }

            return _canvasGroup;
        }

        private void RemoveBar()
        {
            if (_healthView == null)
                return;
            Gameplay.Gameplay.UI.UIService.GetOrCreate<WorldUIWindow>().Remove(_healthView);
            _healthView = null;
            _figure = null;
            _canvasGroup = null;
        }

        private void OnDestroy()
        {
            RemoveBar();
        }
    }
}
