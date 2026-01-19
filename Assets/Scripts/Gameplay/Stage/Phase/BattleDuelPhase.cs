using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Battle phase for duel mode - players take turns until victory/defeat.
    /// </summary>
    public sealed class BattleDuelPhase : IStagePhase, IDisposable
    {
        private readonly TurnSystem _turnSystem;
        private readonly ISubscriber<FigureDeathMessage> _deathSubscriber;
        private readonly ILogger<BattleDuelPhase> _logger;

        private StageContext _context;
        private IDisposable _subscription;
        private int _playerFiguresAlive;
        private int _enemyFiguresAlive;

        public BattleDuelPhase(
            TurnSystem turnSystem,
            ISubscriber<FigureDeathMessage> deathSubscriber,
            ILogService logService)
        {
            _turnSystem = turnSystem;
            _deathSubscriber = deathSubscriber;
            _logger = logService.CreateLogger<BattleDuelPhase>();
        }

        public UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _context = context;

            // Count figures
            CountFigures(context);

            _logger.Info($"Battle started! Player: {_playerFiguresAlive}, Enemy: {_enemyFiguresAlive}");

            // Subscribe to deaths
            _subscription = _deathSubscriber.Subscribe(OnFigureDeath);

            // Start turns
            _turnSystem.StartBattle();

            return UniTask.FromResult(PhaseResult.WaitForCompletion);
        }

        private void CountFigures(StageContext context)
        {
            _playerFiguresAlive = 0;
            _enemyFiguresAlive = 0;

            foreach (var cell in context.Grid.AllCells())
            {
                if (cell.OccupiedBy == null) continue;

                if (cell.OccupiedBy.Team == Team.Player)
                    _playerFiguresAlive++;
                else
                    _enemyFiguresAlive++;
            }
        }

        private void OnFigureDeath(FigureDeathMessage message)
        {
            if (message.Team == Team.Player)
            {
                _playerFiguresAlive--;
                _logger.Info($"Player figure died. Remaining: {_playerFiguresAlive}");
            }
            else
            {
                _enemyFiguresAlive--;
                _logger.Info($"Enemy figure died. Remaining: {_enemyFiguresAlive}");
            }

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (_playerFiguresAlive <= 0)
            {
                _logger.Info("Battle ended: DEFEAT");
                Complete(PhaseResult.Defeat);
            }
            else if (_enemyFiguresAlive <= 0)
            {
                _logger.Info("Battle ended: VICTORY");
                Complete(PhaseResult.Victory);
            }
        }

        private void Complete(PhaseResult result)
        {
            _subscription?.Dispose();
            _subscription = null;
            _context?.CompletePhase?.Invoke(result);
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
