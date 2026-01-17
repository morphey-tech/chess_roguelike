using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Stage.Messages;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public class StageFactory
    {
        private readonly IPublisher<StageCompletedMessage> _completedPublisher;
        private readonly ILogService _logService;

        [Inject]
        private StageFactory(
            IPublisher<StageCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _completedPublisher = completedPublisher;
            _logService = logService;
        }

        public Stage Create(StageConfig config, BoardGrid grid, IEnumerable<IStagePhase> phases)
        {
            return new Stage(
                config, 
                grid, 
                phases,
                _completedPublisher,
                _logService);
        }
    }
}
