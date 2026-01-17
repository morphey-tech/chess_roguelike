using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Selection;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Initializes gameplay services (MovementService, SelectionService) with the grid.
    /// Should run after BoardSpawnPhase.
    /// </summary>
    public sealed class GameplayInitPhase : IStagePhase
    {
        private readonly MovementService _movementService;
        private readonly SelectionService _selectionService;
        private readonly ILogger<GameplayInitPhase> _logger;

        public GameplayInitPhase(
            MovementService movementService,
            SelectionService selectionService,
            ILogService logService)
        {
            _movementService = movementService;
            _selectionService = selectionService;
            _logger = logService.CreateLogger<GameplayInitPhase>();
        }

        public UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info("Initializing gameplay services");
            
            _movementService.Configure(context.Grid);
            _selectionService.Configure(context.Grid);
            
            _logger.Info("Gameplay services initialized");
            return UniTask.FromResult(PhaseResult.Continue);
        }
    }
}
