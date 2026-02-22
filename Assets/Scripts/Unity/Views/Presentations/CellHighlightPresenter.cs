using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay;
using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Project.Unity.Unity.Views.Presentations
{
    public class CellHighlightPresenter : MonoBehaviour, IPresenter
    {
        [FormerlySerializedAs("_renderer")]
        [Header("Highlight")]
        [SerializeField] private GameObject _highlightRenderer;
        [SerializeField] private GameObject _attackRenderer;

        private ILogger<CellHighlightPresenter> _logger;
        
        private EntityLink _link = null!;
        private CancellationToken _destroyToken;

        [Inject]
        private void Construct(ILogService logger)
        {
            _logger = logger.CreateLogger<CellHighlightPresenter>();
        }
        
        public void Init(EntityLink link)
        {
            _link = link;
            _destroyToken = this.GetCancellationTokenOnDestroy();
      //      RunLoopAsync().Forget();
        }

        private void Update()
        {
            Entity entity = _link.GetEntity();
            SetDefault();
            if (entity.Exists<HighlightTag>())
            {
                SetHighlight();
            }
            else if (entity.Exists<AttackHighlightTag>())
            {
                SetAttackHighlight();
            }
            else
            {
                SetDefault();
            }
        }

        private async UniTaskVoid RunLoopAsync()
        {
            await UniTask.WaitUntil(LinkIsExist, cancellationToken: _destroyToken);

            while (!_destroyToken.IsCancellationRequested)
            {
                Entity entity = _link.GetEntity();
                SetDefault();
                if (entity.Exists<HighlightTag>())
                {
                    SetHighlight();
                }
                else if (entity.Exists<AttackHighlightTag>())
                {
                    SetAttackHighlight();
                }
                else
                {
                    SetDefault();
                }
                await UniTask.Yield(_destroyToken);
            }
        }

        private bool LinkIsExist()
        {
            return _link != null;
        }
        
        private void SetDefault()
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
    }
}