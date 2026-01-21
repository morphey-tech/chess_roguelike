using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
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

        public async UniTask SpawnAsync(string boardId)
        {
            _logger.Info($"Spawning board {boardId}");

            BoardConfigRepository repo = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig board = repo.GetBy(boardId) ?? throw new Exception($"Board '{boardId}' not found");

            _boardPresenter.Clear();
            
            char[,] map = board.GetBoard2D();

            for (int r = 0; r < board.Height; r++)
            {
                for (int c = 0; c < board.Width; c++)
                {
                    int entId = IdGetter.MakeId();
                    char symbol = map[r, c];
                    string skinId = symbol.ToString();
                    _boardPresenter.CreateCell(entId, new GridPosition(r, c), skinId);
                }
            }

            if (!string.IsNullOrEmpty(board.AppearStrategyId))
            {
                await _boardPresenter.PlayBoardAppearAsync(board.AppearStrategyId);
            }

            _logger.Info("Board created");
        }
    }
}
