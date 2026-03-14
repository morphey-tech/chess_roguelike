using System;
using MessagePipe;
using Project.Core.Core.Combat;
using Project.Gameplay.Gameplay.Turn;
using TMPro;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.UI.Combat
{
    /// <summary>
    /// View, отвечающая за отображение дополнительной информации о бое:
    /// - Номер текущего хода
    /// Обновляется реактивно через подписку на TurnChangedMessage.
    /// </summary>
    public class CombatExtraView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _turnNumberText;

        private ISubscriber<TurnChangedMessage> _turnChangedPublisher;
        private int _currentTurn = 1;

        private IDisposable? _disposable;
        
        [Inject]
        private void Construct(ISubscriber<TurnChangedMessage> turnChangedPublisher)
        {
            _turnChangedPublisher = turnChangedPublisher;
        }

        /*private void OnEnable()
        {
            _disposable = _turnChangedPublisher.Subscribe(OnTurnChanged);
            UpdateTexts();
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void OnTurnChanged(TurnChangedMessage msg)
        {
            _currentTurn = msg.TurnNumber;
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            _turnNumberText.text = $"Ход: {_currentTurn}";
        }

        public void SetTurnNumber(int turnNumber)
        {
            _currentTurn = turnNumber;
            UpdateTexts();
        }*/
    }
}
