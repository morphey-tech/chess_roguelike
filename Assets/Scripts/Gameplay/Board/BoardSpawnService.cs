using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs;
using Project.Gameplay.Configs;

namespace Project.Gameplay.Gameplay.Board
{
    public sealed class BoardSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly CellsSpawnService _cellSpawner;
        
        public BoardSpawnService(ConfigProvider configProvider, CellsSpawnService cellSpawner)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _cellSpawner = cellSpawner ?? throw new ArgumentNullException(nameof(cellSpawner));
        }

        public async UniTask SpawnAsync(string boardId)
        {
            BoardConfigRepository? repository = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig? boardConfig = repository.GetBy(boardId);
            if (boardConfig == null)
            {
                throw new Exception($"Board config with key {boardId} not found.");
            }

            char[,] board2D = boardConfig.GetBoard2D();
            for (int row = 0; row < boardConfig.Height; row++)
            {
                for (int col = 0; col < boardConfig.Width; col++)
                {
                    char cellSymbol = board2D[row, col];
                    await _cellSpawner.SpawnAsync(row, col, cellSymbol);
                }
            }
        }
    }
}