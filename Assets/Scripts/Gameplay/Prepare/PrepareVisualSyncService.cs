using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare.Messages;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Applies prepare visual events to presenter.
    /// Keeps presenter calls out of PrepareService orchestration logic.
    /// </summary>
    public sealed class PrepareVisualSyncService : IInitializable, IDisposable
    {
        private readonly IPreparePresenter _presenter;
        private readonly ISubscriber<PrepareSelectionChangedMessage> _changedSelectionSubscriber;
        private readonly ISubscriber<PrepareVisualResetMessage> _resetSubscriber;
        private readonly ILogger<PrepareVisualSyncService> _logger;
        
        private IDisposable _subscriptions = null!;

        [Inject]
        private PrepareVisualSyncService(
            IPreparePresenter presenter,
            ISubscriber<PrepareSelectionChangedMessage> selectionChangedSubscriber,
            ISubscriber<PrepareVisualResetMessage> visualResetSubscriber,
            ILogService logService)
        {
            _presenter = presenter;
            _changedSelectionSubscriber = selectionChangedSubscriber;
            _resetSubscriber = visualResetSubscriber;
            _logger = logService.CreateLogger<PrepareVisualSyncService>();

        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _changedSelectionSubscriber.Subscribe(OnSelectionChanged).AddTo(bag);
            _resetSubscriber.Subscribe(_ => OnVisualReset()).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnSelectionChanged(PrepareSelectionChangedMessage message)
        {
            if (string.IsNullOrEmpty(message.FigureId))
            {
                return;
            }
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
