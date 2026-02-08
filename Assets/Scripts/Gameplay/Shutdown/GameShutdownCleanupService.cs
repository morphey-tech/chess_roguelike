using System.Collections.Generic;
using Project.Core.Core.Logging;
using VContainer;

namespace Project.Gameplay.Gameplay.Shutdown
{
    public sealed class GameShutdownCleanupService
    {
        private readonly IEnumerable<IGameShutdownCleanup> _cleanups;
        private readonly ILogger<GameShutdownCleanupService> _logger;

        [Inject]
        public GameShutdownCleanupService(
            IEnumerable<IGameShutdownCleanup> cleanups,
            ILogService logService)
        {
            _cleanups = cleanups;
            _logger = logService.CreateLogger<GameShutdownCleanupService>();
        }

        public void Cleanup()
        {
            foreach (IGameShutdownCleanup cleanup in _cleanups)
            {
                cleanup.Cleanup();
            }

            _logger.Info("Game shutdown cleanup complete");
        }
    }
}
