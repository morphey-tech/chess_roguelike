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
using Project.Gameplay.ShrinkingZone;
using VContainer;
using VContainer.Unity;

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
    public sealed class BonusMoveSession : IInitializable, IBonusMoveSession, IDisposable
    {
        private readonly IBonusMoveController _domainController;
        private readonly VisualPipeline _visualPipeline;
        private readonly StormBattleService _stormBattle;
        private readonly ISubscriber<CellClickedMessage> _cellClickedSubscriber;
        private readonly IPublisher<string, BonusMoveMessage> _bonusMovePublisher;
        private readonly ILogger<BonusMoveSession> _logger;

        private bool _isActive;
        private Figure _actor;
        private BoardGrid _grid;
        private UniTaskCompletionSource<GridPosition> _clickTcs;
        private IDisposable _disposable;

        [Inject]
        private BonusMoveSession(
            IBonusMoveController domainController,
            VisualPipeline visualPipeline,
            StormBattleService stormBattle,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            IPublisher<string, BonusMoveMessage> bonusMovePublisher,
            ILogService logService)
        {
            _domainController = domainController;
            _visualPipeline = visualPipeline;
            _stormBattle = stormBattle;
            _cellClickedSubscriber = cellClickedSubscriber;
            _bonusMovePublisher = bonusMovePublisher;
            _logger = logService.CreateLogger<BonusMoveSession>();


        }

        void IInitializable.Initialize()
        {
            _disposable = _cellClickedSubscriber.Subscribe(OnCellClicked);
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
                _domainController.Start(actor, from, maxDistance, grid);
                _bonusMovePublisher.Publish(BonusMoveMessage.STARTED, BonusMoveMessage.Started(actor));

                await WaitForValidClickAsync();
                _bonusMovePublisher.Publish(BonusMoveMessage.COMPLETED, BonusMoveMessage.Completed(actor));
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
                GridPosition clickedPosition;
                _clickTcs = new UniTaskCompletionSource<GridPosition>();
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

                if (_domainController.TryExecute(clickedPosition))
                {
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
            CheckZoneDamage(_actor, to);
        }

        private void CheckZoneDamage(Figure figure, GridPosition position)
        {
            StormCellStatus status = _stormBattle.GetCellStatus(position.Row, position.Column);
            if (status == StormCellStatus.Danger)
            {
                _stormBattle.ApplyZoneDamage(figure, position);
                _logger.Debug($"Bonus move: {figure.Id} entered danger zone at ({position.Row},{position.Column})");
            }
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (!_isActive)
            {
                return;
            }
            if (_grid == null || !_grid.IsInside(message.Position))
            {
                return;
            }
            if (_clickTcs != null)
            {
                _clickTcs.TrySetResult(message.Position);
            }
            _logger.Debug($"Click received: ({message.Position.Row},{message.Position.Column})");
        }

        public void Cancel()
        {
            if (!_isActive)
            {
                return;
            }
            if (_clickTcs != null)
            {
                _clickTcs.TrySetCanceled();
            }
            _logger.Debug("Cancel requested");
        }

        public void Dispose()
        {
            Cancel();
            _disposable?.Dispose();
        }

    }
}
