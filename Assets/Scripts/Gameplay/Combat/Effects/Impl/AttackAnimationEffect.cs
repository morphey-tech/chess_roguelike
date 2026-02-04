using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Queues attack animation command.
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

        public void Apply(CombatEffectContext context)
        {
            var visualCtx = new AttackVisualContext(_attacker.Id, _targetPosition, _attackId);
            context.Visuals.Enqueue(new AttackCommand(visualCtx));
            context.Logger.Info($"{_attacker} [{_attackId}] attacks");
        }
    }
}
