using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Queues heal visual command.
    /// Note: actual healing is already applied during passive execution.
    /// 
    /// FLEXIBILITY: If heal needs to happen before thorns/secondary damage,
    /// create effect with Phase = SecondaryDamage and appropriate OrderInPhase.
    /// Example: new HealEffect(target, amount, CombatEffectPhase.SecondaryDamage, 5)
    /// </summary>
    public sealed class HealEffect : ICombatEffect
    {
        public CombatEffectPhase Phase { get; }
        public int OrderInPhase { get; }
        
        private readonly Figure _target;
        private readonly int _healedAmount;

        public HealEffect(
            Figure target, 
            int healedAmount, 
            CombatEffectPhase phase = CombatEffectPhase.Healing, 
            int orderInPhase = 0)
        {
            _target = target;
            _healedAmount = healedAmount;
            Phase = phase;
            OrderInPhase = orderInPhase;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_healedAmount > 0)
            {
                // Visual: Queue heal effect
                var visualCtx = new HealVisualContext(_target.Id, _healedAmount);
                context.Visuals.Enqueue(new HealCommand(visualCtx));
                context.Logger.Info($"{_target} healed for {_healedAmount}. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
            }
        }
    }
}
