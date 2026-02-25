using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    /// <summary>
    /// Interactive session for bonus move phase.
    /// 
    /// Responsibilities:
    /// - Listen for clicks (only when session is active)
    /// - Validate via IBonusMoveController (domain)
    /// - Play animation via VisualPipeline
    /// - Publish highlight messages for UI
    /// 
    /// This is NOT a service - it's a session object that runs once per bonus move.
    /// </summary>
    public sealed class BonusMoveSession : IBonusMoveSession, IDisposable
    {
        private readonly IBonusMoveController _domainController;
        private readonly VisualPipeline _visualPipeline;
        private readonly ShrinkingZone.StormBattleService _stormBattle;
        private readonly IPublisher<BonusMoveStartedMessage> _startedPublisher;
        private readonly IPublisher<BonusMoveCompletedMessage> _completedPublisher;
        private readonly ILogger<BonusMoveSession> _logger;
        private readonly IDisposable _subscription;

        // Session state
        private bool _isActive;
        private Figure _actor;
        private BoardGrid _grid;
        private UniTaskCompletionSource<GridPosition> _clickTcs;

        [Inject]
        public BonusMoveSession(
            IBonusMoveController domainController,
            VisualPipeline visualPipeline,
            ShrinkingZone.StormBattleService stormBattle,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            IPublisher<BonusMoveStartedMessage> startedPublisher,
            IPublisher<BonusMoveCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _domainController = domainController;
            _visualPipeline = visualPipeline;
            _stormBattle = stormBattle;
            _startedPublisher = startedPublisher;
            _completedPublisher = completedPublisher;
            _logger = logService.CreateLogger<BonusMoveSession>();

            _subscription = cellClickedSubscriber.Subscribe(OnCellClicked);

            _logger.Info("BonusMoveSession created");
        }

        public async UniTask RunAsync(Figure actor, GridPosition from, int maxDistance, BoardGrid grid)
        {
            if (_isActive)
            {
                _logger.Warning("BonusMoveSession already active!");
                return;
            }

            _logger.Info($"Starting bonus move session for {actor.Id} from ({from.Row},{from.Column}), max: {maxDistance}");

            _isActive = true;
            _actor = actor;
            _grid = grid;

            try
            {
                // Start domain state
                _domainController.Start(actor, from, maxDistance, grid);
                
                // Notify UI to show highlights
                _startedPublisher.Publish(new BonusMoveStartedMessage(actor));

                // Wait for valid click
                await WaitForValidClickAsync();

                // Notify UI to clear highlights
                _completedPublisher.Publish(new BonusMoveCompletedMessage(actor));
                
                _logger.Info($"Bonus move session completed for {actor.Id}");
            }
            finally
            {
                _isActive = false;
                _actor = null;
                _grid = null;
                _clickTcs = null;
            }
        }

        private async UniTask WaitForValidClickAsync()
        {
            while (_domainController.IsActive)
            {
                // Wait for next click
                _clickTcs = new UniTaskCompletionSource<GridPosition>();
                
                GridPosition clickedPosition;
                try
                {
                    clickedPosition = await _clickTcs.Task;
                }
                catch (OperationCanceledException)
                {
                    _logger.Debug("Bonus move session cancelled");
                    _domainController.Cancel();
                    return;
                }

                // Try domain validation + execution
                if (_domainController.TryExecute(clickedPosition))
                {
                    // Domain succeeded - play visual
                    await PlayMoveVisualAsync(clickedPosition);
                    _logger.Info($"Bonus move executed to ({clickedPosition.Row},{clickedPosition.Column})");
                    return;
                }

                _logger.Debug($"Bonus move rejected: ({clickedPosition.Row},{clickedPosition.Column})");
            }
        }

        private async UniTask PlayMoveVisualAsync(GridPosition to)
        {
            using (VisualScope scope = _visualPipeline.BeginScope())
            {
                scope.Enqueue(new MoveCommand(new MoveVisualContext(_actor.Id, to)));
                await scope.PlayAsync();
            }
            
            // Проверить урон от зоны после бонусного движения
            CheckZoneDamage(_actor, to);
        }

        private void CheckZoneDamage(Figure figure, GridPosition position)
        {
            var status = _stormBattle.GetCellStatus(position.Row, position.Column);
            if (status == StormCellStatus.Danger)
            {
                _logger.Debug($"Bonus move: {figure.Id} entered danger zone at ({position.Row},{position.Column})");
                _stormBattle.ApplyZoneDamage(figure, position);
            }
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            // Only process clicks when session is active
            if (!_isActive)
                return;

            if (_grid == null || !_grid.IsInside(message.Position))
                return;

            _logger.Debug($"Click received: ({message.Position.Row},{message.Position.Column})");

            // Complete the waiting task
            if (_clickTcs != null)
            {
                _clickTcs.TrySetResult(message.Position);
            }
        }

        public void Cancel()
        {
            if (!_isActive)
                return;

            _logger.Debug("Cancel requested");
            
            if (_clickTcs != null)
            {
                _clickTcs.TrySetCanceled();
            }
        }

        public void Dispose()
        {
            Cancel();
            _subscription?.Dispose();
        }
    }
}
