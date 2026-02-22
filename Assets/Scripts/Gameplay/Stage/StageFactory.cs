using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.ShrinkingZone;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public class StageFactory
    {
        private readonly ZoneInitService _zoneInitService;
        private readonly IPublisher<StageCompletedMessage> _completedPublisher;
        private readonly ILogService _logService;

        [Inject]
        private StageFactory(
            ZoneInitService zoneInitService,
            IPublisher<StageCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _zoneInitService = zoneInitService;
            _completedPublisher = completedPublisher;
            _logService = logService;
        }

        public Stage Create(StageConfig config, BoardGrid grid, PlayerRunStateModel runState,
            IEnumerable<IStagePhase> phases)
        {
            return new Stage(
                config, 
                grid,
                runState,
                phases,
                _zoneInitService,
                _completedPublisher,
                _logService);
        }
    }
}
