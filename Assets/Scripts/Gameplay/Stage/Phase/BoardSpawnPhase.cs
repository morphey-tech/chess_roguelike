using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Stage.Messages;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class BoardSpawnPhase : IStagePhase
    {
        private readonly BoardSpawnService _boardSpawnService;
        private readonly IPublisher<string, StagePhaseMessage> _stagePhasePublisher;
        private readonly ILogger<BoardSpawnPhase> _logger;

        public BoardSpawnPhase(BoardSpawnService boardSpawnService,
            IPublisher<string, StagePhaseMessage> stagePhasePublisher,
            ILogService logService)
        {
            _boardSpawnService = boardSpawnService;
            _stagePhasePublisher = stagePhasePublisher;
            _logger = logService.CreateLogger<BoardSpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            await _boardSpawnService.SpawnVisualAsync(context.Grid, context.Stage.BoardId);
            _stagePhasePublisher.Publish(StagePhaseMessage.PHASE_COMPLETED, 
                StagePhaseMessage.PhaseCompleted(PhaseIds.BoardSpawn));
            _logger.Info($"Spawning board: {context.Stage.BoardId}");
            return PhaseResult.Continue;
        }
    }
}