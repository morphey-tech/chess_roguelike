using MessagePipe;
using Project.Core.Core.Configs.Stage;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Stage
{
    public class StageFactory
    {
        private readonly IPublisher<StageStartedMessage> _publisher;

        public StageFactory(IPublisher<StageStartedMessage> publisher)
        {
            _publisher = publisher;
        }

        public Stage Create(StageConfig config, BoardGrid grid)
        {
            return new Stage(config, grid, _publisher);
        }
    }
}
