using DG.Tweening;
using System;
using MessagePipe;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Board.Messages;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Turn;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.UI
{
  public class TurnWindow : ParameterlessWindow
  {
    [SerializeField] private CanvasGroup _preparePhase;
    [SerializeField] private RectTransform _playerStep;
    [SerializeField] private RectTransform _botStep;
    [SerializeField] private Text _capacityText;
    [SerializeField] private Button _finishPrepareButton;
    
    private ISubscriber<TurnChangedMessage> _turnChangedPublisher;
    private ISubscriber<BoardCapacityChangedMessage> _capacityChangedSubscriber;
    private IPublisher<PrepareCompleteRequestedMessage> _prepareCompletePublisher;
    private BoardCapacityService _capacityService;
    private IDisposable _turnChangedSubscription;
    private IDisposable _capacityChangedSubscription;
    private Team? _currentTurnTeam = null;
    private bool _isPreparePhase = false;
    
    [Inject]
    private void Construct(
      ISubscriber<TurnChangedMessage> turnChangedPublisher,
      ISubscriber<BoardCapacityChangedMessage> capacityChangedSubscriber,
      IPublisher<PrepareCompleteRequestedMessage> prepareCompletePublisher,
      BoardCapacityService capacityService)
    {
      _turnChangedPublisher = turnChangedPublisher;
      _capacityChangedSubscriber = capacityChangedSubscriber;
      _prepareCompletePublisher = prepareCompletePublisher;
      _capacityService = capacityService;
    }
    
    protected override void OnShowed()
    {
      _turnChangedSubscription ??= _turnChangedPublisher.Subscribe(OnTurnChanged);
      _capacityChangedSubscription ??= _capacityChangedSubscriber.Subscribe(OnCapacityChanged);
      UpdateCapacity(_capacityService.Used, _capacityService.Capacity);
    }

    protected override void OnHidden()
    {
      _turnChangedSubscription?.Dispose();
      _turnChangedSubscription = null;
      _capacityChangedSubscription?.Dispose();
      _capacityChangedSubscription = null;
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
      EnsureFinishPrepareButton();
    }

    public void SetGamePhase()
    {
      _isPreparePhase = false;
      _preparePhase.DOFade(0, 0.35f).SetEase(Ease.OutCubic);
      if (_finishPrepareButton != null)
        _finishPrepareButton.gameObject.SetActive(false);
    }

    private void EnsureFinishPrepareButton()
    {
      if (_finishPrepareButton != null)
      {
        _finishPrepareButton.gameObject.SetActive(true);
        _finishPrepareButton.onClick.RemoveAllListeners();
        _finishPrepareButton.onClick.AddListener(OnFinishPrepareClicked);
      }
    }

    private void OnFinishPrepareClicked()
    {
      _prepareCompletePublisher.Publish(new PrepareCompleteRequestedMessage());
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