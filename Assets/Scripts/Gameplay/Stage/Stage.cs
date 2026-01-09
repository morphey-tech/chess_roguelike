using MessagePipe;
using Project.Core.Core.Configs.Stage;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Stage
{
    public class Stage
    {
        private readonly StageConfig _config;
        private readonly IPublisher<StageStartedMessage> _stageStartedPublisher;

        public string Id => _config.Id;
        public string BoardId => _config.BoardId;
        public BoardGrid Grid { get; }

        public Stage(
            StageConfig config,
            BoardGrid grid,
            IPublisher<StageStartedMessage> stageStartedPublisher)
        {
            _config = config;
            Grid = grid;
            _stageStartedPublisher = stageStartedPublisher;
        }

        public void Begin()
        {
            // Передаём BoardId — BoardSpawnService заспавнит доску
            _stageStartedPublisher.Publish(
                new StageStartedMessage(Id, BoardId));
        }
    }
}
