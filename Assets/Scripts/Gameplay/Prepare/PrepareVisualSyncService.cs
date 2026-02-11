using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare.Messages;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Applies prepare visual events to presenter.
    /// Keeps presenter calls out of PrepareService orchestration logic.
    /// </summary>
    public sealed class PrepareVisualSyncService : IDisposable
    {
        private readonly IPreparePresenter _presenter;
        private readonly ILogger<PrepareVisualSyncService> _logger;
        private readonly IDisposable _subscriptions;

        public PrepareVisualSyncService(
            IPreparePresenter presenter,
            ISubscriber<PrepareSelectionChangedMessage> selectionChangedSubscriber,
            ISubscriber<PrepareVisualResetMessage> visualResetSubscriber,
            ILogService logService)
        {
            _presenter = presenter;
            _logger = logService.CreateLogger<PrepareVisualSyncService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            selectionChangedSubscriber.Subscribe(OnSelectionChanged).AddTo(bag);
            visualResetSubscriber.Subscribe(_ => OnVisualReset()).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnSelectionChanged(PrepareSelectionChangedMessage message)
        {
            if (string.IsNullOrEmpty(message.FigureId))
                return;

            _presenter.SetSelected(message.FigureId, message.IsSelected);
        }

        private void OnVisualReset()
        {
            _presenter.Clear();
            _logger.Debug("Prepare visuals reset");
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
