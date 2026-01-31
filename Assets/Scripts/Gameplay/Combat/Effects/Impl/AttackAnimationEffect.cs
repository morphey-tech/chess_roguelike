using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Plays attack animation on the attacker.
    /// </summary>
    public sealed class AttackAnimationEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Animation;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly GridPosition _targetPosition;
        private readonly string _attackId;

        public AttackAnimationEffect(Figure attacker, GridPosition targetPosition, string attackId)
        {
            _attacker = attacker;
            _targetPosition = targetPosition;
            _attackId = attackId;
        }

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            context.FigurePresenter.PlayAttack(_attacker.Id, _targetPosition);
            context.Logger.Info($"{_attacker} [{_attackId}] attacks");
            return UniTask.CompletedTask;
        }
    }
}
