using DG.Tweening;
using MessagePipe;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.UI
{
  public class TurnWindow : ParameterlessWindow
  {
    [SerializeField] private CanvasGroup _preparePhase;
    [SerializeField] private RectTransform _playerStep;
    [SerializeField] private RectTransform _botStep;
    
    private ISubscriber<TurnChangedMessage> _turnChangedPublisher;
    private Team? _currentTurnTeam = null;
    private bool _isPreparePhase = false;
    
    [Inject]
    private void Construct(ISubscriber<TurnChangedMessage> turnChangedPublisher)
    {
      _turnChangedPublisher = turnChangedPublisher;
    }
    
    protected override void OnShowed()
    {
      _turnChangedPublisher.Subscribe(OnTurnChanged);
    }

    protected override void OnHidden()
    {
      // а как отписаться...
    }

    public void ForceHideSteps()
    {
      _botStep.anchoredPosition = Vector2.up * 100;
      _playerStep.anchoredPosition = -Vector2.up * 100;
    }
    
    private void OnTurnChanged(TurnChangedMessage msg)
    {
      _currentTurnTeam =  msg.CurrentTeam;
      
      var activeTurn = _currentTurnTeam == Team.Player ?  _playerStep : _botStep;
      var inactiveTurn = activeTurn == _playerStep ? _botStep : _playerStep;
      activeTurn.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutCubic);
      inactiveTurn.DOAnchorPos(_currentTurnTeam == Team.Player ? Vector2.up * 100f : -Vector2.up * 100f, 0.25f).SetEase(Ease.OutCubic);
    }

    public void SetPreparePhase()
    {
      _isPreparePhase = true;
      ForceHideSteps();
    }

    public void SetGamePhase()
    {
      _isPreparePhase = false;
      _preparePhase.DOFade(0, 0.35f).SetEase(Ease.OutCubic);
    }
  }
}