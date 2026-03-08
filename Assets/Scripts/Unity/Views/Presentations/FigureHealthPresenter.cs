using Cysharp.Threading.Tasks;
using Project.Core.Core.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Presentations;
using Project.Gameplay.UI;
using Project.Unity.UI.Components.Game;
using UniRx;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public sealed class FigureHealthPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField] private Color _playerTeamColor = Color.green;
        [SerializeField] private Color _enemyTeamColor =  Color.red;

        [SerializeField] private HealthBar _viewTemplate;
        [SerializeField] private Transform _pivot;

        private static WorldUIWindow? _cachedWorldUi;
        
        private EntityLink _entityLink;
        private HealthBar? _healthView;
        private Figure? _figure;
        private CanvasGroup? _canvasGroup;
        private bool _initialized = false;
        private CompositeDisposable? _disposables;

        public async UniTask Init(EntityLink link)
        {
            if(link.GetEntity() is not Figure figure)
            {
                return;
            }

            _entityLink = link;
            _figure = figure;
            _initialized = true;
            _disposables = new CompositeDisposable();

            if (_cachedWorldUi == null)
            {
                await UIService.Initialized;
                _cachedWorldUi = await UIService.GetOrCreateAsync<WorldUIWindow>();
            }
            
            TryCreateHealthBar();
            _figure.Stats.CurrentHp
                .Skip(1)
                .Subscribe(OnHpChanged)
                .AddTo(_disposables);
        }

        private void OnHpChanged(float hp)
        {
            if (_healthView == null)
            {
                return;
            }
            _healthView.SetHp(hp);
        }

        private void TryCreateHealthBar()
        {
            if (_healthView != null || _figure == null || _cachedWorldUi == null)
            {
                return;
            }

            _healthView = _cachedWorldUi.Add(_viewTemplate, _pivot);
            if (_healthView == null)
            {
                return;
            }
            Color color = _figure.Team == Team.Player ? _playerTeamColor : _enemyTeamColor;
            _healthView.Init(_figure.Stats.CurrentHp.Value, _figure.Stats.MaxHp, color);
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

        public void SetDamagePreview(float? predictedHp)
        {
            if (_healthView == null)
                return;

            if (predictedHp.HasValue)
            {
                _healthView.SetDamagePreview(predictedHp.Value);
            }
            else
            {
                _healthView.ClearDamagePreview();
            }
        }

        private CanvasGroup? GetOrCreateCanvasGroup()
        {
            if (_healthView == null)
            {
                return null;
            }

            if (_canvasGroup == null || _canvasGroup.gameObject != _healthView.gameObject)
            {
                _canvasGroup = _healthView.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _healthView.gameObject.AddComponent<CanvasGroup>();
                }
            }

            return _canvasGroup;
        }

        private void RemoveBar()
        {
            if (_healthView == null || _cachedWorldUi == null)
            {
                return;
            }

            // Отписываемся от всех подписок
            _disposables?.Clear();

            _cachedWorldUi.Remove(_healthView);
            _healthView = null;
            _figure = null;
            _canvasGroup = null;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            RemoveBar();
        }
    }
}
