using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Prepare.Messages;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Coordinates the placement phase.
    /// Creates placement rules, starts PrepareService, waits for completion.
    /// </summary>
    public sealed class PreparePlacementPhase : IStagePhase, IDisposable
    {
        private readonly PrepareService _prepareService;
        private readonly ISubscriber<PreparePhaseCompletedMessage> _completedSubscriber;
        private readonly ILogger<PreparePlacementPhase> _logger;

        private StageContext _context;
        private IDisposable? _subscription;

        public PreparePlacementPhase(
            PrepareService prepareService,
            ISubscriber<PreparePhaseCompletedMessage> completedSubscriber,
            ILogService logService)
        {
            _prepareService = prepareService;
            _completedSubscriber = completedSubscriber;
            _logger = logService.CreateLogger<PreparePlacementPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _context = context;
            _subscription = _completedSubscriber.Subscribe(OnPrepareCompleted);

            _logger.Info("PreparePlacementPhase starting");
            IPreparePlacementRules rules = new FrontRowsPlacementRules(context.Grid, allowedRows: 2);
            await _prepareService.Start(context.RunState, context.Grid, rules);
            return PhaseResult.WaitForCompletion;
        }

        private void OnPrepareCompleted(PreparePhaseCompletedMessage message)
        {
            _logger.Info($"Prepare completed: {message.PlacedFiguresCount} figures placed");

            _subscription?.Dispose();
            _subscription = null;

            _context?.CompletePhase?.Invoke(PhaseResult.Continue);
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
