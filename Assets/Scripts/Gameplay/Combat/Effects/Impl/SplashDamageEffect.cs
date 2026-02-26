using System;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies splash damage via DamageApplier. Смерть обрабатывает LifeService.
    /// Все сплеш цели получают урон одновременно (через IsParallel = true).
    /// </summary>
    public sealed class SplashDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure[]? _targets;
        private readonly int _damage;

        public SplashDamageEffect(Figure attacker, Figure[] targets, int damage)
        {
            _attacker = attacker;
            _targets = targets;
            _damage = damage;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_targets == null || _targets.Length == 0)
                return;

            // Применяем урон всем целям и создаём визуальные события с IsParallel = true
            foreach (Figure target in _targets)
            {
                if (target.Stats.CurrentHp.Value <= 0)
                {
                    continue;
                }

                DamageContext dmgCtx = new(_attacker, target, _damage, false, false, false,
                    "splash", Array.Empty<IDamageModifier>());
                (DamageResult result, _) = context.DamageApplier.Apply(context, dmgCtx);

                // IsParallel = true — команды будут выполнены параллельно
                context.AddVisualEvent(new DamageVisualEvent(
                    target.Id,
                    result.Final,
                    isCritical: false,
                    isDodged: false,
                    damageType: "splash",
                    isParallel: true));

                context.Logger.Info($"Splash hit {target} for {result.Final} damage. HP: {target.Stats.CurrentHp.Value}/{target.Stats.MaxHp}");
            }
        }
    }
}
