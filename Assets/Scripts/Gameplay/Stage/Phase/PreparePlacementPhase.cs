using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Stage.Messages;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Coordinates the placement phase.
    /// Creates placement rules, starts PrepareService, waits for completion.
    /// </summary>
    public sealed class PreparePlacementPhase : IStagePhase, IDisposable
    {
        private readonly PrepareService _prepareService;
        private readonly ISubscriber<PrepareMessage> _prepareSubscriber;
        private readonly IPublisher<string, StagePhaseMessage> _stagePhasePublisher;
        private readonly ILogger<PreparePlacementPhase> _logger;

        private StageContext _context;
        private IDisposable? _subscription;

        [Inject]
        private PreparePlacementPhase(
            PrepareService prepareService,
            ISubscriber<PrepareMessage> prepareSubscriber,
            IPublisher<string, StagePhaseMessage> stagePhasePublisher,
            ILogService logService)
        {
            _prepareService = prepareService;
            _prepareSubscriber = prepareSubscriber;
            _stagePhasePublisher = stagePhasePublisher;
            _logger = logService.CreateLogger<PreparePlacementPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _context = context;
            _subscription = _prepareSubscriber.Subscribe(OnPrepareMessage);
            IPreparePlacementRules rules = new FrontRowsPlacementRules(context.Grid, allowedRows: 2);
            await _prepareService.Start(context.RunState, context.Grid, rules);
            _logger.Info("PreparePlacementPhase starting");
            return PhaseResult.WaitForCompletion;
        }

        private void OnPrepareMessage(PrepareMessage message)
        {
            if (message.Type != PrepareMessage.PHASE_COMPLETED)
            {
                return;
            }

            _subscription?.Dispose();
            _subscription = null;

            _context?.CompletePhase?.Invoke(PhaseResult.Continue);
            _stagePhasePublisher.Publish(StagePhaseMessage.PHASE_COMPLETED, 
                StagePhaseMessage.PhaseCompleted(PhaseIds.PreparePlacement));
            
            _logger.Info($"Prepare completed: {message.PlacedFiguresCount} figures placed");
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
