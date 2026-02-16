using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Battle phase for duel mode - players take turns until victory/defeat.
    /// </summary>
    public sealed class BattleDuelPhase : IStagePhase, IDisposable
    {
        private readonly TurnService _turnService;
        private readonly ISubscriber<FigureDeathMessage> _deathSubscriber;
        private readonly ISubscriber<TurnChangedMessage> _turnChangedSubscriber;
        private readonly ILogger<BattleDuelPhase> _logger;

        private StageContext _context;
        private IDisposable? _deathSubscription;
        private IDisposable? _turnSubscription;
        private StageResultService? _resultService;
        private CombatStats? _combatStats;
        private bool _completed;

        public BattleDuelPhase(
            TurnService turnService,
            ISubscriber<FigureDeathMessage> deathSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            ILogService logService)
        {
            _turnService = turnService;
            _deathSubscriber = deathSubscriber;
            _turnChangedSubscriber = turnChangedSubscriber;
            _logger = logService.CreateLogger<BattleDuelPhase>();
        }

        public UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _context = context;
            _context.Result = null;

            _combatStats = new CombatStats();
            _resultService = new StageResultService(new StageEndDetector(context.Grid), _combatStats);
            _completed = false;

            _logger.Info("Battle started");

            // Subscribe to deaths
            _deathSubscription = _deathSubscriber.Subscribe(OnFigureDeath);
            _turnSubscription = _turnChangedSubscriber.Subscribe(OnTurnChanged);

            // Start turns
            _turnService.StartBattle();
            _combatStats.Turns = _turnService.TurnNumber;
            CheckBattleEnd();

            return UniTask.FromResult(PhaseResult.WaitForCompletion);
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            if (_combatStats != null)
                _combatStats.Turns = message.TurnNumber;
            CheckBattleEnd();
        }

        private void OnFigureDeath(FigureDeathMessage message)
        {
            if (message.Team == Team.Enemy && _combatStats != null)
                _combatStats.Kills++;

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (_completed)
                return;

            StageResult? result = _resultService?.TryBuild();
            if (result == null)
                return;

            _context.Result = result;
            _logger.Info($"Battle ended: {result.Outcome}, turns={result.TurnCount}, kills={result.EnemiesKilled}");
            Complete(result.Outcome == StageOutcome.Victory ? PhaseResult.Victory : PhaseResult.Defeat);
        }

        private void Complete(PhaseResult result)
        {
            if (_completed)
                return;
            _completed = true;

            _deathSubscription?.Dispose();
            _deathSubscription = null;
            _turnSubscription?.Dispose();
            _turnSubscription = null;
            _context?.CompletePhase?.Invoke(result);
        }

        void IDisposable.Dispose()
        {
            _deathSubscription?.Dispose();
            _turnSubscription?.Dispose();
        }
    }
}
