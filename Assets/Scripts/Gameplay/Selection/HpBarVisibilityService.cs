using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Gameplay;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Async;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Selection
{
    /// <summary>
    /// Applies HP bar visibility policy based on config + hover + selection state.
    /// </summary>
    public sealed class HpBarVisibilityService : IDisposable
    {
        private readonly IFigurePresenter _figurePresenter;
        private readonly RunHolder _runHolder;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<HpBarVisibilityService> _logger;
        private readonly IDisposable _subscriptions;
        private readonly CancellationTokenSource _disposeCts = new();

        private GameplayConfig _config = new();
        private int? _hoveredFigureId;
        private int? _selectedFriendlyFigureId;

        public HpBarVisibilityService(
            IFigurePresenter figurePresenter,
            RunHolder runHolder,
            ConfigProvider configProvider,
            ISubscriber<FigureSelectedMessage> selectedSubscriber,
            ISubscriber<FigureDeselectedMessage> deselectedSubscriber,
            ISubscriber<FigureHoverChangedMessage> hoverChangedSubscriber,
            ISubscriber<FigureSpawnedMessage> figureSpawnedSubscriber,
            ISubscriber<FigureDeathMessage> figureDeathSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            ILogService logService)
        {
            _figurePresenter = figurePresenter;
            _runHolder = runHolder;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<HpBarVisibilityService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            selectedSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            deselectedSubscriber.Subscribe(OnFigureDeselected).AddTo(bag);
            hoverChangedSubscriber.Subscribe(OnHoverChanged).AddTo(bag);
            figureSpawnedSubscriber.Subscribe(OnFigureSpawned).AddTo(bag);
            figureDeathSubscriber.Subscribe(OnFigureDeath).AddTo(bag);
            turnChangedSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _subscriptions = bag.Build();

            LoadConfigAsync()
                .AttachExternalCancellation(_disposeCts.Token)
                .ForgetLogged(_logger, "Failed to load gameplay config for HP bar policy", _disposeCts.Token);
        }

        private async UniTask LoadConfigAsync()
        {
            _config = await _configProvider.Get<GameplayConfig>("gameplay_conf", _disposeCts.Token) ?? new GameplayConfig();
            RefreshAll();
            _logger.Info($"HP bar policy loaded: allies={_config.HpBarVisibilityModeAllies}, enemies={_config.HpBarVisibilityModeEnemies}");
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            if (message.Figure != null && message.Figure.Team == Team.Player)
            {
                _selectedFriendlyFigureId = message.Figure.Id;
            }
            else if (message.Figure == null)
            {
                _selectedFriendlyFigureId = null;
            }

            RefreshAll();
        }

        private void OnFigureDeselected(FigureDeselectedMessage message)
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

        private void OnFigureSpawned(FigureSpawnedMessage message)
        {
            ApplyToFigure(message.Figure);
        }

        private void OnFigureDeath(FigureDeathMessage message)
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
            var grid = _runHolder.Current?.CurrentStage?.Grid;
            if (grid == null)
                return;

            foreach (Figure figure in grid.GetAllFigures())
            {
                ApplyToFigure(figure);
            }
        }

        private void ApplyToFigure(Figure figure)
        {
            bool shouldShow = ShouldShowBar(figure);
            if (shouldShow)
                _figurePresenter.ShowFigureHealthBar(figure.Id);
            else
                _figurePresenter.HideFigureHealthBar(figure.Id);
        }

        private bool ShouldShowBar(Figure figure)
        {
            if (figure == null || figure.Stats.IsDead)
                return false;
            return HpBarVisibilityPolicy.ShouldShow(
                _config.HpBarVisibilityModeAllies,
                _config.HpBarVisibilityModeEnemies,
                figure.Team,
                isHovered: _hoveredFigureId == figure.Id,
                hasFriendlySelection: _selectedFriendlyFigureId.HasValue);
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
            _disposeCts.Dispose();
            _subscriptions?.Dispose();
        }
    }
}
