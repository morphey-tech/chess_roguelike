using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Orchestrates combat phases and builds effect list.
    /// Does NOT apply damage directly — that's done by effects.
    /// </summary>
    public sealed class CombatResolver
    {
        private readonly PassiveTriggerService _passives;
        private readonly ILogger<CombatResolver> _logger;

        public CombatResolver(PassiveTriggerService passives, ILogService logService)
        {
            _passives = passives;
            _logger = logService.CreateLogger<CombatResolver>();
        }

        public CombatResult Resolve(HitContext context)
        {
            var effects = new List<ICombatEffect>();
            
            // === Before Hit Phase ===
            BeforeHitContext before = new()
            {
                Attacker = context.Attacker,
                Target = context.Target,
                BaseDamage = context.BaseDamage
            };

            _passives.TriggerBeforeHit(context.Attacker, context.Target, before);
            int finalDamage = (int)(before.BaseDamage * before.DamageMultiplier) + before.BonusDamage;
            _logger.Debug($"Damage calc: Base={before.BaseDamage}, Mult={before.DamageMultiplier}, Bonus={before.BonusDamage} => Final={finalDamage}");

            // === Build Effects List ===
            // Attack animation
            effects.Add(new AttackAnimationEffect(context.Attacker, context.TargetPosition, context.AttackId));
            
            // Primary damage (will apply HP change and add KillEffect if needed)
            effects.Add(new DealDamageEffect(context.Attacker, context.Target, finalDamage, before.IsCritical));
            
            // Effects from attack strategy (splash, pierce, etc.)
            effects.AddRange(context.Effects);

            // === After Hit Phase ===
            // Note: AfterHit passives see expected damage but HP not yet changed.
            // They add effects that will be applied after damage.
            AfterHitContext after = new()
            {
                Attacker = context.Attacker,
                Target = context.Target,
                AttackerPosition = context.AttackerPosition,
                TargetPosition = context.TargetPosition,
                Grid = context.Grid,
                DamageDealt = finalDamage,
                TargetDied = finalDamage >= context.Target.Stats.CurrentHp,
                WasCritical = before.IsCritical
            };

            _passives.TriggerAfterHit(context.Attacker, context.Target, after);
            
            // Effects from passives
            effects.AddRange(after.Effects);
            
            // Sort by Phase, then by OrderInPhase
            var sortedEffects = effects
                .OrderBy(e => e.Phase)
                .ThenBy(e => e.OrderInPhase)
                .ToList();

            _logger.Info($"Combat prepared: {context.Attacker} -> {context.Target}, DMG:{finalDamage}, Crit:{before.IsCritical}, Effects:{sortedEffects.Count}");

            return new CombatResult(sortedEffects, finalDamage, after.TargetDied, before.IsCritical);
        }
    }
}
