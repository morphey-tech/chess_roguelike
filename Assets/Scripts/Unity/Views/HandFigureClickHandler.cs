using System;
using MessagePipe;
using Project.Core.Core.Physics;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare;
using Project.Unity.Unity.Views.Components;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity-side handler that detects clicks on hand figures.
    /// Listens to RawClickMessage, does raycast, publishes HandFigureClickedMessage.
    /// </summary>
    public sealed class HandFigureClickHandler : IStartable, IDisposable
    {
        private readonly ISubscriber<RawClickMessage> _rawClickSubscriber;
        private readonly IPublisher<HandFigureClickedMessage> _handFigureClickedPublisher;

        private IDisposable _subscription;

        [Inject]
        private HandFigureClickHandler(
            ISubscriber<RawClickMessage> rawClickSubscriber,
            IPublisher<HandFigureClickedMessage> handFigureClickedPublisher)
        {
            _rawClickSubscriber = rawClickSubscriber;
            _handFigureClickedPublisher = handFigureClickedPublisher;
        }

        void IStartable.Start()
        {
            _subscription = _rawClickSubscriber.Subscribe(OnRawClick);
            UnityEngine.Debug.Log("[HandFigureClickHandler] Started");
        }

        private void OnRawClick(RawClickMessage message)
        {
            if (!Physics.Raycast(message.Ray, out RaycastHit hit, PhysicsSettings.DefaultRaycastDistance,
                    PhysicsSettings.FigureLayerMask))
            {
                return;
            }

            HandFigureMarker marker = hit.collider.GetComponentInParent<HandFigureMarker>();
            if (marker == null || string.IsNullOrEmpty(marker.FigureId))
            {
                return;
            }

            _handFigureClickedPublisher.Publish(new HandFigureClickedMessage(marker.FigureId));
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }

    }
}
