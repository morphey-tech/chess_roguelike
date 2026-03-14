using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Stage.Analysis;
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

        public void Show(StageActorAnalysis analysis)
        {
            if (_movementService.Grid == null)
            {
                _logger.Warning("StageHighlightRenderer: Grid is null");
                return;
            }

            int moveCount = 0;
            int attackCount = 0;
            int underAttackCount = 0;
            var moveTargets = analysis.MoveTargets as HashSet<GridPosition>
                              ?? new HashSet<GridPosition>(analysis.MoveTargets);
            var attackTargets = analysis.AttackTargets as HashSet<GridPosition>
                                ?? new HashSet<GridPosition>(analysis.AttackTargets);
            var dangerousCells = analysis.DangerousCells as HashSet<GridPosition>
                                 ?? new HashSet<GridPosition>(analysis.DangerousCells);

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
                if (dangerousCells.Contains(pos))
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.Del<AttackHighlightTag>();
                    boardCell.EnsureComponent(new UnderAttackHighlightTag());
                    underAttackCount++;
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

            _logger.Info($"StageHighlightRenderer: move={moveCount}, attack={attackCount}, dangerous={underAttackCount}");
        }

        public void ShowMovesOnly(IReadOnlyCollection<GridPosition> moveTargets)
        {
            if (_movementService.Grid == null)
            {
                _logger.Warning("StageHighlightRenderer: Grid is null");
                return;
            }

            int moveCount = 0;
            var moves = moveTargets as HashSet<GridPosition>
                        ?? new HashSet<GridPosition>(moveTargets);

            foreach (var boardCell in _movementService.Grid.AllCells())
            {
                var pos = boardCell.Position;

                if (moves.Contains(pos))
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

            _logger.Info($"StageHighlightRenderer: move={moveCount}");
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
