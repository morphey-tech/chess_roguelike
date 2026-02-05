using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Interaction;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Initializes gameplay services (MovementService, InteractionController) with the grid.
    /// Should run after BoardSpawnPhase.
    /// </summary>
    public sealed class GameplayInitPhase : IStagePhase
    {
        private readonly MovementService _movementService;
        private readonly InteractionController _interactionController;
        private readonly ILogger<GameplayInitPhase> _logger;

        public GameplayInitPhase(
            MovementService movementService,
            InteractionController interactionController,
            ILogService logService)
        {
            _movementService = movementService;
            _interactionController = interactionController;
            _logger = logService.CreateLogger<GameplayInitPhase>();
        }

        public UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info("Initializing gameplay services");
            
            _movementService.Configure(context.Grid);
            _interactionController.Configure(context.Grid);
            
            _logger.Info("Gameplay services initialized");
            return UniTask.FromResult(PhaseResult.Continue);
        }
    }
}
