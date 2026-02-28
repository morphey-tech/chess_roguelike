using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
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
        private readonly PassiveTriggerService _passiveTriggerService;

        [Inject]
        private CombatResolver(
            ILogService logService,
            PassiveTriggerService passiveTriggerService)
        {
            _logger = logService.CreateLogger<CombatResolver>();
            _passiveTriggerService = passiveTriggerService;
        }

        public CombatResult Resolve(HitContext context)
        {
            Figure attacker = context.Attacker ?? throw new NullReferenceException(nameof(context.Attacker));
            Figure target = context.Target ?? throw new NullReferenceException(nameof(context.Target));

            // Use raw damage from attack profile - actual damage calculated in PrimaryHitEffect
            // after passives apply their modifiers
            float rawDamage = attacker.Stats.Attack.Value;

            _logger.Debug($"Damage calc: ATK={rawDamage}");

            List<ICombatEffect> effects = new();
            effects.Add(new PrimaryHitEffect(
                attacker,
                target,
                context.AttackerPosition,
                context.TargetPosition,
                rawDamage,
                context.AttackId,
                context.Delivery,
                context.ProjectileConfigId));

            effects.AddRange(context.Effects);
            List<ICombatEffect> sortedEffects = effects
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

        /// <summary>
        /// Рассчитывает урон для превью с учётом пассивок.
        /// Возвращает финальный урон после применения всех модификаторов.
        /// </summary>
        public float CalculatePreviewDamage(Figure attacker, Figure target, BoardGrid grid)
        {
            BeforeHitContext beforeContext = new()
            {
                Attacker = attacker,
                Target = target,
                Grid = grid,
                BaseDamage = attacker.Stats.Attack.Value
            };

            _passiveTriggerService.TriggerBeforeHit(attacker, target, beforeContext);

            float atk = attacker.Stats.Attack.Value;
            float def = target.Stats.Defence.Value;
            float finalDamage = Math.Max(1f, atk - def);

            finalDamage = finalDamage * beforeContext.DamageMultiplier + beforeContext.BonusDamage;
            if (finalDamage < 0)
            {
                finalDamage = 0;
            }

            attacker.Stats.Attack.ClearByContext(ModifierSourceContext.PreviewCalculation);
            attacker.Stats.Defence.ClearByContext(ModifierSourceContext.PreviewCalculation);
            attacker.Stats.Evasion.ClearByContext(ModifierSourceContext.PreviewCalculation);
            target.Stats.Attack.ClearByContext(ModifierSourceContext.PreviewCalculation);
            target.Stats.Defence.ClearByContext(ModifierSourceContext.PreviewCalculation);
            target.Stats.Evasion.ClearByContext(ModifierSourceContext.PreviewCalculation);
            return finalDamage;
        }
    }
}
