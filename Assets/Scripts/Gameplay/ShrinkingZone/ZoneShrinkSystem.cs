using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.ShrinkingZone.Config;
using Project.Core.Core.ShrinkingZone.Core;
using Project.Core.Core.ShrinkingZone.Messages;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.ShrinkingZone.Messages;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Основная система shrinking zone
    /// </summary>
    public class ZoneShrinkSystem : IZoneShrinkQueryService, IDisposable
    {
        public ZoneState CurrentState => _state;
        public int CurrentLayer => _currentLayer;
        public int StepInLayer => _stepInLayer;
        public int ActivationTurn => _activationTurn;
        public int? FirstDamageTurn => _firstDamageTurn;
        public bool FirstDamageDealt => _firstDamageDealt;

        private readonly ZoneShrinkConfig _config;
        private readonly IZoneShrinkStrategy _strategy;
        private readonly IPublisher<ZoneStateChangedMessage> _stateChangedPublisher;
        private readonly IPublisher<ZoneCellsUpdatedMessage> _cellsUpdatedPublisher;
        private readonly IPublisher<UnitTakeZoneDamageMessage> _damagePublisher;
        private readonly IDisposable _subscriptions;

        private ZoneState _state = ZoneState.Inactive;
        private int _currentLayer;
        private int _stepInLayer;
        private int _activationTurn;
        private int? _firstDamageTurn;
        private bool _firstDamageDealt;

        private HashSet<GridPosition> _warningCells;
        private HashSet<GridPosition> _dangerCells;

        public ZoneShrinkSystem(
            ZoneShrinkConfig config,
            IZoneShrinkStrategy strategy,
            IPublisher<ZoneStateChangedMessage> stateChangedPublisher,
            IPublisher<ZoneCellsUpdatedMessage> cellsUpdatedPublisher,
            IPublisher<UnitTakeZoneDamageMessage> damagePublisher,
            ISubscriber<ZoneBattleStartedMessage> battleStartedSubscriber,
            ISubscriber<ZoneTurnStartedMessage> turnStartedSubscriber,
            ISubscriber<ZoneDamageDealtMessage> damageDealtSubscriber,
            ISubscriber<ZoneFigureTurnEndedMessage> figureTurnEndedSubscriber)
        {
            _config = config;
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _stateChangedPublisher = stateChangedPublisher;
            _cellsUpdatedPublisher = cellsUpdatedPublisher;
            _damagePublisher = damagePublisher;

            _warningCells = new HashSet<GridPosition>();
            _dangerCells = new HashSet<GridPosition>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            battleStartedSubscriber.Subscribe(_ => OnBattleStarted()).AddTo(bag);
            turnStartedSubscriber.Subscribe(msg => OnTurnStarted(msg.Turn)).AddTo(bag);
            damageDealtSubscriber.Subscribe(msg => OnDamageDealt(msg.Turn)).AddTo(bag);
            figureTurnEndedSubscriber.Subscribe(OnFigureTurnEnded).AddTo(bag);
            _subscriptions = bag.Build();

            Debug.Log($"[ZONE SYSTEM] Created with config: min_turn={_config.MinTurn}, max_turn={_config.MaxTurn}");
        }

        private void OnBattleStarted()
        {
            _state = ZoneState.Inactive;
            _currentLayer = 0;
            _stepInLayer = 0;
            _activationTurn = CalculateActivationTurn();
            _firstDamageTurn = null;
            _firstDamageDealt = false;

            _warningCells.Clear();
            _dangerCells.Clear();

            Debug.Log($"[ZONE SYSTEM] Battle started, activation_turn={_activationTurn}");
            PublishStateChange(_state);
        }

        private void OnTurnStarted(int turn)
        {
            Debug.Log($"[ZONE SYSTEM] Turn {turn} started, state={_state}, activation_turn={_activationTurn}");

            if (_state == ZoneState.Inactive && turn >= _activationTurn)
            {
                _state = ZoneState.Active;
                _firstDamageTurn = turn;
                UpdateZoneCells();
                Debug.Log($"[ZONE SYSTEM] Zone ACTIVATED at turn {turn}");
                PublishStateChange(_state);
            }

            if (_state == ZoneState.Active && _firstDamageTurn.HasValue)
            {
                int turnsSinceActivation = turn - _firstDamageTurn.Value;
                if (turnsSinceActivation > 0 && turnsSinceActivation % _config.ShrinkInterval == 0)
                {
                    Debug.Log($"[ZONE SYSTEM] Shrinking zone: layer={_currentLayer}, step={_stepInLayer}");
                    AdvanceZone();
                }
            }
        }

        private void OnDamageDealt(int turn)
        {
            Debug.Log($"[ZONE SYSTEM] Damage dealt at turn {turn}, firstDamageDealt={_firstDamageDealt}, state={_state}");

            if (!_firstDamageDealt && _state == ZoneState.Inactive)
            {
                _firstDamageDealt = true;
                int oldActivationTurn = _activationTurn;
                _activationTurn = CalculateActivationTurn();
                Debug.Log($"[ZONE SYSTEM] First damage recorded, activation_turn: {oldActivationTurn} -> {_activationTurn}");
            }
        }

        private void OnFigureTurnEnded(ZoneFigureTurnEndedMessage msg)
        {
            if (_state != ZoneState.Active)
                return;

            var status = GetCellStatus(msg.Row, msg.Col);
            Debug.Log($"[ZONE SYSTEM] Figure turn ended at ({msg.Row},{msg.Col}), status={status}");

            if (IsDangerCell(msg.Row, msg.Col))
            {
                int damage = CalculateDamage(msg.Target.MaxHP);
                Debug.Log($"[ZONE SYSTEM] Dealing {damage} damage to figure (maxHP={msg.Target.MaxHP})");
                // Урон применяется в ZoneDamageService через UnitTakeZoneDamageMessage
                _damagePublisher.Publish(new UnitTakeZoneDamageMessage(
                    msg.Target, damage, new GridPosition(msg.Row, msg.Col)));
            }
        }

        public CellStatus GetCellStatus(int row, int col)
        {
            var pos = new GridPosition(row, col);

            if (_dangerCells.Contains(pos))
                return CellStatus.Danger;
            if (_warningCells.Contains(pos))
                return CellStatus.Warning;
            return CellStatus.Safe;
        }

        public ZoneState GetCurrentState() => _state;

        public int GetActivationTurn() => _activationTurn;

        private int CalculateActivationTurn()
        {
            if (_firstDamageTurn.HasValue)
            {
                return Mathf.Max(_config.MinTurn,
                    Mathf.Min(_config.MaxTurn, _firstDamageTurn.Value + 1));
            }
            return _config.MinTurn;
        }

        private int CalculateDamage(int unitMaxHP)
        {
            int flatDamage = _config.ZoneDamageFlat;
            int percentDamage = Mathf.FloorToInt(unitMaxHP * _config.ZoneDamagePercent);
            return flatDamage + percentDamage;
        }

        private void UpdateZoneCells()
        {
            var context = new ZoneContext(
                _config.BoardSize,
                _currentLayer,
                _stepInLayer,
                _config.ShrinkInterval,
                _config.SafeZoneMinSize
            );

            _warningCells = _strategy.GetWarningCells(context).ToHashSet();
            _dangerCells = _strategy.GetDangerCells(context).ToHashSet();

            Debug.Log($"[ZONE SYSTEM] Cells updated: {_warningCells.Count} warning, {_dangerCells.Count} danger");
            _cellsUpdatedPublisher.Publish(new ZoneCellsUpdatedMessage(
                _warningCells.ToArray(),
                _dangerCells.ToArray()
            ));
        }

        private void AdvanceZone()
        {
            var context = new ZoneContext(
                _config.BoardSize,
                _currentLayer,
                _stepInLayer,
                _config.ShrinkInterval,
                _config.SafeZoneMinSize
            );

            if (_strategy.HasNextStep(context))
            {
                _strategy.AdvanceStep(ref context);
                _currentLayer = context.CurrentLayer;
                _stepInLayer = context.StepInLayer;
                Debug.Log($"[ZONE SYSTEM] Advanced to layer={_currentLayer}, step={_stepInLayer}");
                UpdateZoneCells();
            }
            else
            {
                _state = ZoneState.MinSizeReached;
                Debug.Log($"[ZONE SYSTEM] Zone reached minimum size");
                PublishStateChange(_state);
            }
        }

        private bool IsDangerCell(int row, int col)
        {
            return _dangerCells.Contains(new GridPosition(row, col));
        }

        private void PublishStateChange(ZoneState state)
        {
            _stateChangedPublisher.Publish(new ZoneStateChangedMessage(state));
        }

        public ZoneShrinkState SaveState()
        {
            return new ZoneShrinkState
            {
                State = _state,
                CurrentLayer = _currentLayer,
                StepInLayer = _stepInLayer,
                ActivationTurn = _activationTurn,
                FirstDamageTurn = _firstDamageTurn,
                FirstDamageDealt = _firstDamageDealt
            };
        }

        public void LoadState(ZoneShrinkState state)
        {
            _state = state.State;
            _currentLayer = state.CurrentLayer;
            _stepInLayer = state.StepInLayer;
            _activationTurn = state.ActivationTurn;
            _firstDamageTurn = state.FirstDamageTurn;
            _firstDamageDealt = state.FirstDamageDealt;

            UpdateZoneCells();
            PublishStateChange(_state);
        }

        void IDisposable.Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
