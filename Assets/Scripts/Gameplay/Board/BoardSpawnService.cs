using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using VContainer;

namespace Project.Gameplay.Gameplay.Board
{
    /// <summary>
    /// Pure gameplay service - no Unity dependencies.
    /// Delegates visualization to IBoardView.
    /// </summary>
    public sealed class BoardSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly VisualPipeline _visualPipeline;
        private readonly ILogger<BoardSpawnService> _logger;

        [Inject]
        private BoardSpawnService(
            ConfigProvider configProvider,
            VisualPipeline visualPipeline,
            ILogService logService)
        {
            _configProvider = configProvider;
            _visualPipeline = visualPipeline;
            _logger = logService.CreateLogger<BoardSpawnService>();
        }

        public async UniTask<BoardGrid> GetGridAsync(string boardId)
        {
            BoardConfigRepository repo = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig board = repo.Get(boardId) ?? throw new Exception($"Board '{boardId}' not found");
            return new BoardGrid(board.Width, board.Height);
        }

        public async UniTask SpawnVisualAsync(BoardGrid grid, string boardId)
        {
            _logger.Info($"Spawning board visual {boardId}");
            BoardConfigRepository repo = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig board = repo.Get(boardId) ?? throw new Exception($"Board '{boardId}' not found");
            
            string[,] map = board.GetBoard2D();
            List<CellSpawnRequest> requests = CollectCellRequests(grid, map);
            
            using VisualScope scope = _visualPipeline.BeginScope();
            scope.Enqueue(new SpawnBoardAssetCommand(board.BackgroundAssetKey, board.BackgroundAppearStrategyId));
            scope.Enqueue(new SpawnBoardAssetCommand(board.BoardAssetKey, board.BoardAppearStrategyId));
            scope.Enqueue(new SpawnBoardCellsCommand(requests, board.CellsAppearStrategyId));
            await scope.PlayAsync();
            _logger.Info("Board visual created");
        }

        private static List<CellSpawnRequest> CollectCellRequests(BoardGrid grid, string[,] map)
        {
            List<CellSpawnRequest> list = new(grid.Width * grid.Height);
            for (int r = 0; r < grid.Height; r++)
            {
                for (int c = 0; c < grid.Width; c++)
                {
                    GridPosition gridPosition = new(r, c);
                    BoardCell cell = grid.GetBoardCell(gridPosition);
                    string skinId = map[r, c];
                    list.Add(new CellSpawnRequest(cell, gridPosition, skinId));
                }
            }
            return list;
        }
    }
}
