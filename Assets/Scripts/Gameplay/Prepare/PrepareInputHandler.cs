using System;
using MessagePipe;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare.Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Dedicated input bridge for prepare phase.
    /// Subscribes to input messages and delegates to PrepareService.
    /// </summary>
    public sealed class PrepareInputHandler : IStartable, IDisposable
    {
        private readonly PrepareService _prepareService;
        private readonly ISubscriber<HandFigureClickedMessage> _figureClickedSubscriber;
        private readonly ISubscriber<CellClickedMessage> _cellClickedSubscriber;
        private readonly ISubscriber<CancelRequestedMessage> _cancelSubscriber;
        private readonly ISubscriber<PrepareCompleteRequestedMessage> _prepareCompletedSubscriber;

        private IDisposable _subscriptions = null!;

        [Inject]
        private PrepareInputHandler(
            PrepareService prepareService,
            ISubscriber<HandFigureClickedMessage> handFigureClickedSubscriber,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            ISubscriber<PrepareCompleteRequestedMessage> prepareCompleteRequestedSubscriber)
        {
            _prepareService = prepareService;
            _figureClickedSubscriber = handFigureClickedSubscriber;
            _cellClickedSubscriber = cellClickedSubscriber;
            _cancelSubscriber = cancelSubscriber;
            _prepareCompletedSubscriber = prepareCompleteRequestedSubscriber;
        }

        void IStartable.Start()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _figureClickedSubscriber.Subscribe(_prepareService.HandleHandFigureClicked).AddTo(bag);
            _cellClickedSubscriber.Subscribe(_prepareService.HandleCellClicked).AddTo(bag);
            _cancelSubscriber.Subscribe(_ => _prepareService.HandleCancelRequested()).AddTo(bag);
            _prepareCompletedSubscriber.Subscribe(_ => _prepareService.RequestCompletePrepare()).AddTo(bag);
            _subscriptions = bag.Build();
            
            UnityEngine.Debug.Log("[PrepareInputHandler] Started and subscribed to messages");
        }
        
        void IDisposable.Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
