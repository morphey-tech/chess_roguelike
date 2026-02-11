using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Shutdown;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Resets runtime systems and visual world before loading next stage.
    /// </summary>
    public sealed class StageRuntimeResetService
    {
        private readonly InteractionController _interactionController;
        private readonly InteractionLockService _interactionLock;
        private readonly PrepareService _prepareService;
        private readonly GameShutdownCleanupService _cleanupService;

        public StageRuntimeResetService(
            InteractionController interactionController,
            InteractionLockService interactionLock,
            PrepareService prepareService,
            GameShutdownCleanupService cleanupService)
        {
            _interactionController = interactionController;
            _interactionLock = interactionLock;
            _prepareService = prepareService;
            _cleanupService = cleanupService;
        }

        public void ResetRuntime()
        {
            _interactionController.Deactivate();
            _interactionLock.Reset();
            _prepareService.Reset();
            _cleanupService.Cleanup();
        }
    }
}
