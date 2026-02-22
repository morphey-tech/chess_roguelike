using System;
using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Stage coordinator - handles highlights and visual state.
    /// Does NOT contain game logic or turn execution.
    /// 
    /// Responsibilities:
    /// - Highlight management for selection and bonus moves
    /// - SelectTag management on figures
    /// - Reacting to turn changes for cleanup
    /// </summary>
    public class StageService : IDisposable
    {
        private readonly IStageQueryService _query;
        private readonly IStageHighlightRenderer _renderer;
        private readonly ILogger<StageService> _logger;
        private readonly IDisposable _subscriptions;

        private enum StageMode
        {
            Normal,
            BonusMove
        }

        private StageMode _mode;

        [Inject]
        private StageService(
            IStageQueryService query,
            IStageHighlightRenderer renderer,
            ISubscriber<FigureSpawnedMessage> figureSpawnedSubscriber,
            ISubscriber<FigureSelectedMessage> selectionSubscriber,
            ISubscriber<FigureDeselectedMessage> figureDeselectedSubscriber,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ISubscriber<BonusMoveStartedMessage> bonusMoveStartedSubscriber,
            ISubscriber<BonusMoveCompletedMessage> bonusMoveCompletedSubscriber,
            ILogService logService)
        {
            _query = query;
            _renderer = renderer;
            _logger = logService.CreateLogger<StageService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            figureSpawnedSubscriber.Subscribe(OnFigureSpawned).AddTo(bag);
            selectionSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            figureDeselectedSubscriber.Subscribe(OnFigureDeselected).AddTo(bag);
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            bonusMoveStartedSubscriber.Subscribe(OnBonusMoveStarted).AddTo(bag);
            bonusMoveCompletedSubscriber.Subscribe(OnBonusMoveCompleted).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnFigureSpawned(FigureSpawnedMessage message)
        {
            _logger.Debug($"Figure {message.Figure.Id} spawned at ({message.Position.Row}, {message.Position.Column})");
        }

        private void OnBonusMoveStarted(BonusMoveStartedMessage message)
        {
            _mode = StageMode.BonusMove;
            IReadOnlyCollection<GridPosition> moveTargets = _query.GetBonusMoveTargets();
            _renderer.Show(StageSelectionInfo.ForMoves(moveTargets));
            _logger.Debug($"Bonus move started for {message.Actor}, showing highlights");
        }

        private void OnBonusMoveCompleted(BonusMoveCompletedMessage message)
        {
            _mode = StageMode.Normal;
            _renderer.Clear();
            _logger.Debug($"Bonus move completed for {message.Actor}, clearing highlights");
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            if (_mode == StageMode.BonusMove)
            {
                return;
            }

            if (message.Figure != null)
            {
                StageSelectionInfo info = _query.GetSelectionInfo(message.Figure, message.Position);
                _renderer.Show(StageSelectionInfo.ForCombat(info.MoveTargets, info.AttackTargets));
                message.Figure.EnsureComponent(new SelectTag());
                _logger.Debug($"Figure {message.Figure.Id} selected at ({message.Position.Row}, {message.Position.Column})");
            }
            else
            {
                _renderer.Clear();
                _logger.Debug("Selection cleared");
            }
        }
        
        private void OnFigureDeselected(FigureDeselectedMessage message)
        {
            message.Figure.Del<SelectTag>();
            if (_mode != StageMode.BonusMove)
            {
                _renderer.Clear();
            }
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _mode = StageMode.Normal;
            _renderer.Clear();
            _logger.Info($"Turn {message.TurnNumber}: {message.CurrentTeam}'s turn");
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
