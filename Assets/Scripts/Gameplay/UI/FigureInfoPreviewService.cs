using System;
using System.Collections.Generic;
using System.Threading;
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
    public sealed class FigureInfoPreviewService : IInitializable, IDisposable
    {
        private readonly ISubscriber<FigureHoverChangedMessage> _figureHoverPublisher;
        private readonly ISubscriber<CellClickedMessage> _cellClickedPublisher;
        private readonly ISubscriber<RightClickMessage> _rightClickPublisher;
        private readonly ISubscriber<CancelRequestedMessage> _cancelPublisher;
        private readonly ITooltipService _tooltipService;
        private readonly IUIService _uiService;
        private readonly RunHolder _runHolder;
        private readonly ConfigProvider _configProvider;
        private readonly CombatResolver _combatResolver;
        private readonly ILogger<FigureInfoPreviewService> _logger;

        private int? _hoveredFigureId;
        private FigureInfoWindow? _window;
        private FigureInfoConfigRepository? _figureInfoCache;
        private PassiveConfigRepository? _passiveCache;
        private CancellationTokenSource? _cts;
        private IDisposable _disposable = null!;

        [Inject]
        private FigureInfoPreviewService(
            ISubscriber<FigureHoverChangedMessage> figureHoverPublisher,
            ISubscriber<CellClickedMessage> cellClickedPublisher,
            ISubscriber<RightClickMessage> rightClickPublisher,
            ISubscriber<CancelRequestedMessage> cancelPublisher,
            ITooltipService tooltipService,
            IUIService uiService,
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
            _uiService = uiService;
            _runHolder = runHolder;
            _configProvider = configProvider;
            _combatResolver = combatResolver;
            _logger = logService.CreateLogger<FigureInfoPreviewService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _figureHoverPublisher.Subscribe(OnFigureHoverChanged).AddTo(bag);
            _cellClickedPublisher.Subscribe(OnCellClicked).AddTo(bag);
            _rightClickPublisher.Subscribe(OnRightClick).AddTo(bag);
            _cancelPublisher.Subscribe(OnCancelRequested).AddTo(bag);
            _disposable = bag.Build();
            EnsureConfigsAsync().Forget();
        }

        private async UniTaskVoid EnsureConfigsAsync()
        {
            _figureInfoCache = await _configProvider.Get<FigureInfoConfigRepository>("figures_info_conf");
            _passiveCache = await _configProvider.Get<PassiveConfigRepository>("passives_conf");
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

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                ShowFigureInfo(figure, _cts.Token).Forget();
            }
            catch (OperationCanceledException)
            {
                //Swallow
            }
            catch (Exception e)
            {
                _logger.Error($"[FigureInfoPreviewService] Catch exception while try show figure info. e={e.Message}");
            }
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

        private async UniTask ShowFigureInfo(Figure figure, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (_window == null)
            {
                try
                {
                    _window = await _uiService.GetOrCreateAsync<FigureInfoWindow>();
                    token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    //Swallow
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[FigureInfoPreviewService] Failed to create FigureInfoWindow: {ex.Message}", ex);
                    return;
                }
            }

            if (_passiveCache == null)
            {
                _logger.Debug("[FigureInfoPreviewService] Passive cache is not loaded yet, skipping");
                return;
            }

            try
            {
                token.ThrowIfCancellationRequested();
                BeforeHitContext previewContext = new()
                {
                    Attacker = figure,
                    Target = figure,
                    Grid = _runHolder.Current?.CurrentStage?.Grid ??
                           throw new InvalidOperationException("Grid is null"),
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
                    token.ThrowIfCancellationRequested();
                    PassiveConfig? passiveConfig = _passiveCache.Get(passive.Id);
                    if (passiveConfig != null)
                    {
                        passiveConfigs.Add(passiveConfig);
                    }
                }

                token.ThrowIfCancellationRequested();
                FigureInfoWindow.FigureInfoModel model = new()
                {
                    Figure = figure,
                    InfoConfig = infoConfig,
                    PassiveConfigs = passiveConfigs
                };

                _window.Show(model);
                figure.Stats.Attack.ClearByContext(ModifierSourceContext.PreviewCalculation);
                figure.Stats.Defence.ClearByContext(ModifierSourceContext.PreviewCalculation);
                figure.Stats.Evasion.ClearByContext(ModifierSourceContext.PreviewCalculation);
            }
            catch (OperationCanceledException)
            {
                // Swallow
            }
            catch (Exception ex)
            {
                _logger.Error($"[FigureInfoPreviewService] Failed to show figure info window: {ex.Message}");
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
