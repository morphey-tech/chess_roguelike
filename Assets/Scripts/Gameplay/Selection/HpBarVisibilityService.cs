using Project.Core.Core.Combat;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Gameplay;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Extensions;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.Gameplay.Turn;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Selection
{
    /// <summary>
    /// Applies HP bar visibility policy based on config + hover + selection state.
    /// </summary>
    public sealed class HpBarVisibilityService : IInitializable, IDisposable
    {
        private readonly IFigurePresenter _figurePresenter;
        private readonly RunHolder _runHolder;
        private readonly ConfigProvider _configProvider;
        private readonly ISubscriber<FigureHoverChangedMessage> _hoverChangedSubscriber;
        private readonly ISubscriber<string, FigureSelectMessage> _figureSelectSubscriber;
        private readonly ISubscriber<string, FigureBoardMessage> _figureBoardSubscriber;
        private readonly ISubscriber<FigureDiedMessage> _figureDeathSubscriber;
        private readonly ISubscriber<TurnChangedMessage> _turnChangedSubscriber;
        private readonly ISubscriber<PreparePhaseCompletedMessage> _prepareCompletedSubscriber;
        private readonly ISubscriber<StageStartedMessage> _stageStartedSubscriber;
        private readonly ILogger<HpBarVisibilityService> _logger;

        private GameplayConfig _config = new();
        private int? _hoveredFigureId;
        private int? _selectedFriendlyFigureId;
        private bool _isPreparePhase = true;
        
        private readonly CancellationTokenSource _disposeCts = new();
        private IDisposable _subscriptions = null!;

        [Inject]
        private HpBarVisibilityService(
            IFigurePresenter figurePresenter,
            RunHolder runHolder,
            ConfigProvider configProvider,
            ISubscriber<FigureHoverChangedMessage> hoverChangedSubscriber,
            ISubscriber<string, FigureSelectMessage> figureSelectSubscriber,
            ISubscriber<string, FigureBoardMessage> figureBoardSubscriber,
            ISubscriber<FigureDiedMessage> figureDeathSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            ISubscriber<PreparePhaseCompletedMessage> prepareCompletedSubscriber,
            ISubscriber<StageStartedMessage> stageStartedSubscriber,
            ILogService logService)
        {
            _figurePresenter = figurePresenter;
            _runHolder = runHolder;
            _configProvider = configProvider;
            _hoverChangedSubscriber = hoverChangedSubscriber;
            _figureSelectSubscriber = figureSelectSubscriber;
            _figureBoardSubscriber = figureBoardSubscriber;
            _figureDeathSubscriber = figureDeathSubscriber;
            _turnChangedSubscriber = turnChangedSubscriber;
            _prepareCompletedSubscriber = prepareCompletedSubscriber;
            _stageStartedSubscriber = stageStartedSubscriber;
            _logger = logService.CreateLogger<HpBarVisibilityService>();


        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _figureBoardSubscriber.Subscribe(FigureSelectMessage.SELECTED, OnFigureSelected).AddTo(bag);
            _figureBoardSubscriber.Subscribe(FigureSelectMessage.DESELECTED, OnFigureDeselected).AddTo(bag);
            _figureBoardSubscriber.Subscribe(FigureBoardMessage.SPAWNED, OnFigureSpawned).AddTo(bag);
            _figureDeathSubscriber.Subscribe(OnFigureDeath).AddTo(bag);
            _hoverChangedSubscriber.Subscribe(OnHoverChanged).AddTo(bag);
            _turnChangedSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _prepareCompletedSubscriber.Subscribe(OnPrepareCompleted).AddTo(bag);
            _stageStartedSubscriber.Subscribe(OnStageStarted).AddTo(bag);
            _subscriptions = bag.Build();
            
            LoadConfigAsync().AttachExternalCancellation(_disposeCts.Token)
                .ForgetLogged(_logger, "Failed to load gameplay config for HP bar policy", 
                    _disposeCts.Token);
        }

        private async UniTask LoadConfigAsync()
        {
            _config = await _configProvider.Get<GameplayConfig>("gameplay_conf", _disposeCts.Token) ?? new GameplayConfig();
            RefreshAll();
            _logger.Info($"HP bar policy loaded: allies={_config.HpBarVisibilityModeAllies}, enemies={_config.HpBarVisibilityModeEnemies}, hideDuringPrepare={_config.HideHpBarsDuringPrepare}");
        }

        private void OnPrepareCompleted(PreparePhaseCompletedMessage message)
        {
            _isPreparePhase = false;
            RefreshAll();
        }

        private void OnStageStarted(StageStartedMessage message)
        {
            _isPreparePhase = true;
        }

        private void OnFigureSelected(FigureBoardMessage message)
        {
            if (message.Figure is { Team: Team.Player })
            {
                _selectedFriendlyFigureId = message.Figure.Id;
            }
            else if (message.Figure == null)
            {
                _selectedFriendlyFigureId = null;
            }

            RefreshAll();
        }

        private void OnFigureDeselected(FigureBoardMessage message)
        {
            if (_selectedFriendlyFigureId.HasValue && _selectedFriendlyFigureId.Value == message.Figure.Id)
                _selectedFriendlyFigureId = null;

            RefreshAll();
        }

        private void OnHoverChanged(FigureHoverChangedMessage message)
        {
            _hoveredFigureId = message.FigureId;
            RefreshAll();
        }

        private void OnFigureSpawned(FigureBoardMessage boardMessage)
        {
            ApplyToFigure(boardMessage.Figure);
        }

        private void OnFigureDeath(FigureDiedMessage message)
        {
            if (_hoveredFigureId == message.FigureId)
                _hoveredFigureId = null;
            if (_selectedFriendlyFigureId == message.FigureId)
                _selectedFriendlyFigureId = null;

            RefreshAll();
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _selectedFriendlyFigureId = null;
            RefreshAll();
        }

        private void RefreshAll()
        {
            BoardGrid? grid = _runHolder.Current?.CurrentStage?.Grid;
            if (grid == null)
            {
                return;
            }

            foreach (Figure figure in grid.GetAllFigures())
            {
                ApplyToFigure(figure);
            }
        }

        private void ApplyToFigure(Figure figure)
        {
            bool shouldShow = ShouldShowBar(figure);
            if (shouldShow)
            {
                _figurePresenter.ShowFigureHealthBar(figure.Id);
            }
            else
            {
                _figurePresenter.HideFigureHealthBar(figure.Id);
            }
        }

        private bool ShouldShowBar(Figure? figure)
        {
            if (figure == null || figure.Stats.IsDead)
            {
                return false;
            }

            // Скрываем HP бары на стадии подготовки, если включена опция
            if (_config.HideHpBarsDuringPrepare && _isPreparePhase)
            {
                return false;
            }

            return HpBarVisibilityPolicy.ShouldShow(
                _config.HpBarVisibilityModeAllies,
                _config.HpBarVisibilityModeEnemies,
                figure.Team,
                isHovered: _hoveredFigureId == figure.Id,
                hasFriendlySelection: _selectedFriendlyFigureId.HasValue);
        }

        void IDisposable.Dispose()
        {
            _disposeCts.Cancel();
            _disposeCts.Dispose();
            _subscriptions?.Dispose();
        }
    }
}
