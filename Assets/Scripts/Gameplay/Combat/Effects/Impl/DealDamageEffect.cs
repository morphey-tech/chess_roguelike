using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies damage to target, updates ActionContext, plays visual effects.
    /// If target dies, adds KillEffect to pending effects.
    /// </summary>
    public sealed class DealDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Damage;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly int _damage;
        private readonly bool _isCritical;

        public DealDamageEffect(Figure attacker, Figure target, int damage, bool isCritical = false)
        {
            _attacker = attacker;
            _target = target;
            _damage = damage;
            _isCritical = isCritical;
        }

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            // Apply damage
            bool died = _target.Stats.TakeDamage(_damage);
            
            // Visual effect
            context.FigurePresenter.PlayDamageEffect(_target.Id);
            
            // Update ActionContext
            context.ActionContext.LastDamageDealt = _damage;
            
            // Log
            string critText = _isCritical ? " (CRIT)" : "";
            context.Logger.Info($"{_target} takes {_damage} damage{critText}. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");

            // If died, add KillEffect and trigger passives
            if (died)
            {
                context.Passives.TriggerKill(_attacker, _target);
                context.Passives.TriggerDeath(_target, _attacker);
                
                BoardCell targetCell = context.Grid.GetBoardCell(context.ActionContext.To);
                context.PendingEffects.Add(new KillEffect(_target, targetCell));
            }

            return UniTask.CompletedTask;
        }
    }
}
