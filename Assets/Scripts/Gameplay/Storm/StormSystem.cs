using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Configs.Storm;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Messages;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Основная система shrinking zone
    /// </summary>
    public class StormSystem : IStormQueryService, IDisposable
    {
        public StormState CurrentState { get; private set; } = StormState.Inactive;

        public int CurrentLayer { get; private set; }

        public int StepInLayer { get; private set; }

        public int ActivationTurn { get; private set; }
        public int? FirstDamageTurn => _firstDamageTurn;
        public bool FirstDamageDealt => _firstDamageDealt;

        private readonly StormConfig _config;
        private readonly IStormStrategy _strategy;
        private readonly IPublisher<string, StormMessage> _stormPublisher;
        private readonly IDisposable _subscriptions;
        private readonly ILogger<StormSystem> _logger;

        private int? _firstDamageTurn;
        private bool _firstDamageDealt;

        private HashSet<GridPosition> _warningCells;
        private HashSet<GridPosition> _dangerCells;

        public StormSystem(
            StormConfig config,
            IStormStrategy strategy,
            IPublisher<string, StormMessage> stormPublisher,
            ISubscriber<string, StormMessage> stormSubscriber,
            ILogService logService)
        {
            _config = config;
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _stormPublisher = stormPublisher;
            _logger = logService.CreateLogger<StormSystem>();

            _warningCells = new HashSet<GridPosition>();
            _dangerCells = new HashSet<GridPosition>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            stormSubscriber.Subscribe(StormMessage.BATTLE_STARTED, OnBattleStarted).AddTo(bag);
            stormSubscriber.Subscribe(StormMessage.TURN_STARTED, OnTurnStarted).AddTo(bag);
            stormSubscriber.Subscribe(StormMessage.DAMAGE_DEALT, OnDamageDealt).AddTo(bag);
            stormSubscriber.Subscribe(StormMessage.FIGURE_TURN_ENDED, OnFigureTurnEnded).AddTo(bag);
            _subscriptions = bag.Build();
        }
        
        private void OnBattleStarted(StormMessage message)
        {
            CurrentState = StormState.Inactive;
            CurrentLayer = 0;
            StepInLayer = 0;
            ActivationTurn = CalculateActivationTurn();
            _firstDamageTurn = null;
            _firstDamageDealt = false;

            _warningCells.Clear();
            _dangerCells.Clear();

            _logger.Info($"Battle started, activation_turn={ActivationTurn}");
            PublishStateChange(CurrentState);
        }

        private void OnTurnStarted(StormMessage message)
        {
            _logger.Debug($"Turn {message.Turn} started, state={CurrentState}, activation_turn={ActivationTurn}, firstDamageTurn={_firstDamageTurn?.ToString() ?? "null"}, layer={CurrentLayer}, step={StepInLayer}");

            if (CurrentState == StormState.Inactive && message.Turn >= ActivationTurn)
            {
                CurrentState = StormState.Active;
                _firstDamageTurn = message.Turn;
                StepInLayer = 1;
                UpdateStormCells();
                _logger.Info($"Zone ACTIVATED at turn {message.Turn}, layer={CurrentLayer}, step={StepInLayer}, dangerCells={_dangerCells.Count}, warningCells={_warningCells.Count}");
                PublishStateChange(CurrentState);
            }

            if (CurrentState == StormState.Active && _firstDamageTurn.HasValue)
            {
                int turnsSinceActivation = message.Turn - _firstDamageTurn.Value;
                _logger.Debug($"Checking shrink: turnsSinceActivation={turnsSinceActivation}, shrinkInterval={_config.ShrinkInterval}, shouldShrink={turnsSinceActivation > 0 && turnsSinceActivation % _config.ShrinkInterval == 0}");
                if (turnsSinceActivation > 0 && turnsSinceActivation % _config.ShrinkInterval == 0)
                {
                    _logger.Debug($"Shrinking zone: BEFORE layer={CurrentLayer}, step={StepInLayer}");
                    AdvanceStorm();
                    _logger.Debug($"Shrinking zone: AFTER layer={CurrentLayer}, step={StepInLayer}");
                }
            }
        }

        private void OnDamageDealt(StormMessage message)
        {
            _logger.Debug($"Damage dealt at turn {message.Turn}, firstDamageDealt={_firstDamageDealt}, state={CurrentState}");

            if (!_firstDamageDealt && CurrentState == StormState.Inactive)
            {
                _firstDamageDealt = true;
                int oldActivationTurn = ActivationTurn;
                ActivationTurn = CalculateActivationTurn();
                _logger.Info($"First damage recorded, activation_turn: {oldActivationTurn} -> {ActivationTurn}");
            }
        }


        private void OnFigureTurnEnded(StormMessage msg)
        {
            _logger.Debug($"Figure turn ended at ({msg.Row},{msg.Col})");
        }

        public StormCellStatus GetCellStatus(int row, int col)
        {
            GridPosition pos = new(row, col);
            if (_dangerCells.Contains(pos))
            {
                return StormCellStatus.Danger;
            }
            return _warningCells.Contains(pos) ? StormCellStatus.Warning : StormCellStatus.Safe;
        }

        public int GetDangerCellsCount() => _dangerCells.Count;
        public int GetWarningCellsCount() => _warningCells.Count;

        public StormState GetCurrentState() => CurrentState;

        public int GetActivationTurn() => ActivationTurn;

        private int CalculateActivationTurn()
        {
            if (_firstDamageTurn.HasValue)
            {
                return UnityEngine.Mathf.Max(_config.MinTurn,
                    UnityEngine.Mathf.Min(_config.MaxTurn, _firstDamageTurn.Value + 1));
            }
            return _config.MinTurn;
        }

        public int CalculateDamage(int unitMaxHP)
        {
            int flatDamage = _config.ZoneDamageFlat;
            int percentDamage = UnityEngine.Mathf.FloorToInt(unitMaxHP * _config.ZoneDamagePercent);
            return flatDamage + percentDamage;
        }

        private void UpdateStormCells()
        {
            StormContext context = new(
                _config.BoardSize,
                CurrentLayer,
                StepInLayer,
                _config.ShrinkInterval,
                _config.SafeZoneMinSize
            );

            _warningCells = _strategy.GetWarningCells(context).ToHashSet();
            _dangerCells = _strategy.GetDangerCells(context).ToHashSet();
            _logger.Info($"Cells updated: layer={CurrentLayer}, step={StepInLayer}, {_warningCells.Count} warning, {_dangerCells.Count} danger");

            string dangerCellsStr = string.Join(", ", _dangerCells.Select(c => $"({c.Row},{c.Column})"));
            _logger.Debug($"Danger cells: {dangerCellsStr}");

            string warningCellsStr = string.Join(", ", _warningCells.Select(c => $"({c.Row},{c.Column})"));
            _logger.Debug($"Warning cells: {warningCellsStr}");

            _stormPublisher.Publish(StormMessage.CELLS_UPDATED, StormMessage.CellsUpdated(
                _warningCells.ToArray(), _dangerCells.ToArray()
            ));
        }

        private void AdvanceStorm()
        {
            StormContext context = new(
                _config.BoardSize,
                CurrentLayer,
                StepInLayer,
                _config.ShrinkInterval,
                _config.SafeZoneMinSize
            );

            if (_strategy.HasNextStep(context))
            {
                _strategy.AdvanceStep(ref context);
                CurrentLayer = context.CurrentLayer;
                StepInLayer = context.StepInLayer;
                _logger.Info($"Advanced to layer={CurrentLayer}, step={StepInLayer}");
                UpdateStormCells();
            }
            else
            {
                CurrentState = StormState.MinSizeReached;
                _logger.Info("Zone reached minimum size");
                PublishStateChange(CurrentState);
            }
        }

        private bool IsDangerCell(int row, int col)
        {
            return _dangerCells.Contains(new GridPosition(row, col));
        }

        private void PublishStateChange(StormState state)
        {
            _stormPublisher.Publish(StormMessage.STATE_CHANGED, StormMessage.StateChanged(state));
        }

        public StormSaveContext SaveState()
        {
            return new StormSaveContext
            {
                State = CurrentState,
                CurrentLayer = CurrentLayer,
                StepInLayer = StepInLayer,
                ActivationTurn = ActivationTurn,
                FirstDamageTurn = _firstDamageTurn,
                FirstDamageDealt = _firstDamageDealt
            };
        }

        public void LoadState(StormSaveContext state)
        {
            CurrentState = state.State;
            CurrentLayer = state.CurrentLayer;
            StepInLayer = state.StepInLayer;
            ActivationTurn = state.ActivationTurn;
            _firstDamageTurn = state.FirstDamageTurn;
            _firstDamageDealt = state.FirstDamageDealt;

            UpdateStormCells();
            PublishStateChange(CurrentState);
        }

        void IDisposable.Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
