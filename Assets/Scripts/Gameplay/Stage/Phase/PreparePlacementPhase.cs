using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare.Messages;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Phase where player places figures from hand onto the board.
    /// Returns WaitForCompletion and completes when PreparePhaseCompletedMessage is received.
    /// </summary>
    public sealed class PreparePlacementPhase : IStagePhase, IDisposable
    {
        private readonly ISubscriber<PreparePhaseCompletedMessage> _completedSubscriber;
        private readonly ILogger<PreparePlacementPhase> _logger;
        
        private StageContext _context;
        private IDisposable _subscription;

        public PreparePlacementPhase(
            ISubscriber<PreparePhaseCompletedMessage> completedSubscriber,
            ILogService logService)
        {
            _completedSubscriber = completedSubscriber;
            _logger = logService.CreateLogger<PreparePlacementPhase>();
        }

        public UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _context = context;
            
            // Subscribe to completion message from PrepareService
            _subscription = _completedSubscriber.Subscribe(OnPrepareCompleted);
            
            _logger.Info("Waiting for player to place figures...");
            
            // PrepareService should be started externally or we could start it here
            // Return WaitForCompletion - Stage will wait until we call context.CompletePhase
            return UniTask.FromResult(PhaseResult.WaitForCompletion);
        }

        private void OnPrepareCompleted(PreparePhaseCompletedMessage message)
        {
            _logger.Info($"Player placed {message.PlacedFiguresCount} figures");
            _subscription?.Dispose();
            _subscription = null;
            
            _context?.CompletePhase?.Invoke(PhaseResult.Continue);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}