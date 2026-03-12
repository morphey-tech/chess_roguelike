using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Stage.Messages;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Coordinates the placement phase.
    /// Creates placement rules, starts PrepareService, waits for completion.
    /// </summary>
    public sealed class PreparePlacementPhase : IStagePhase, IDisposable
    {
        private readonly PrepareService _prepareService;
        private readonly ISubscriber<string, PrepareMessage> _prepareSubscriber;
        private readonly IPublisher<PhaseCompletedMessage> _phaseCompletedPublisher;
        private readonly ILogger<PreparePlacementPhase> _logger;

        private StageContext _context;
        private IDisposable? _subscription;

        public PreparePlacementPhase(
            PrepareService prepareService,
            ISubscriber<string, PrepareMessage> prepareSubscriber,
            IPublisher<PhaseCompletedMessage> phaseCompletedPublisher,
            ILogService logService)
        {
            _prepareService = prepareService;
            _prepareSubscriber = prepareSubscriber;
            _phaseCompletedPublisher = phaseCompletedPublisher;
            _logger = logService.CreateLogger<PreparePlacementPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _context = context;
            _subscription = _prepareSubscriber.Subscribe(PrepareMessage.PHASE_COMPLETED, OnPreparePhaseCompleted);

            _logger.Info("PreparePlacementPhase starting");
            IPreparePlacementRules rules = new FrontRowsPlacementRules(context.Grid, allowedRows: 2);
            await _prepareService.Start(context.RunState, context.Grid, rules);
            return PhaseResult.WaitForCompletion;
        }

        private void OnPreparePhaseCompleted(PrepareMessage message)
        {
            _subscription?.Dispose();
            _subscription = null;

            _context?.CompletePhase?.Invoke(PhaseResult.Continue);
            _phaseCompletedPublisher.Publish(new PhaseCompletedMessage(PhaseIds.PreparePlacement));
            _logger.Info($"Prepare completed: {message.PlacedFiguresCount} figures placed");
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
