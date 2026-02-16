using System;
using MessagePipe;
using Project.Core.Core.Physics;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare;
using Project.Unity.Unity.Views.Components;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity-side handler that detects clicks on hand figures.
    /// Listens to RawClickMessage, does raycast, publishes HandFigureClickedMessage.
    /// </summary>
    public sealed class HandFigureClickHandler : IDisposable
    {
        private readonly IPublisher<HandFigureClickedMessage> _handFigureClickedPublisher;
        private readonly IDisposable _subscription;

        [Inject]
        public HandFigureClickHandler(
            ISubscriber<RawClickMessage> rawClickSubscriber,
            IPublisher<HandFigureClickedMessage> handFigureClickedPublisher)
        {
            _handFigureClickedPublisher = handFigureClickedPublisher;
            _subscription = rawClickSubscriber.Subscribe(OnRawClick);
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
