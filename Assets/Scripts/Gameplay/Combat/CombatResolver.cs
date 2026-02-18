using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Figures;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Single place for damage formula: uses only Attack.Value and Defence.Value.
    /// Passives only manage stat modifiers — no formulas here from passives.
    /// </summary>
    public sealed class CombatResolver
    {
        private readonly ILogger<CombatResolver> _logger;

        [Inject]
        private CombatResolver(ILogService logService)
        {
            _logger = logService.CreateLogger<CombatResolver>();
        }

        public CombatResult Resolve(HitContext context)
        {
            Figure attacker = context.Attacker ?? throw new NullReferenceException(nameof(context.Attacker));
            Figure target = context.Target ?? throw new NullReferenceException(nameof(context.Target));

            float atk = attacker.Stats.Attack.Value;
            float def = target.Stats.Defence.Value;
            float baseDamage = Math.Max(1f, atk - def);
            
            _logger.Debug($"Damage calc: ATK={atk} DEF={def} BaseDamage={baseDamage}");

            var effects = new List<ICombatEffect>();
            effects.Add(new PrimaryHitEffect(
                attacker,
                target,
                context.AttackerPosition,
                context.TargetPosition,
                baseDamage,
                context.AttackId,
                context.Delivery,
                context.ProjectileConfigId));

            // Effects from attack strategy (splash, pierce, etc.)
            effects.AddRange(context.Effects);

            // Sort by Phase, then by OrderInPhase
            var sortedEffects = effects
                .OrderBy(e => e.Phase)
                .ThenBy(e => e.OrderInPhase)
                .ToList();

            _logger.Info($"Combat prepared: {context.Attacker} -> {context.Target}, Effects:{sortedEffects.Count}");

            return new CombatResult(
                context.Attacker,
                context.AttackerPosition,
                context.Delivery,
                hits: new List<HitResult>(),
                sortedEffects,
                damageDealt: 0,
                targetDied: false,
                wasCritical: false);
        }
    }
}
