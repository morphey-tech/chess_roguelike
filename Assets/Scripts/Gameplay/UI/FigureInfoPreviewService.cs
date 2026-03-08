using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.UI;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Сервис управления окном информации о фигуре.
    /// Открывает окно при клике на фигуру (ЛКМ).
    /// Закрывает окно при клике вне фигуры или по ПКМ.
    /// </summary>
    public sealed class FigureInfoPreviewService : IStartable, IDisposable
    {
        private readonly ISubscriber<FigureHoverChangedMessage> _figureHoverPublisher;
        private readonly ISubscriber<CellClickedMessage> _cellClickedPublisher;
        private readonly ISubscriber<RightClickMessage> _rightClickPublisher;
        private readonly ISubscriber<CancelRequestedMessage> _cancelPublisher;
        private readonly ITooltipService _tooltipService;
        private readonly RunHolder _runHolder;
        private readonly ConfigProvider _configProvider;
        private readonly CombatResolver _combatResolver;
        private readonly ILogger<FigureInfoPreviewService> _logger;

        private int? _hoveredFigureId;
        private IDisposable? _hoverSubscription;
        private IDisposable? _clickSubscription;
        private IDisposable? _rightClickSubscription;
        private IDisposable? _cancelSubscription;
        private FigureInfoWindow? _window;
        private FigureInfoConfigRepository? _figureInfoCache;
        private PassiveConfigRepository? _passiveCache;

        [Inject]
        private FigureInfoPreviewService(
            ISubscriber<FigureHoverChangedMessage> figureHoverPublisher,
            ISubscriber<CellClickedMessage> cellClickedPublisher,
            ISubscriber<RightClickMessage> rightClickPublisher,
            ISubscriber<CancelRequestedMessage> cancelPublisher,
            ITooltipService tooltipService,
            RunHolder runHolder,
            ConfigProvider configProvider,
            CombatResolver combatResolver,
            IAssetService assetService,
            ILogService logService)
        {
            _figureHoverPublisher = figureHoverPublisher;
            _cellClickedPublisher = cellClickedPublisher;
            _rightClickPublisher = rightClickPublisher;
            _cancelPublisher = cancelPublisher;
            _tooltipService = tooltipService;
            _runHolder = runHolder;
            _configProvider = configProvider;
            _combatResolver = combatResolver;
            _logger = logService.CreateLogger<FigureInfoPreviewService>();
        }

        void IStartable.Start()
        {
            _hoverSubscription = _figureHoverPublisher.Subscribe(OnFigureHoverChanged);
            _clickSubscription = _cellClickedPublisher.Subscribe(OnCellClicked);
            _rightClickSubscription = _rightClickPublisher.Subscribe(OnRightClick);
            _cancelSubscription = _cancelPublisher.Subscribe(OnCancelRequested);
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync()
        {
            _figureInfoCache = await _configProvider.Get<FigureInfoConfigRepository>("figures_info_conf");
            _passiveCache = await _configProvider.Get<PassiveConfigRepository>("passives_conf");
            await UIService.Initialized;
            _window = await UIService.GetOrCreateAsync<FigureInfoWindow>();
        }

        private void OnFigureHoverChanged(FigureHoverChangedMessage message)
        {
            _hoveredFigureId = message.FigureId;
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (!_hoveredFigureId.HasValue)
            {
                CloseWindow();
                return;
            }

            Run.Run? run = _runHolder.Current;
            if (run?.CurrentStage?.Grid == null)
            {
                CloseWindow();
                return;
            }

            BoardGrid? grid = run.CurrentStage.Grid;
            Figure? figure = grid.GetFigureById(_hoveredFigureId.Value);
            if (figure == null)
            {
                CloseWindow();
                return;
            }

            ShowFigureInfo(figure);
        }

        private void OnRightClick(RightClickMessage message)
        {
            CloseWindow();
        }

        private void OnCancelRequested(CancelRequestedMessage message)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            if (_window != null && _window.IsVisible())
            {
                _window.Hide();
            }
            _tooltipService.HideTooltip();
        }

        private void ShowFigureInfo(Figure figure)
        {
            if (_window == null)
            {
                _logger.Debug("FigureInfoWindow is not initialized yet, skipping");
                return;
            }

            if (_passiveCache == null)
            {
                _logger.Debug("Passive cache is not loaded yet, skipping");
                return;
            }

            try
            {
                BeforeHitContext previewContext = new()
                {
                    Attacker = figure,
                    Target = figure,
                    Grid = _runHolder.Current?.CurrentStage?.Grid ?? throw new InvalidOperationException("Grid is null"),
                    BaseDamage = figure.Stats.Attack.Value
                };

                _combatResolver.ApplyPassivesForPreview(figure, figure, previewContext);

                FigureInfoConfig? infoConfig = null;
                if (!string.IsNullOrEmpty(figure.InfoId) && _figureInfoCache != null)
                {
                    infoConfig = _figureInfoCache.Get(figure.InfoId);
                }

                List<PassiveConfig> passiveConfigs = new();
                foreach (IPassive? passive in figure.BasePassives)
                {
                    PassiveConfig? passiveConfig = _passiveCache.Get(passive.Id);
                    if (passiveConfig != null)
                    {
                        passiveConfigs.Add(passiveConfig);
                    }
                }

                FigureInfoWindow.FigureInfoModel model = new()
                {
                    Figure = figure,
                    InfoConfig = infoConfig,
                    PassiveConfigs = passiveConfigs
                };

                _window.Show(model);

                // Очищаем модификаторы превью
                figure.Stats.Attack.ClearByContext(ModifierSourceContext.PreviewCalculation);
                figure.Stats.Defence.ClearByContext(ModifierSourceContext.PreviewCalculation);
                figure.Stats.Evasion.ClearByContext(ModifierSourceContext.PreviewCalculation);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to show figure info window: {ex.Message}");
            }
        }

        void IDisposable.Dispose()
        {
            _hoverSubscription?.Dispose();
            _clickSubscription?.Dispose();
            _rightClickSubscription?.Dispose();
            _cancelSubscription?.Dispose();
        }
    }
}
