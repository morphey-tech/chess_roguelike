using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Turn.Actions.Impl
{
    /// <summary>
    /// Moves toward an enemy target to get into attack range.
    /// Used for melee units that need to close distance.
    /// </summary>
    public sealed class MoveToTargetAction : ICombatAction
    {
        public string Id => "move_to_enemy";

        private readonly MovementService _movementService;
        private readonly VisualPipeline _visualPipeline;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ILogger<MoveToTargetAction> _logger;

        public MoveToTargetAction(
            MovementService movementService,
            VisualPipeline visualPipeline,
            IAttackQueryService attackQueryService,
            ILogService logService)
        {
            _movementService = movementService;
            _visualPipeline = visualPipeline;
            _attackQueryService = attackQueryService;
            _logger = logService.CreateLogger<MoveToTargetAction>();
        }

        public bool CanExecute(ActionContext context)
        {
            if (context.Actor.MovedThisTurn)
                return false;

            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure? target = targetCell?.OccupiedBy;
            if (target == null)
                return false;

            int attackRange = context.Actor.Stats.AttackRange;
            if (AttackUtils.IsInRange(context.From, targetCell.Position, attackRange))
                return false; // Already in range

            GridPosition? movePos = FindStraightStepToAttack(
                context.From, targetCell.Position, context.Grid, attackRange);
            return movePos != null && context.Grid.GetBoardCell(movePos.Value).IsFree;
        }

        public IReadOnlyCollection<ActionPreview> GetPreviews(Figure actor, GridPosition from, BoardGrid grid)
        {
            var targets = new HashSet<ActionPreview>();
            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;

            foreach (Figure enemy in grid.GetFiguresByTeam(enemyTeam))
            {
                BoardCell? cell = grid.FindFigure(enemy);
                if (cell == null)
                {
                    continue;
                }

                GridPosition enemyPos = cell.Position;
                if (_attackQueryService.GetTargets(actor, from, grid).Contains(enemyPos))
                {
                    continue; // Already can attack, skip
                }

                // Check if we can move toward this enemy to get in attack range
                GridPosition? movePos = FindStraightStepToAttack(from, enemyPos, grid, actor.Stats.AttackRange);
                if (movePos != null && grid.GetBoardCell(movePos.Value).IsFree)
                {
                    // Verify we can actually attack from the move position using real CanAttack check
                    if (_attackQueryService.GetTargets(actor, movePos.Value, grid).Contains(enemyPos))
                    {
                        targets.Add(new ActionPreview
                        {
                            MoveTo = movePos,
                            AttackPosition = enemyPos,
                            Target = enemy
                        });
                    }
                }
            }

            return targets;
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            if (!CanExecute(context))
                return;

            _logger.Debug($"MoveToTarget: {context.Actor} moving toward target");

            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure? target = targetCell?.OccupiedBy;
            if (target == null)
                return;

            GridPosition enemyPos = targetCell.Position;
            int attackRange = context.Actor.Stats.AttackRange;

            GridPosition? movePos = FindStraightStepToAttack(
                context.From, enemyPos, context.Grid, attackRange);

            if (movePos == null)
            {
                _logger.Debug("No valid step to melee");
                return;
            }

            BoardCell cell = context.Grid.GetBoardCell(movePos.Value);
            if (!cell.IsFree)
                return;

            // === DOMAIN ===
            _movementService.MoveFigure(context.From, movePos.Value);
            context.Actor.MovedThisTurn = true;
            context.ActionExecuted = true;

            // === VISUAL ===
            using (VisualScope scope = _visualPipeline.BeginScope())
            {
                scope.Enqueue(new MoveCommand(new MoveVisualContext(context.Actor.Id, movePos.Value)));
                await scope.PlayAsync();
            }

            context.From = movePos.Value;
            _logger.Debug($"Moved to melee position {movePos}");
        }

        private static GridPosition? FindStraightStepToAttack(
            GridPosition from,
            GridPosition enemy,
            BoardGrid grid,
            int attackRange)
        {
            int dRow = enemy.Row - from.Row;
            int dCol = enemy.Column - from.Column;

            if (dRow != 0 && dCol != 0)
                return null; // Not on straight line

            int stepRow = Math.Sign(dRow);
            int stepCol = Math.Sign(dCol);

            int distance = Math.Abs(dRow) + Math.Abs(dCol);

            if (distance <= attackRange)
                return null; // Already in range

            int stepsToTarget = distance - attackRange;
            GridPosition current = from;

            for (int i = 1; i <= stepsToTarget; i++)
            {
                current = new GridPosition(
                    current.Row + stepRow,
                    current.Column + stepCol
                );

                if (!grid.IsInside(current))
                    return null;

                BoardCell cell = grid.GetBoardCell(current);
                if (!cell.IsFree)
                    return null;
            }

            return current;
        }
    }
}
