using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class AttackStep : ITurnStep
    {
        public string Id { get; }

        private readonly AttackStrategyFactory _attackFactory;
        private readonly CombatResolver _combatResolver;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ILogger<AttackStep> _logger;

        public AttackStep(
            string id,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
            IFigurePresenter figurePresenter,
            IPublisher<FigureDeathMessage> deathPublisher,
            ILogger<AttackStep> logger)
        {
            Id = id;
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
            _figurePresenter = figurePresenter;
            _deathPublisher = deathPublisher;
            _logger = logger;
        }

        public UniTask ExecuteAsync(ActionContext context)
        {
            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure defender = targetCell?.OccupiedBy;

            if (defender == null || defender.Team == context.Actor.Team)
                return UniTask.CompletedTask;

            IAttackStrategy attackStrategy = _attackFactory.Get(context.Actor.AttackId);
            
            if (!attackStrategy.CanAttack(context.Actor, context.From, context.To, context.Grid))
                return UniTask.CompletedTask;

            HitContext hitContext = attackStrategy.CreateHitContext(
                context.Actor, 
                defender, 
                context.From, 
                context.To, 
                context.Grid);

            CombatResult result = _combatResolver.Resolve(hitContext);

            _logger.Info($"{context.Actor} [{attackStrategy.Id}] attacks {defender} for {result.DamageDealt} damage. HP: {defender.Stats.CurrentHp}/{defender.Stats.MaxHp}");

            if (result.HealedAmount > 0)
                _logger.Info($"{context.Actor} healed for {result.HealedAmount}");

            _figurePresenter.PlayAttack(context.Actor.Id, context.To);
            _figurePresenter.PlayDamageEffect(defender.Id);

            context.LastDamageDealt = result.DamageDealt;
            context.LastAttackKilledTarget = result.TargetDied;

            if (result.TargetDied)
            {
                _logger.Info($"{defender} died!");
                targetCell.RemoveFigure();
                _figurePresenter.RemoveFigure(defender.Id);
                _deathPublisher.Publish(new FigureDeathMessage(defender.Id, defender.Team));
            }

            // Process additional targets (splash, pierce, etc.)
            if (result.AdditionalResults != null)
            {
                foreach (AdditionalTargetResult additionalResult in result.AdditionalResults)
                {
                    Figure additionalTarget = additionalResult.Target;
                    
                    _figurePresenter.PlayDamageEffect(additionalTarget.Id);
                    _logger.Info($"Splash hit {additionalTarget} for {additionalResult.DamageDealt} damage");

                    if (additionalResult.Died)
                    {
                        _logger.Info($"{additionalTarget} died from splash!");
                        BoardCell additionalCell = context.Grid.FindFigure(additionalTarget);
                        additionalCell?.RemoveFigure();
                        _figurePresenter.RemoveFigure(additionalTarget.Id);
                        _deathPublisher.Publish(new FigureDeathMessage(additionalTarget.Id, additionalTarget.Team));
                    }
                }
            }

            // Handle attacker movement from passives (e.g., auto-retreat for AI)
            if (result.AttackerMovedTo.HasValue)
            {
                _logger.Info($"{context.Actor} retreated to ({result.AttackerMovedTo.Value.Row}, {result.AttackerMovedTo.Value.Column})");
                _figurePresenter.MoveFigure(context.Actor.Id, result.AttackerMovedTo.Value);
                context.From = result.AttackerMovedTo.Value;
            }

            // Request bonus move if passive triggered it (e.g., slippery)
            if (result.BonusMoveDistance.HasValue)
            {
                _logger.Info($"{context.Actor} gets bonus move with distance {result.BonusMoveDistance.Value}");
                context.BonusMoveDistance = result.BonusMoveDistance.Value;
            }

            return UniTask.CompletedTask;
        }
    }
}
