using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Stage.Messages;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Represents a single stage/level. Controls the flow through phases.
    /// Stage owns the phase execution - phases don't call back into Stage.
    /// </summary>
    public class Stage : IDisposable
    {
        public string Id => _config.Id;
        public string BoardId => _config.BoardId;
        public BoardGrid? Grid { get; }
        public bool IsCompleted { get; private set; }

        private readonly StageConfig _config;
        private readonly PlayerRunStateModel _runState;
        private readonly List<IStagePhase> _phases;
        private readonly IPublisher<StageCompletedMessage> _completedPublisher;
        private readonly ILogger<Stage> _logger;

        private StageContext _context = null!;
        private int _currentPhaseIndex = -1;
        private UniTaskCompletionSource<PhaseResult>? _waitingPhaseCompletion;
        private bool _cancelRequested;

        public Stage(
            StageConfig config,
            BoardGrid grid,
            PlayerRunStateModel runState,
            IEnumerable<IStagePhase> phases,
            IPublisher<StageCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _config = config;
            Grid = grid;
            _runState = runState;
            _phases = new List<IStagePhase>(phases);
            _completedPublisher = completedPublisher;
            _logger = logService.CreateLogger<Stage>();
        }

        public async UniTask BeginAsync()
        {
            _logger.Info($"Stage {Id} beginning, board: {BoardId}, phases: {_phases.Count}");
            _context = new StageContext(this, _config, _runState);
            _context.CompletePhase = OnPhaseCompleted;
            await RunPhasesAsync();
        }

        private async UniTask RunPhasesAsync()
        {
            while (++_currentPhaseIndex < _phases.Count)
            {
                if (_cancelRequested)
                {
                    return;
                }

                IStagePhase phase = _phases[_currentPhaseIndex];
                _logger.Info($"[Phase {_currentPhaseIndex + 1}/{_phases.Count}] {phase.GetType().Name} starting");

                PhaseResult result = await phase.ExecuteAsync(_context);
                if (result == PhaseResult.WaitForCompletion)
                {
                    _waitingPhaseCompletion = new UniTaskCompletionSource<PhaseResult>();
                    result = await _waitingPhaseCompletion.Task;
                    _waitingPhaseCompletion = null;
                }

                if (_cancelRequested)
                {
                    return;
                }

                _logger.Info($"[Phase] {phase.GetType().Name} completed with {result}");
                if (result is PhaseResult.Victory or PhaseResult.Defeat)
                {
                    Complete(result == PhaseResult.Victory ? StageResult.Victory : StageResult.Defeat);
                    return;
                }
            }

            Complete(StageResult.Victory);
        }

        private void OnPhaseCompleted(PhaseResult result)
        {
            _waitingPhaseCompletion?.TrySetResult(result);
        }

        public void Abort()
        {
            if (_cancelRequested || IsCompleted)
            {
                return;
            }

            _cancelRequested = true;
            _waitingPhaseCompletion?.TrySetResult(PhaseResult.Defeat);
            _logger.Info($"Stage {Id} abort requested");
        }

        private void Complete(StageResult result)
        {
            if (IsCompleted)
            {
                return;
            }
            IsCompleted = true;
            _completedPublisher.Publish(new StageCompletedMessage(Id, result));
            _logger.Info($"Stage {Id} completed: {result}");
        }

        public void Dispose()
        {
            foreach (IStagePhase phase in _phases)
            {
                if (phase is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
