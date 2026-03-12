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
        private readonly ISubscriber<string, PrepareMessage> _prepareSubscriber;
        private readonly ILogger<PrepareVisualSyncService> _logger;

        private IDisposable _subscriptions = null!;

        [Inject]
        private PrepareVisualSyncService(
            IPreparePresenter presenter,
            ISubscriber<string, PrepareMessage> prepareSubscriber,
            ILogService logService)
        {
            _presenter = presenter;
            _prepareSubscriber = prepareSubscriber;
            _logger = logService.CreateLogger<PrepareVisualSyncService>();

        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _prepareSubscriber.Subscribe(PrepareMessage.SELECTION_CHANGED, OnPrepareSelectionChanged).AddTo(bag);
            _prepareSubscriber.Subscribe(PrepareMessage.VISUAL_RESET, OnPrepareVisualReset).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnPrepareSelectionChanged(PrepareMessage message)
        {
            if (string.IsNullOrEmpty(message.FigureId))
            {
                return;
            }
            _presenter.SetSelected(message.FigureId, message.IsSelected);
        }

        private void OnPrepareVisualReset(PrepareMessage message)
        {
            OnVisualReset();
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
