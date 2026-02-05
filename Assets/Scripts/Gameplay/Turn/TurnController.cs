using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Turn.Execution;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn
{
    /// <summary>
    /// Pure orchestrator for executing player turns.
    /// 
    /// Responsibilities (exactly 2):
    /// 1. Manage interaction lock scope
    /// 2. Orchestrate execution flow: TurnExecutor → BonusMoveSession → EndTurn
    /// 
    /// Does NOT:
    /// - Handle clicks
    /// - Manage state machines
    /// - Play visuals directly
    /// - Publish UI messages
    /// 
    /// All complex interaction logic is delegated to specialized components.
    /// </summary>
    public sealed class TurnController : ITurnController
    {
        private readonly IInteractionLock _interactionLock;
        private readonly ITurnExecutor _turnExecutor;
        private readonly IBonusMoveSession _bonusMoveSession;
        private readonly TurnSystem _turnSystem;
        private readonly RunHolder _runHolder;
        private readonly ILogger<TurnController> _logger;

        [Inject]
        public TurnController(
            IInteractionLock interactionLock,
            ITurnExecutor turnExecutor,
            IBonusMoveSession bonusMoveSession,
            TurnSystem turnSystem,
            RunHolder runHolder,
            ILogService logService)
        {
            _interactionLock = interactionLock;
            _turnExecutor = turnExecutor;
            _bonusMoveSession = bonusMoveSession;
            _turnSystem = turnSystem;
            _runHolder = runHolder;
            _logger = logService.CreateLogger<TurnController>();
            
            _logger.Info("TurnController created (orchestrator only)");
        }

        public async UniTask ExecuteMoveAsync(Figure actor, GridPosition from, GridPosition to)
        {
            await ExecuteTurnAsync(actor, from, to);
        }

        public async UniTask ExecuteAttackAsync(Figure actor, GridPosition from, GridPosition to)
        {
            await ExecuteTurnAsync(actor, from, to);
        }

        private async UniTask ExecuteTurnAsync(Figure actor, GridPosition from, GridPosition to)
        {
            BoardGrid grid = GetCurrentGrid();
            if (grid == null)
            {
                _logger.Error("Cannot execute turn: no grid available");
                return;
            }

            if (actor == null)
            {
                _logger.Error("Cannot execute turn: actor is null");
                return;
            }

            _logger.Info($"Executing turn for {actor.Id}: ({from.Row},{from.Column}) -> ({to.Row},{to.Column})");

            // Lock scope covers entire turn including bonus move
            using (IDisposable lockHandle = _interactionLock.Acquire("turn-execution"))
            {
                try
                {
                    // 1. Execute the main turn
                    TurnExecutionResult result = await _turnExecutor.ExecuteAsync(actor, from, to, grid);

                    if (!result.Success)
                    {
                        _logger.Debug($"Turn execution failed for {actor}");
                        return;
                    }

                    _logger.Info($"Turn executed. Final pos: ({result.ActorFinalPosition.Row},{result.ActorFinalPosition.Column}), " +
                                 $"BonusMove: {(result.BonusMoveDistance.HasValue ? result.BonusMoveDistance.Value.ToString() : "none")}");

                    // 2. Delegate bonus move to session (if granted)
                    if (result.BonusMoveDistance.HasValue && result.BonusMoveDistance.Value > 0)
                    {
                        await _bonusMoveSession.RunAsync(actor, result.ActorFinalPosition, result.BonusMoveDistance.Value, grid);
                    }

                    // 3. End the turn
                    _turnSystem.EndTurn();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Turn execution error: {ex.Message}");
                    throw;
                }
            }
        }

        private BoardGrid GetCurrentGrid()
        {
            return _runHolder.Current?.CurrentStage?.Grid;
        }
    }
}
