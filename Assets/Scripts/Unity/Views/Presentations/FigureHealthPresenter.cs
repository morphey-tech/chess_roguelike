using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Presentations;
using Project.Gameplay.UI;
using Project.Unity.UI.Components.Game;
using UnityEngine;

namespace Project.Unity.Presentations
{
    public sealed class FigureHealthPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField] private Color _playerTeamColor = Color.green;
        [SerializeField] private Color _enemyTeamColor =  Color.red;
        
        [SerializeField] private HealthBar _viewTemplate;
        [SerializeField] private Transform _pivot;
        
        private EntityLink _entityLink;
        private HealthBar _healthView;
        private Figure? _figure;

        public void Init(EntityLink link)
        {
            // Таких преколов канешн не должно быть. Шо та с энтитей делать надо
            if(link.GetEntity() is not Figure figure)
                return;
            
            _entityLink = link;
            _figure = figure;
            _healthView = Core.Window.UI.GetOrCreate<WorldUIWindow>().Add(_viewTemplate, _pivot);

            var color = _figure.Team == Team.Player ? _playerTeamColor : _enemyTeamColor;
            _healthView.Init(_figure.Stats.CurrentHp, _figure.Stats.MaxHp, color);
        }

        private void Update()
        {
            if(_figure == null)
                return;

            _healthView.SetHp(_figure.Stats.CurrentHp);
        }

        private void OnDestroy()
        {
            if(_healthView != null)
                Core.Window.UI.GetOrCreate<WorldUIWindow>().Remove(_healthView);
        }
    }
}
