using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class MoveToTargetStep : ITurnStep
    {
        public string Id => "move_to_enemy";

        private readonly MovementService _movementService;
        private readonly VisualPipeline _visualPipeline;
        private readonly ILogger<MoveToTargetStep> _logger;

        public MoveToTargetStep(
            MovementService movementService, 
            VisualPipeline visualPipeline,
            ILogService logService)
        {
            _movementService = movementService;
            _visualPipeline = visualPipeline;
            _logger = logService.CreateLogger<MoveToTargetStep>();
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            _logger.Debug($"MoveToTarget: {context.Actor} looking for targets");

            if (context.Actor.MovedThisTurn)
                return;

            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure? target = targetCell.OccupiedBy;
            if (target == null)
                return;

            GridPosition enemyPos = targetCell.Position;

            int attackRange = context.Actor.Stats.AttackRange;
            
            if (Attack.AttackUtils.IsInRange(context.From, enemyPos, attackRange))
            {
                _logger.Debug($"Already in attack range (range: {attackRange})");
                return;
            }

            GridPosition? movePos =
                FindStraightStepToAttack(context.From, enemyPos, context.Grid,
                    context.Actor.Stats.AttackRange);

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

            // === VISUAL ===
            using (VisualScope scope = _visualPipeline.BeginScope())
            {
                scope.Enqueue(
                    new MoveCommand(
                        new MoveVisualContext(context.Actor.Id, movePos.Value)
                    )
                );

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
                return null;

            int stepRow = Math.Sign(dRow);
            int stepCol = Math.Sign(dCol);

            int distance = Math.Abs(dRow) + Math.Abs(dCol);

            if (distance <= attackRange)
                return null;

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

        /// <summary>
        /// Получить все клетки в указанном радиусе от позиции
        /// </summary>
        private static List<GridPosition> GetCellsInRange(GridPosition center, BoardGrid grid, int range)
        {
            List<GridPosition> cells = new List<GridPosition>();
            
            for (int row = center.Row - range; row <= center.Row + range; row++)
            {
                for (int col = center.Column - range; col <= center.Column + range; col++)
                {
                    GridPosition pos = new GridPosition(row, col);
                    
                    // Проверяем что клетка в пределах сетки и в радиусе
                    if (grid.IsInside(pos) && 
                        Attack.AttackUtils.GetDistance(center, pos) <= range &&
                        pos != center) // исключаем саму клетку врага
                    {
                        cells.Add(pos);
                    }
                }
            }
            
            return cells;
        }
    }
}