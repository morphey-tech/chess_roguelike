using Project.Core.Core.Combat;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.Actions;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Movement;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageQueryService : IStageQueryService
    {
        private readonly MovementService _movementService;
        private readonly IBonusMoveController _bonusMoveController;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ILogger<StageQueryService> _logger;

        [Inject]
        private StageQueryService(
            MovementService movementService,
            IAttackQueryService attackQueryService,
            IBonusMoveController bonusMoveController,
            TurnPatternResolver patternResolver,
            ILogService logService)
        {
            _movementService = movementService;
            _attackQueryService = attackQueryService;
            _bonusMoveController = bonusMoveController;
            _logger = logService.CreateLogger<StageQueryService>();
        }

        public StageSelectionInfo GetSelectionInfo(Figure? actor, GridPosition pos)
        {
            if (actor?.TurnPattern == null)
            {
                return new StageSelectionInfo(null, null);
            }

            HashSet<GridPosition> moveTargets = new HashSet<GridPosition>();
            HashSet<GridPosition> attackTargets = new HashSet<GridPosition>();

            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;
            List<Figure> enemies = _movementService.Grid.GetFiguresByTeam(enemyTeam).ToList();

            BoardGrid grid = _movementService.Grid;
            foreach (TurnPatternDescription? patternDesc in actor.TurnPattern.Patterns)
            {
                ICombatAction action = patternDesc.Action;
                IReadOnlyCollection<ActionPreview> previews = action.GetPreviews(actor, pos, grid);

                foreach (ActionPreview? preview in previews)
                {
                    if (preview.MoveTo.HasValue)
                    {
                        moveTargets.Add(preview.MoveTo.Value);
                    }
                    if (preview.Target != null)
                    {
                        BoardCell targetCell = grid.FindFigure(preview.Target)!;
                        attackTargets.Add(targetCell.Position);
                    }
                }
            }

            return new StageSelectionInfo(moveTargets, attackTargets);
        }

        public IReadOnlyCollection<GridPosition> GetBonusMoveTargets()
        {
            var result = new HashSet<GridPosition>();
            foreach (MovementStrategyResult move in _bonusMoveController.GetAvailablePositions())
            {
                if (move.CanOccupy() && move.IsFree)
                    result.Add(move.Position);
            }
            return result;
        }

        public IReadOnlyCollection<GridPosition> GetUnderAttackCells(Figure actor, GridPosition pos)
        {
            HashSet<GridPosition> underAttackCells = new();
            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;
            BoardGrid grid = _movementService.Grid;

            // Получаем все клетки, куда может пойти фигура
            StageSelectionInfo selectionInfo = GetSelectionInfo(actor, pos);
            
            List<Figure> enemies = grid.GetFiguresByTeam(enemyTeam).ToList();
            _logger.Info($"GetUnderAttackCells: {enemies.Count} enemies, actor at ({pos.Row},{pos.Column}), {selectionInfo.MoveTargets.Count} move targets");
            
            // Проверяем текущую позицию - если фигура под атакой, подсвечиваем
            foreach (Figure enemy in enemies)
            {
                BoardCell? enemyCell = grid.FindFigure(enemy);
                if (enemyCell == null)
                    continue;

                bool canAttack = _attackQueryService.CanAttackCell(enemy, enemyCell.Position, pos, grid);
                _logger.Info($"  Enemy {enemy.Id} at ({enemyCell.Position.Row},{enemyCell.Position.Column}) can attack current pos ({pos.Row},{pos.Column}): {canAttack}");
                if (canAttack)
                {
                    _logger.Info($"  Current cell ({pos.Row},{pos.Column}) is under attack by enemy {enemy.Id}");
                    underAttackCells.Add(pos);
                    break;
                }
            }
            
            // Для каждой клетки, куда можем пойти, проверяем, может ли враг атаковать её
            foreach (GridPosition cellToCheck in selectionInfo.MoveTargets)
            {
                // Пропускаем текущую позицию - уже проверили
                if (cellToCheck == pos)
                    continue;
                    
                foreach (Figure enemy in enemies)
                {
                    BoardCell? enemyCell = grid.FindFigure(enemy);
                    if (enemyCell == null)
                    {
                        _logger.Warning($"  Enemy {enemy.Id} not found on grid");
                        continue;
                    }

                    // Проверяем, может ли враг атаковать эту клетку
                    if (_attackQueryService.CanAttackCell(enemy, enemyCell.Position, cellToCheck, grid))
                    {
                        _logger.Info($"  Cell ({cellToCheck.Row},{cellToCheck.Column}) is under attack by enemy {enemy.Id}");
                        underAttackCells.Add(cellToCheck);
                        break; // Достаточно одного врага
                    }
                }
            }

            _logger.Info($"GetUnderAttackCells: total {underAttackCells.Count} under-attack cells: {string.Join(",", underAttackCells.Select(p => $"({p.Row},{p.Column})"))}");
            return underAttackCells;
        }

        public void Clear()
        {
            // no cached state
        }
    }
}
