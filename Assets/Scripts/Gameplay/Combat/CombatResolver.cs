using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat.Contexts;

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
            bool died = context.Target.Stats.TakeDamage(finalDamage);

            var after = new AfterHitContext
            {
                Attacker = context.Attacker,
                Target = context.Target,
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

            return new CombatResult
            {
                DamageDealt = finalDamage,
                TargetDied = died,
                HealedAmount = after.HealedAmount,
                AttackerMoves = attackerMoves,
                WasCritical = before.IsCritical
            };
        }
    }
}
