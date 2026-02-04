using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Turn.Steps;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn.Execution
{
    public sealed class TurnExecutor : ITurnExecutor
    {
        private readonly TurnPatternResolver _patternResolver;
        private readonly MovementService _movementService;
        private readonly ILogger<TurnExecutor> _logger;

        [Inject]
        private TurnExecutor(
            TurnPatternResolver patternResolver,
            MovementService movementService,
            ILogService logService)
        {
            _patternResolver = patternResolver;
            _movementService = movementService;
            _logger = logService.CreateLogger<TurnExecutor>();
        }

        public async UniTask<TurnExecutionResult> ExecuteAsync(Figure actor, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (actor.TurnPattern == null)
            {
                _logger.Error($"Figure {actor} has no TurnPatternSet!");
                return TurnExecutionResult.Failed;
            }

            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;
            List<Figure> enemies = grid.GetFiguresByTeam(enemyTeam).ToList();

            ActionContext context = new()
            {
                Actor = actor,
                Grid = grid,
                From = from,
                To = to,
                Enemies = enemies,
                MovementService = _movementService
            };

            // DEBUG: Verify ActionContext is fresh
            _logger.Info($"[DEBUG] NEW ActionContext for {actor.Id}: BonusMoveDistance={context.BonusMoveDistance?.ToString() ?? "null"} (should be null)");

            ITurnStep step = _patternResolver.Resolve(actor, actor.TurnPattern, context);

            if (step == null)
            {
                _logger.Debug($"No valid pattern for {actor}");
                return TurnExecutionResult.Failed;
            }

            _logger.Info($"{actor} executing pattern: {step.Id}");
            await step.ExecuteAsync(context);
            
            // DEBUG: Check after step execution
            _logger.Info($"[DEBUG] AFTER step for {actor.Id}: BonusMoveDistance={context.BonusMoveDistance?.ToString() ?? "null"}");

            GridPosition finalPosition = context.From;
            
            TurnExecutionResult result = new()
            {
                Success = true,
                ActorFinalPosition = finalPosition,
                BonusMoveDistance = context.BonusMoveDistance,
                KilledTarget = context.LastAttackKilledTarget
            };

            _logger.Info($"Turn executed. Final pos: ({finalPosition.Row},{finalPosition.Column}), " +
                         $"BonusMove: {result.BonusMoveDistance?.ToString() ?? "none"}");

            return result;
        }
    }
}
