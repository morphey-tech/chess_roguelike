using Project.Core.Core.Combat;
using DG.Tweening;
using System;
using MessagePipe;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Turn;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.UI
{
  public class TurnWindow : ParameterlessWindow
  {
    [SerializeField] private RectTransform _playerStep;
    [SerializeField] private RectTransform _botStep;

    private ISubscriber<TurnChangedMessage> _turnChangedPublisher;
    
    private IDisposable? _disposable;
    private Team? _currentTurnTeam = null;

    [Inject]
    private void Construct(ISubscriber<TurnChangedMessage> turnChangedPublisher)
    {
      _turnChangedPublisher = turnChangedPublisher;
    }

    protected override void OnShowed()
    {
      _disposable ??= _turnChangedPublisher.Subscribe(OnTurnChanged);
    }

    protected override void OnHidden()
    {
      _disposable?.Dispose();
      _disposable = null;
    }

    public void ForceHideSteps()
    {
      _botStep.anchoredPosition = Vector2.up * 100;
      _playerStep.anchoredPosition = -Vector2.up * 100;
    }

    private void OnTurnChanged(TurnChangedMessage msg)
    {
      _currentTurnTeam = msg.CurrentTeam;
      RectTransform activeTurn = _currentTurnTeam == Team.Player ? _playerStep : _botStep;
      RectTransform inactiveTurn = activeTurn == _playerStep ? _botStep : _playerStep;
      activeTurn.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutCubic);
      inactiveTurn.DOAnchorPos(_currentTurnTeam == Team.Player ? Vector2.up * 100f : -Vector2.up * 100f, 0.25f)
        .SetEase(Ease.OutCubic);
    }
  }
}