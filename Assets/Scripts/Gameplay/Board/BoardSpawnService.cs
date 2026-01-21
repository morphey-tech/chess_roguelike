using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Presentations;
using VContainer;

namespace Project.Gameplay.Gameplay.Board
{
    /// <summary>
    /// Pure gameplay service - no Unity dependencies.
    /// Delegates visualization to IBoardView.
    /// </summary>
    public sealed class BoardSpawnService
    {
        public IBoardPresenter BoardPresenter => _boardPresenter;
        private readonly ConfigProvider _configProvider;
        private readonly IBoardPresenter _boardPresenter;
        private readonly ILogger<BoardSpawnService> _logger;

        [Inject]
        private BoardSpawnService(
            ConfigProvider configProvider,
            IBoardPresenter boardPresenter,
            ILogService logService)
        {
            _configProvider = configProvider;
            _boardPresenter = boardPresenter;
            _logger = logService.CreateLogger<BoardSpawnService>();
        }

        public async UniTask<BoardGrid> SpawnAsync(string boardId)
        {
            _logger.Info($"Spawning board {boardId}");

            BoardConfigRepository repo = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig board = repo.GetBy(boardId) ?? throw new Exception($"Board '{boardId}' not found");
            
            BoardGrid grid = new(board.Width, board.Height);
            char[,] map = board.GetBoard2D();
            MakeBoardView(grid, map);

            if (!string.IsNullOrEmpty(board.AppearStrategyId))
            {
                await _boardPresenter.PlayBoardAppearAsync(board.AppearStrategyId);
            }

            _logger.Info("Board created");
            return grid;
        }

        private void MakeBoardView(BoardGrid grid, char[,] map)
        {
            _boardPresenter.Clear();
            for (var r = 0; r < grid.Height; r++)
            {
                for (var c = 0; c < grid.Width; c++)
                {
                    var gridPosition = new GridPosition(r, c);
                    var cell = grid.GetBoardCell(gridPosition);
                    var symbol = map[r, c];
                    var skinId = symbol.ToString();
                    _boardPresenter.CreateCell(cell, gridPosition, skinId);
                }
            }
        }
    }
}
