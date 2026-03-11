using System;
using MessagePipe;
using Project.Core.Window;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Board.Messages;
using TMPro;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.UI
{
    public sealed class BoardCapacityWindow : ParameterlessWindow
    {
        [SerializeField]
        private TextMeshProUGUI _capacityText;

        private ISubscriber<BoardCapacityChangedMessage> _capacityChangedSubscriber;
        private BoardCapacityService _capacityService;
        private IDisposable? _capacitySubscription;

        [Inject]
        private void Construct(
            ISubscriber<BoardCapacityChangedMessage> capacityChangedSubscriber,
            BoardCapacityService capacityService)
        {
            _capacityChangedSubscriber = capacityChangedSubscriber;
            _capacityService = capacityService;
        }

        protected override void OnShowed()
        {
            _capacitySubscription?.Dispose();
            _capacitySubscription = _capacityChangedSubscriber.Subscribe(OnCapacityChanged);
            UpdateCapacity(_capacityService.Used, _capacityService.Capacity);
        }

        protected override void OnHidden()
        {
            _capacitySubscription?.Dispose();
            _capacitySubscription = null;
        }

        private void OnCapacityChanged(BoardCapacityChangedMessage msg)
        {
            UpdateCapacity(msg.Used, msg.Capacity);
        }

        private void UpdateCapacity(int used, int capacity)
        {
            if (_capacityText == null)
                return;

            _capacityText.text = $"{used}/{capacity}";
        }
    }
}

