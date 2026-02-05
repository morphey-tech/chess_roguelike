using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Movement;
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
    public class StageService : IStartable, IDisposable
    {
        private readonly RunHolder _runHolder;
        private readonly IBonusMoveController _bonusMoveController;
        private readonly IBoardPresenter _boardPresenter;
        private readonly MovementService _movementService;
        private readonly ILogger<StageService> _logger;
        private readonly IDisposable _subscriptions;

        // Track if we're showing bonus move highlights (to avoid overwriting during selection events)
        private bool _showingBonusMoveHighlights;

        [Inject]
        private StageService(
            RunHolder runHolder,
            IBonusMoveController bonusMoveController,
            IBoardPresenter boardPresenter,
            MovementService movementService,
            ISubscriber<FigureSpawnedMessage> figureSpawnedSubscriber,
            ISubscriber<FigureSelectedMessage> selectionSubscriber,
            ISubscriber<FigureDeselectedMessage> figureDeselectedSubscriber,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ISubscriber<BonusMoveStartedMessage> bonusMoveStartedSubscriber,
            ISubscriber<BonusMoveCompletedMessage> bonusMoveCompletedSubscriber,
            ILogService logService)
        {
            _runHolder = runHolder;
            _bonusMoveController = bonusMoveController;
            _boardPresenter = boardPresenter;
            _movementService = movementService;
            _logger = logService.CreateLogger<StageService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            figureSpawnedSubscriber.Subscribe(OnFigureSpawned).AddTo(bag);
            selectionSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            figureDeselectedSubscriber.Subscribe(OnFigureDeselected).AddTo(bag);
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            bonusMoveStartedSubscriber.Subscribe(OnBonusMoveStarted).AddTo(bag);
            bonusMoveCompletedSubscriber.Subscribe(OnBonusMoveCompleted).AddTo(bag);
            _subscriptions = bag.Build();

            _logger.Info("StageService created");
        }

        void IStartable.Start()
        {
            _logger.Info("StageService started");
        }

        private void OnFigureSpawned(FigureSpawnedMessage message)
        {
            _logger.Debug($"Figure {message.Figure.Id} spawned at ({message.Position.Row}, {message.Position.Column})");
        }

        private void OnBonusMoveStarted(BonusMoveStartedMessage message)
        {
            _showingBonusMoveHighlights = true;
            _logger.Debug($"Bonus move started for {message.Actor}, showing highlights");
            HighlightPositions(_bonusMoveController.GetAvailablePositions());
        }

        private void OnBonusMoveCompleted(BonusMoveCompletedMessage message)
        {
            _showingBonusMoveHighlights = false;
            _logger.Debug($"Bonus move completed for {message.Actor}, clearing highlights");
            ClearHighlights();
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            // Don't change highlights during bonus move
            if (_showingBonusMoveHighlights)
                return;

            if (message.Figure != null)
            {
                _logger.Debug($"Figure {message.Figure.Id} selected at ({message.Position.Row}, {message.Position.Column})");
                HighlightPositions(_movementService.GetAvailableMoves(message.Figure, message.Position));
                message.Figure.EnsureComponent(new SelectTag());
            }
            else
            {
                ClearHighlights();
                _logger.Debug("Selection cleared");
            }
        }
        
        private void OnFigureDeselected(FigureDeselectedMessage message)
        {
            message.Figure.Del<SelectTag>();
            
            // Clear highlights immediately when figure is deselected
            // (before move animation starts)
            if (!_showingBonusMoveHighlights)
            {
                ClearHighlights();
            }
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _logger.Info($"Turn {message.TurnNumber}: {message.CurrentTeam}'s turn");
            
            // Reset highlight state
            _showingBonusMoveHighlights = false;
            
            // Clear any pending highlights
            ClearHighlights();
        }

        private void ClearHighlights()
        {
            HighlightPositions(null);
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
        
        private void HighlightPositions(IEnumerable<MovementStrategyResult> positions)
        {
            if (_movementService.Grid == null)
                return;

            foreach (var boardCell in _movementService.Grid.AllCells())
            {
                var strategyResult = positions?.FirstOrDefault(p =>
                    boardCell.Position.Column == p.Position.Column && boardCell.Position.Row == p.Position.Row) ?? MovementStrategyResult.MakeEmpty();

                if (!strategyResult.CanOccupy())
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.Del<AttackHighlightTag>();
                }
                else
                {
                    if (strategyResult.IsFree)
                        boardCell.EnsureComponent(new HighlightTag());
                    else
                        boardCell.EnsureComponent(new AttackHighlightTag());
                }
            }
        }
    }
}
