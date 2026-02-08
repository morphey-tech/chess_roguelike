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
            var moveTargets = info.MoveTargets as HashSet<GridPosition> 
                              ?? new HashSet<GridPosition>(info.MoveTargets);
            var attackTargets = info.AttackTargets as HashSet<GridPosition> 
                                ?? new HashSet<GridPosition>(info.AttackTargets);

            foreach (var boardCell in _movementService.Grid.AllCells())
            {
                var pos = boardCell.Position;

                if (attackTargets.Contains(pos))
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.EnsureComponent(new AttackHighlightTag());
                    attackCount++;
                    continue;
                }

                if (moveTargets.Contains(pos))
                {
                    boardCell.Del<AttackHighlightTag>();
                    boardCell.EnsureComponent(new HighlightTag());
                    moveCount++;
                }
                else
                {
                    boardCell.Del<HighlightTag>();
                    boardCell.Del<AttackHighlightTag>();
                }
            }

            _logger.Info($"StageHighlightRenderer: move={moveCount}, attack={attackCount}");
        }

        public void Clear()
        {
            Show(new StageSelectionInfo(null, null));
        }
    }
}
