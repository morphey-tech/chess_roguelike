using Cysharp.Threading.Tasks;
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

        public AttackStep(
            string id,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
            IFigurePresenter figurePresenter)
        {
            Id = id;
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
            _figurePresenter = figurePresenter;
        }

        public UniTask ExecuteAsync(TurnStepContext context)
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

            _figurePresenter.PlayAttack(context.Actor.Id, context.To);
            _figurePresenter.PlayDamageEffect(defender.Id);

            context.LastDamageDealt = result.DamageDealt;
            context.LastAttackKilledTarget = result.TargetDied;

            if (result.TargetDied)
            {
                targetCell.RemoveFigure();
                _figurePresenter.RemoveFigure(defender.Id);
            }

            return UniTask.CompletedTask;
        }
    }
}
