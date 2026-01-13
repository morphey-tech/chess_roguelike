using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Selection;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Represents a single stage/level. Controls the flow: spawn board → spawn figures.
    /// Pure gameplay - no Unity dependencies.
    /// </summary>
    public class Stage
    {
        public string Id => _config.Id;
        public string BoardId => _config.BoardId;
        public BoardGrid Grid { get; }

        private readonly StageConfig _config;
        private readonly BoardSpawnService _boardSpawnService;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly MovementService _movementService;
        private readonly SelectionService _selectionService;
        private readonly ILogger<Stage> _logger;

        public Stage(
            StageConfig config,
            BoardGrid grid,
            BoardSpawnService boardSpawnService,
            FigureSpawnService figureSpawnService,
            MovementService movementService,
            SelectionService selectionService,
            ILogService logService)
        {
            _config = config;
            Grid = grid;
            _boardSpawnService = boardSpawnService;
            _figureSpawnService = figureSpawnService;
            _movementService = movementService;
            _selectionService = selectionService;
            _logger = logService.CreateLogger<Stage>();
        }

        public async UniTask BeginAsync()
        {
            _logger.Info($"Stage {Id} beginning, board: {BoardId}");

            await _boardSpawnService.SpawnAsync(BoardId);
            _logger.Info("Board spawned");

            _movementService.Configure(Grid);
            _selectionService.Configure(Grid);

            await SpawnInitialFiguresAsync();
            _logger.Info($"Stage {Id} ready");
        }

        private async UniTask SpawnInitialFiguresAsync()
        {
            // Заглушка для теста, потом из руки будем выставлять
            GridPosition playerPawnPos = new(1, Grid.Width / 2);

            _logger.Info($"Spawning player pawn at ({playerPawnPos.Row}, {playerPawnPos.Column})");

            await _figureSpawnService.SpawnAsync(
                Grid,
                playerPawnPos,
                "pawn",
                Team.Player);

            _logger.Info("Initial figures spawned");
        }
    }
}
