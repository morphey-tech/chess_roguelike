using System;
using DG.Tweening;
using MessagePipe;
using Project.Core.Core.Combat;
using Project.Gameplay.Gameplay.Turn;
using TMPro;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.UI.Combat
{
    /// <summary>
    /// View, отвечающая за отображение текущего хода в бою.
    /// Показывает текст "Ход игрока" / "Ход противника" и анимирует появление.
    /// </summary>
    public class CombatTurnView : MonoBehaviour
    {
        private const string TURN_PREFIX_TEXT = "Ход";
        private const string PLAYER_TURN_TEXT = "Игрок";
        private const string ENEMY_TURN_TEXT = "Враг";

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _turnText = null!;

        private ISubscriber<TurnChangedMessage> _turnChangedPublisher = null!; 
        private IDisposable? _disposable;

        [Inject]
        private void Construct(ISubscriber<TurnChangedMessage> turnChangedPublisher)
        {
            _turnChangedPublisher = turnChangedPublisher;
        }

        public void Initialize()
        {
            _disposable = _turnChangedPublisher.Subscribe(OnTurnChanged);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnTurnChanged(TurnChangedMessage msg)
        {
            UpdateTurnText(msg);
            PlayAnimation();
        }

        private void UpdateTurnText(TurnChangedMessage msg)
        {
            string teamMessage = msg.CurrentTeam == Team.Player ? PLAYER_TURN_TEXT : ENEMY_TURN_TEXT;
            _turnText.text = $"{TURN_PREFIX_TEXT} {msg.TurnNumber} - {teamMessage}";
        }

        private void PlayAnimation()
        {
            _turnText.DOKill();
    
            _turnText.transform.DOScale(0.8f, 0.15f).SetEase(Ease.InBack);
            _turnText.DOFade(0f, 0.15f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                _turnText.transform.localScale = Vector3.one * 0.8f;
                _turnText.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
                _turnText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            });
        }
    }
}
