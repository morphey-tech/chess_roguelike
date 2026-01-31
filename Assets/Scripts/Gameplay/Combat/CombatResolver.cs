using System.Collections.Generic;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
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
            var before = new BeforeHitContext
            {
                Attacker = context.Attacker,
                Target = context.Target,
                BaseDamage = context.BaseDamage
            };

            _passives.TriggerBeforeHit(context.Attacker, context.Target, before);

            int finalDamage = (int)(before.BaseDamage * before.DamageMultiplier) + before.BonusDamage;
            
            _logger.Debug($"Damage calc: Base={before.BaseDamage}, Mult={before.DamageMultiplier}, Bonus={before.BonusDamage} => Final={finalDamage}");
            _logger.Debug($"Target HP before: {context.Target.Stats.CurrentHp}/{context.Target.Stats.MaxHp}");
            
            bool died = context.Target.Stats.TakeDamage(finalDamage);
            
            _logger.Debug($"Target HP after: {context.Target.Stats.CurrentHp}/{context.Target.Stats.MaxHp}, Died={died}");

            var after = new AfterHitContext
            {
                Attacker = context.Attacker,
                Target = context.Target,
                AttackerPosition = context.AttackerPosition,
                TargetPosition = context.TargetPosition,
                Grid = context.Grid,
                DamageDealt = finalDamage,
                TargetDied = died,
                WasCritical = before.IsCritical
            };

            _passives.TriggerAfterHit(context.Attacker, context.Target, after);

            if (died)
            {
                _passives.TriggerKill(context.Attacker, context.Target);
                _passives.TriggerDeath(context.Target, context.Attacker);
            }

            bool attackerMoves = context.AttackerMovesOnKill 
                && died 
                && AttackUtils.GetDistance(context.AttackerPosition, context.TargetPosition) == 1;

            _logger.Info($"Combat: {context.Attacker} -> {context.Target}, DMG:{finalDamage}, Died:{died}, Heal:{after.HealedAmount}, Crit:{before.IsCritical}");

            // Process additional targets (splash, pierce, etc.)
            List<AdditionalTargetResult> additionalResults = ProcessAdditionalTargets(context);

            return new CombatResult
            {
                DamageDealt = finalDamage,
                TargetDied = died,
                HealedAmount = after.HealedAmount,
                AttackerMoves = attackerMoves,
                WasCritical = before.IsCritical,
                AdditionalResults = additionalResults,
                AttackerMovedTo = after.AttackerMovedTo,
                BonusMoveDistance = after.BonusMoveDistance
            };
        }

        private List<AdditionalTargetResult> ProcessAdditionalTargets(HitContext context)
        {
            var results = new List<AdditionalTargetResult>();
            
            if (context.AdditionalTargets == null || context.AdditionalTargets.Count == 0)
                return results;

            int additionalDamage = (int)(context.BaseDamage * context.AdditionalDamageMultiplier);

            foreach (Figure target in context.AdditionalTargets)
            {
                if (target == null || target.Stats.CurrentHp <= 0)
                    continue;

                bool targetDied = target.Stats.TakeDamage(additionalDamage);
                
                _logger.Info($"Splash/AoE: {context.Attacker} -> {target}, DMG:{additionalDamage}, Died:{targetDied}");

                if (targetDied)
                {
                    _passives.TriggerKill(context.Attacker, target);
                    _passives.TriggerDeath(target, context.Attacker);
                }

                results.Add(new AdditionalTargetResult
                {
                    Target = target,
                    DamageDealt = additionalDamage,
                    Died = targetDied
                });
            }

            return results;
        }
    }
}
