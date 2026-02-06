using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class BoardSpawnPhase : IStagePhase
    {
        private readonly BoardSpawnService _boardSpawnService;
        private readonly ILogger<BoardSpawnPhase> _logger;

        public BoardSpawnPhase(BoardSpawnService boardSpawnService, ILogService logService)
        {
            _boardSpawnService = boardSpawnService;
            _logger = logService.CreateLogger<BoardSpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info($"Spawning board: {context.Stage.BoardId}");
            await _boardSpawnService.SpawnVisualAsync(context.Grid, context.Stage.BoardId);
            _logger.Info("Board spawned");
            return PhaseResult.Continue;
        }
    }
}