using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using VContainer;
using MessagePipe;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.Artifacts;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.UI.Project.Gameplay.Gameplay.UI;

namespace Project.Gameplay.UI
{
    public class ArtifactsWindow : ParameterlessWindow
    {
        [Header("Layout")]
        [SerializeField] private Transform _contentParent;

        [Header("Empty State")]
        [SerializeField] private GameObject _emptyState;

        private ArtifactService _artifactService = null!;
        private ConfigProvider _configProvider = null!;
        private IUIAssetService _uiAssetService = null!;
        private ISubscriber<ArtifactChangedMessage> _artifactChangedSubscriber = null!;

        private readonly List<ArtifactItemView> _itemViews = new();
        private ArtifactConfigRepository? _repository;
        private CompositeDisposable _disposables = null!;
        private IDisposable? _subscription;

        public override bool HideOtherWindows => false;
        public override bool IgnoreHideOthersWindows => true;
        public override bool NeedShowBackground => false;
        public override int ZOrder => 90;

        [Inject]
        private void Construct(ArtifactService artifactService,
            ConfigProvider configProvider,
            IUIAssetService uiAssetService,
            ISubscriber<ArtifactChangedMessage> artifactChangedSubscriber)
        {
            _artifactService = artifactService;
            _configProvider = configProvider;
            _uiAssetService = uiAssetService;
            _artifactChangedSubscriber = artifactChangedSubscriber;
        }

        //Remove async void
        protected override async void OnInit()
        {
            _disposables = new CompositeDisposable();

            // Load artifacts config
            try
            {
                _repository = await _configProvider.Get<ArtifactConfigRepository>("artifacts_conf");
            }
            catch (Exception ex)
            {
                //Logger
                Debug.LogError($"Failed to load artifacts config: {ex.Message}");
            }
            
            // Subscribe to artifact changes
            _subscription = _artifactChangedSubscriber.Subscribe(OnArtifactChanged);
        }

        private void OnArtifactChanged(ArtifactChangedMessage message)
        {
            Refresh();
        }

        public void Refresh()
        {
            CreateItemViews().Forget();
        }

        //Calcellation token
        private async UniTask CreateItemViews()
        {
            CancellationToken ct = gameObject.GetCancellationTokenOnDestroy();
            
            // Clear existing
            foreach (ArtifactItemView? view in _itemViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }
            _itemViews.Clear();

            IReadOnlyList<ArtifactInstance> artifacts = _artifactService.Artifacts;
            _emptyState?.SetActive(artifacts.Count == 0);

            foreach (ArtifactInstance? instance in artifacts)
            {
                ArtifactConfig? config = _repository?.Get(instance.ConfigId);
                if (config == null)
                {
                    continue;
                }

                ArtifactItemView? view = await _uiAssetService.CreateAsync<ArtifactItemView>(
                    "ArtifactItemView",
                    parent: _contentParent, ct: ct);
                await view.Initialize(config, instance.Stack);
                _itemViews.Add(view);
            }
        }

        protected override void OnShowed()
        {
            CreateItemViews().Forget();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _subscription?.Dispose();
        }
    }
}
