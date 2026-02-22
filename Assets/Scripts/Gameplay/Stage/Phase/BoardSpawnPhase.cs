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
        private readonly IPublisher<PhaseCompletedMessage> _phaseCompletedPublisher;
        private readonly ILogger<BoardSpawnPhase> _logger;

        public BoardSpawnPhase(BoardSpawnService boardSpawnService,
            IPublisher<PhaseCompletedMessage> phaseCompletedPublisher,
            ILogService logService)
        {
            _boardSpawnService = boardSpawnService;
            _phaseCompletedPublisher = phaseCompletedPublisher;
            _logger = logService.CreateLogger<BoardSpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info($"Spawning board: {context.Stage.BoardId}");
            await _boardSpawnService.SpawnVisualAsync(context.Grid, context.Stage.BoardId);
            _phaseCompletedPublisher.Publish(new PhaseCompletedMessage(PhaseIds.BoardSpawn));
            return PhaseResult.Continue;
        }
    }
}