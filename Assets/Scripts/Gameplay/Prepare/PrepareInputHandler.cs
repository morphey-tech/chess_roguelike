using System;
using MessagePipe;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare.Messages;
using VContainer;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Dedicated input bridge for prepare phase.
    /// Subscribes to input messages and delegates to PrepareService.
    /// </summary>
    public sealed class PrepareInputHandler : IDisposable
    {
        private readonly PrepareService _prepareService;
        private readonly IDisposable _subscriptions;

        [Inject]
        private PrepareInputHandler(
            PrepareService prepareService,
            ISubscriber<HandFigureClickedMessage> handFigureClickedSubscriber,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            ISubscriber<PrepareCompleteRequestedMessage> prepareCompleteRequestedSubscriber)
        {
            _prepareService = prepareService;
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            handFigureClickedSubscriber.Subscribe(_prepareService.HandleHandFigureClicked).AddTo(bag);
            cellClickedSubscriber.Subscribe(_prepareService.HandleCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => _prepareService.HandleCancelRequested()).AddTo(bag);
            prepareCompleteRequestedSubscriber.Subscribe(_ => _prepareService.RequestCompletePrepare()).AddTo(bag);
            _subscriptions = bag.Build();
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
