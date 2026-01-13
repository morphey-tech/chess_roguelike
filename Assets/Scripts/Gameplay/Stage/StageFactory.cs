using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Selection;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public class StageFactory
    {
        private readonly BoardSpawnService _boardSpawnService;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly MovementService _movementService;
        private readonly SelectionService _selectionService;
        private readonly ILogService _logService;

        [Inject]
        private StageFactory(
            BoardSpawnService boardSpawnService,
            FigureSpawnService figureSpawnService,
            MovementService movementService,
            SelectionService selectionService,
            ILogService logService)
        {
            _boardSpawnService = boardSpawnService;
            _figureSpawnService = figureSpawnService;
            _movementService = movementService;
            _selectionService = selectionService;
            _logService = logService;
        }

        public Stage Create(StageConfig config, BoardGrid grid)
        {
            return new Stage(
                config, 
                grid, 
                _boardSpawnService, 
                _figureSpawnService,
                _movementService,
                _selectionService,
                _logService);
        }
    }
}
