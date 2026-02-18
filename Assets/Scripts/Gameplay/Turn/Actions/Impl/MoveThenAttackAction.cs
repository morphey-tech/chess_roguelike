using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Turn.Actions.Impl
{
    /// <summary>
    /// Composite action: move along a line toward an enemy, then attack.
    /// Used for units like Rook that can move then attack in one turn.
    /// 
    /// Contract: From = current position, To = enemy cell to attack.
    /// The action computes the intermediate move position along the line.
    /// </summary>
    public sealed class MoveThenAttackAction : ICombatAction
    {
        public string Id { get; }

        private readonly MovementService _movementService;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ICombatAction _moveAction;
        private readonly ICombatAction _attackAction;

        public MoveThenAttackAction(
            string id,
            MovementService movementService,
            IAttackQueryService attackQueryService,
            ICombatAction moveAction,
            ICombatAction attackAction)
        {
            Id = id;
            _movementService = movementService;
            _attackQueryService = attackQueryService;
            _moveAction = moveAction;
            _attackAction = attackAction;
        }

        public bool CanExecute(ActionContext context)
        {
            BoardCell? findFigure = context.Grid.FindFigure(context.Actor);
            Debug.LogError($"{findFigure.Position.Row} - {findFigure.Position.Column}");
            Debug.LogError($"{context.From.Row} - {context.From.Column}");
            return GetPreviews(context.Actor, context.From, context.Grid)
                .Any(p => p.AttackPosition == context.To);
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

                // Only consider enemies on the same row or column (straight line)
                if (from.Row != enemyPos.Row && from.Column != enemyPos.Column)
                    continue;

                // Get all reachable cells along the line toward the enemy
                IEnumerable<GridPosition> lineMoves = _movementService.GetAvailableMoves(actor, from)
                    .Where(m => m.CanOccupy() && m.IsFree && IsOnLine(from, enemyPos, m.Position))
                    .Select(m => m.Position);

                // Check if from any of these positions we can attack the enemy
                foreach (GridPosition mid in lineMoves)
                {
                    if (_attackQueryService.GetTargets(actor, mid, grid).Contains(enemyPos))
                    {
                        targets.Add(new ActionPreview
                        {
                            MoveTo = mid,
                            AttackPosition = enemyPos,
                            Target = enemy
                        });
                        break;
                    }
                }

                // Also check if we can attack from current position
                if (_attackQueryService.GetTargets(actor, from, grid).Contains(enemyPos))
                {
                    targets.Add(new ActionPreview
                    {
                        MoveTo = from,
                        AttackPosition = enemyPos,
                        Target = enemy
                    });
                }
            }

            return targets;
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            if (!CanExecute(context))
                return;

            GridPosition attackTarget = context.To;
            GridPosition moveTo = ComputeMoveTo(context.Actor, context.From, attackTarget, context.Grid);

            // Move if needed
            if (moveTo != context.From)
            {
                context.To = moveTo;
                await _moveAction.ExecuteAsync(context);
                context.From = moveTo;
            }

            // Attack
            context.To = attackTarget;
            await _attackAction.ExecuteAsync(context);
            context.ActionExecuted = true;
        }

        private GridPosition ComputeMoveTo(Figure actor, GridPosition from, GridPosition attackTarget, BoardGrid grid)
        {
            // If we can attack from current position, don't move
            if (_attackQueryService.GetTargets(actor, from, grid).Contains(attackTarget))
                return from;

            // Walk along the line toward attackTarget, find closest cell from which we can attack
            int dRow = attackTarget.Row - from.Row;
            int dCol = attackTarget.Column - from.Column;

            if (dRow != 0 && dCol != 0)
                return from; // Not on same line

            // Get all reachable moves along the line
            var lineMoves = _movementService.GetAvailableMoves(actor, from)
                .Where(m => m.CanOccupy() && m.IsFree && IsOnLine(from, attackTarget, m.Position))
                .OrderBy(m => Attack.AttackUtils.GetDistance(from, m.Position))
                .ToList();

            // Find the closest position from which we can attack
            foreach (var move in lineMoves)
            {
                if (_attackQueryService.GetTargets(actor, move.Position, grid).Contains(attackTarget))
                {
                    return move.Position;
                }
            }

            return from;
        }

        private static bool IsOnLine(GridPosition from, GridPosition to, GridPosition p)
        {
            return (from.Row == to.Row && from.Row == p.Row) ||
                   (from.Column == to.Column && from.Column == p.Column);
        }
    }
}
