using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Stage.Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Caches figure passive icons for current battle.
    /// Listens to StageStarted/StageCompleted messages to preload/clear automatically.
    /// </summary>
    public sealed class FigureIconCacheService : IInitializable, IDisposable
    {
        private readonly ConfigProvider _configProvider;
        private readonly PlayerRunStateService _runStateService;
        private readonly IAssetService _assetService;
        private readonly ISubscriber<string, StagePhaseMessage> _stagePhaseSubscriber;
        private readonly ILogger<FigureIconCacheService> _logger;
        private readonly HashSet<string> _cachedIconAddresses = new();
        private readonly List<Sprite> _cachedSprites = new();
        private IDisposable? _subscription;

        [Inject]
        private FigureIconCacheService(
            ConfigProvider configProvider,
            PlayerRunStateService runStateService,
            IAssetService assetService,
            ISubscriber<string, StagePhaseMessage> stagePhaseSubscriber,
            ILogService logService)
        {
            _configProvider = configProvider;
            _runStateService = runStateService;
            _assetService = assetService;
            _stagePhaseSubscriber = stagePhaseSubscriber;
            _logger = logService.CreateLogger<FigureIconCacheService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _stagePhaseSubscriber.Subscribe(StagePhaseMessage.STAGE_STARTED, OnStageStarted).AddTo(bag);
            _stagePhaseSubscriber.Subscribe(StagePhaseMessage.STAGE_COMPLETED, OnStageCompleted).AddTo(bag);
            _subscription = bag.Build();
        }

        private void OnStageStarted(StagePhaseMessage msg)
        {
            _logger.Debug($"[FigureIconCache] Stage started: {msg.StageId}");
            
            if (_runStateService.Current != null)
            {
                PreloadForRunAsync(_runStateService.Current).Forget();
            }
        }

        private void OnStageCompleted(StagePhaseMessage msg)
        {
            _logger.Debug($"[FigureIconCache] Stage completed: {msg.StageId}, clearing cache...");
            Clear();
        }

        /// <summary>
        /// Preloads icons for all passives of figures in current run state.
        /// </summary>
        public async UniTask PreloadForRunAsync(PlayerRunStateModel runState)
        {
            _logger.Debug("[FigureIconCache] Starting preload for run...");
            HashSet<string> figureIds = new HashSet<string>();

            foreach (FigureState figure in runState.FiguresInHand)
            {
                figureIds.Add(figure.TypeId);
            }

            foreach (FigureState figure in runState.FiguresOnBoard)
            {
                figureIds.Add(figure.TypeId);
            }

            await PreloadForFiguresAsync(figureIds);
        }

        /// <summary>
        /// Preloads icons for specific figure types.
        /// </summary>
        public async UniTask PreloadForFiguresAsync(HashSet<string> figureTypeIds)
        {
            HashSet<string> iconAddresses = new();

            FigureConfigRepository? figureRepo = await _configProvider.Get<FigureConfigRepository>("figures_conf");
            FigureInfoConfigRepository? figureInfoRepo = await _configProvider.Get<FigureInfoConfigRepository>("figure_info_conf");
            FigureDescriptionConfigRepository? figureDescRepo = await _configProvider.Get<FigureDescriptionConfigRepository>("figure_descriptions_conf");
            PassiveConfigRepository? passiveRepo = await _configProvider.Get<PassiveConfigRepository>("passives_conf");

            if (figureRepo == null || figureInfoRepo == null || figureDescRepo == null || passiveRepo == null)
            {
                _logger.Warning("[FigureIconCache] Failed to load configs");
                return;
            }

            foreach (string figureTypeId in figureTypeIds)
            {
                FigureConfig? figureConfig = figureRepo.Get(figureTypeId);
                if (figureConfig == null) continue;

                // Figure's icon from FigureInfoConfig
                FigureInfoConfig? figureInfoConfig = figureInfoRepo.Get(figureConfig.InfoId);
                if (figureInfoConfig != null && !string.IsNullOrEmpty(figureInfoConfig.Icon))
                {
                    iconAddresses.Add(figureInfoConfig.Icon);
                }

                // Passive icons from FigureDescriptionConfig
                FigureDescriptionConfig? figureDescConfig = figureDescRepo.Get(figureTypeId);
                if (figureDescConfig?.Passives != null)
                {
                    foreach (string passiveId in figureDescConfig.Passives)
                    {
                        PassiveConfig? passiveConfig = passiveRepo.Get(passiveId);
                        if (passiveConfig != null && !string.IsNullOrEmpty(passiveConfig.Icon))
                        {
                            iconAddresses.Add(passiveConfig.Icon);
                        }
                    }
                }
            }

            // Remove already cached
            iconAddresses.ExceptWith(_cachedIconAddresses);

            int count = 0;
            foreach (string address in iconAddresses)
            {
                Sprite? sprite = await _assetService.LoadAsync<Sprite>(address);
                if (sprite != null)
                {
                    _cachedIconAddresses.Add(address);
                    _cachedSprites.Add(sprite);
                    count++;
                }
            }

            _logger.Info($"[FigureIconCache] Preloaded {count} new icons (total cached: {_cachedIconAddresses.Count})");
        }

        /// <summary>
        /// Clears cached icons to free memory.
        /// </summary>
        public void Clear()
        {
            _logger.Debug("[FigureIconCache] Clearing cache...");

            foreach (Sprite sprite in _cachedSprites)
            {
                if (sprite != null)
                {
                    _assetService.Release(sprite);
                }
            }

            _cachedSprites.Clear();
            _cachedIconAddresses.Clear();
            _logger.Info("[FigureIconCache] Cache cleared");
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
