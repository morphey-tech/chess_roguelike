using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies splash/AoE damage to an additional target.
    /// If target dies, adds KillEffect to pending effects.
    /// </summary>
    public sealed class SplashDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly int _damage;

        public SplashDamageEffect(Figure attacker, Figure target, int damage)
        {
            _attacker = attacker;
            _target = target;
            _damage = damage;
        }

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            if (_target == null || _target.Stats.CurrentHp <= 0)
                return UniTask.CompletedTask;

            // Apply damage
            bool died = _target.Stats.TakeDamage(_damage);
            
            // Visual
            context.FigurePresenter.PlayDamageEffect(_target.Id);
            context.Logger.Info($"Splash hit {_target} for {_damage} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");

            // If died, add KillEffect
            if (died)
            {
                context.Passives.TriggerKill(_attacker, _target);
                context.Passives.TriggerDeath(_target, _attacker);
                
                BoardCell cell = context.Grid.FindFigure(_target);
                context.PendingEffects.Add(new KillEffect(_target, cell, "splash"));
            }

            return UniTask.CompletedTask;
        }
    }
}
