using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs;
using Project.Core.Logging;
using Project.Gameplay.Configs;
using Project.Gameplay.Gameplay.Board.Appear;
using Project.Gameplay.Gameplay.Stage;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board
{
    public sealed class BoardSpawnService : IDisposable
    {
        private readonly ConfigProvider _configProvider;
        private readonly CellsSpawnService _cellSpawner;
        private readonly BoardAppearAnimationFactory _animationFactory;
        private readonly ILogger<BoardSpawnService> _logger;
        private readonly IDisposable _disposable;
        
        public BoardSpawnService(
            ConfigProvider configProvider,
            CellsSpawnService cellSpawner,
            BoardAppearAnimationFactory animationFactory,
            ISubscriber<StageStartedMessage> stageSubscriber,
            ILogService logService)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _cellSpawner = cellSpawner ?? throw new ArgumentNullException(nameof(cellSpawner));
            _animationFactory = animationFactory ?? throw new ArgumentNullException(nameof(animationFactory));
            _logger = logService.CreateLogger<BoardSpawnService>();
            
            _disposable = stageSubscriber.Subscribe(Spawn);
            _logger.Info("BoardSpawnService created and subscribed");
        }

        private void Spawn(StageStartedMessage message)
        {
            _logger.Info($"Received StageStartedMessage: stageId={message.StageId}, boardId={message.BoardId}");
            SpawnAsync(message.BoardId).Forget();
        }
        
        private async UniTask SpawnAsync(string boardId)
        {
            _logger.Info($"SpawnAsync started for boardId={boardId}");
            
            BoardConfigRepository repository =
                await _configProvider.Get<BoardConfigRepository>("boards_conf");

            BoardConfig? boardConfig =
                repository.GetBy(boardId);

            if (boardConfig == null)
            {
                _logger.Error($"Board config with key {boardId} not found!");
                throw new Exception($"Board config with key {boardId} not found.");
            }

            _logger.Info($"Board config loaded: {boardConfig.Width}x{boardConfig.Height}");

            char[,] board2D = boardConfig.GetBoard2D();
            List<UniTask<GameObject>> spawnTasks = new List<UniTask<GameObject>>();
            
            for (int row = 0; row < boardConfig.Height; row++)
            {
                for (int col = 0; col < boardConfig.Width; col++)
                {
                    char cellSymbol = board2D[row, col];
                    spawnTasks.Add(_cellSpawner.SpawnAsync(row, col, cellSymbol));
                }
            }

            GameObject[] spawnedCells = await UniTask.WhenAll(spawnTasks);
            _logger.Info($"Spawned {spawnedCells.Length} cells, running appear animation");

            IBoardAppearAnimationStrategy animation =
                _animationFactory.Get(boardConfig.AppearStrategyId);
            await animation.Appear(spawnedCells);
            
            _logger.Info("Board spawn completed");
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}
