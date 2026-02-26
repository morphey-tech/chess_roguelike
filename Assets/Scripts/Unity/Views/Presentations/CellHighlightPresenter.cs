using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay;
using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using Project.Gameplay.ShrinkingZone;
using Shapes;
using UniRx;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views.Presentations
{
    public class CellHighlightPresenter : MonoBehaviour, IPresenter
    {
        [Header("Actions")]
        [SerializeField] private GameObject _highlightRenderer;
        [SerializeField] private GameObject _attackRenderer;
        
        [Header("Zone")]
        [SerializeField] private ShapeRenderer _zonePreviewRenderer;
        [SerializeField] private Color _zoneWarningColor;
        [SerializeField] private Color _zoneDangersColor;
        
        private ILogger<CellHighlightPresenter> _logger;
        
        private EntityLink _link = null!;
        private IDisposable? _disposable;

        [Inject]
        private void Construct(ILogService logger)
        {
            _logger = logger.CreateLogger<CellHighlightPresenter>();
        }
        
        public UniTask Init(EntityLink link)
        {
            _link = link;
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _link.GetEntity().Components.ObserveAdd().Subscribe(OnComponentAdded);
            _link.GetEntity().Components.ObserveRemove().Subscribe(OnComponentRemoved);
            _disposable = bag.Build();
            return UniTask.CompletedTask;
        }

        private void OnComponentAdded(CollectionAddEvent<IEntityComponent> evt)
        {
            SetDefaultHighlight();
            switch (evt.Value)
            {
                case HighlightTag _:
                    SetHighlight();
                    break;
                case AttackHighlightTag _:
                    SetAttackHighlight();
                    break;
                case StormWarningTag _:
                    ShowWarningsZone();
                    break;
                case StormDangerTag _:
                    ShowDangerousZone();
                    break;
            }
        }

        private void OnComponentRemoved(CollectionRemoveEvent<IEntityComponent> evt)
        {
            Entity entity = _link.GetEntity();
            if (!entity.Exists<HighlightTag>() && !entity.Exists<AttackHighlightTag>())
            {
                SetDefaultHighlight();
            }
        }

        private void SetDefaultHighlight()
        {
            if (_highlightRenderer != null)
                _highlightRenderer.SetActive(false);
            if (_attackRenderer != null)
                _attackRenderer.SetActive(false);
        }
        
        private void SetHighlight()
        {
            if (_highlightRenderer != null)
                _highlightRenderer.SetActive(true);
            if (_attackRenderer != null)
                _attackRenderer.SetActive(false);
        }
        
        private void SetAttackHighlight()
        {
            if (_highlightRenderer != null)
                _highlightRenderer.SetActive(false);
            if (_attackRenderer != null)
                _attackRenderer.SetActive(true);
        }

        private void ShowWarningsZone()
        {
            _zonePreviewRenderer.gameObject.SetActive(true);
            _zonePreviewRenderer.Color = _zoneWarningColor;
        }

        private void ShowDangerousZone()
        {
            _zonePreviewRenderer.gameObject.SetActive(true);
            _zonePreviewRenderer.Color = _zoneDangersColor;
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}