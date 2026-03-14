using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Figures;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageHighlightRenderer : IStageHighlightRenderer
    {
        private readonly MovementService _movementService;
        private readonly ILogger<StageHighlightRenderer> _logger;

        [Inject]
        public StageHighlightRenderer(MovementService movementService, ILogService logService)
        {
            _movementService = movementService;
            _logger = logService.CreateLogger<StageHighlightRenderer>();
        }

        public void Show(StageSelectionInfo info)
        {
            if (_movementService.Grid == null)
            {
                _logger.Warning("StageHighlightRenderer: Grid is null");
                return;
            }

            int moveCount = 0;
            int attackCount = 0;
            int underAttackCount = 0;
            var moveTargets = info.MoveTargets as HashSet<GridPosition>
                              ?? new HashSet<GridPosition>(info.MoveTargets);
            var attackTargets = info.AttackTargets as HashSet<GridPosition>
                                ?? new HashSet<GridPosition>(info.AttackTargets);
            var underAttackTargets = info.UnderAttackTargets as HashSet<GridPosition>
                                     ?? new HashSet<GridPosition>(info.UnderAttackTargets);

            _logger.Info($"StageHighlightRenderer: moveTargets={moveTargets.Count}, attackTargets={attackTargets.Count}, underAttackTargets={underAttackTargets.Count}");

            foreach (var boardCell in _movementService.Grid.AllCells())
            {
                var pos = boardCell.Position;

                // Приоритет 1: Клетки атаки (наивысший приоритет)
                if (attackTargets.Contains(pos))
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.Del<UnderAttackHighlightTag>();
                    boardCell.EnsureComponent(new AttackHighlightTag());
                    attackCount++;
                    continue;
                }

                // Приоритет 2: Клетка под атакой (включая текущую позицию фигуры)
                if (underAttackTargets.Contains(pos))
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.Del<AttackHighlightTag>();
                    boardCell.EnsureComponent(new UnderAttackHighlightTag());
                    underAttackCount++;
                    _logger.Info($"  UnderAttack: ({pos.Row},{pos.Column})");
                    continue;
                }

                // Приоритет 3: Клетки для хода
                if (moveTargets.Contains(pos))
                {
                    boardCell.Del<AttackHighlightTag>();
                    boardCell.Del<UnderAttackHighlightTag>();
                    boardCell.EnsureComponent(new HighlightTag());
                    moveCount++;
                }
                else
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.Del<AttackHighlightTag>();
                    boardCell.Del<UnderAttackHighlightTag>();
                }
            }

            _logger.Info($"StageHighlightRenderer: move={moveCount}, attack={attackCount}, underAttack={underAttackCount}");
        }

        public void Clear()
        {
            if (_movementService.Grid == null)
            {
                return;
            }

            // Очищаем все подсветки со всех клеток
            foreach (var boardCell in _movementService.Grid.AllCells())
            {
                boardCell.Del<HighlightTag>();
                boardCell.Del<AttackHighlightTag>();
                boardCell.Del<UnderAttackHighlightTag>();
            }

            _logger.Info("StageHighlightRenderer: cleared all highlights");
        }
    }
}
