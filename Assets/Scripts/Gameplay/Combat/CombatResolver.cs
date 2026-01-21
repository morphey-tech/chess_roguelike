using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class CombatResolver
    {
        private readonly ILogger<CombatResolver> _logger;

        public CombatResolver(ILogService logService)
        {
            _logger = logService.CreateLogger<CombatResolver>();
        }

        public CombatResult Resolve(HitContext context)
        {
            List<IPassive> attackerPassives = context.Attacker.Passives.OrderBy(p => p.Priority).ToList();
            List<IPassive> defenderPassives = context.Target.Passives.OrderBy(p => p.Priority).ToList();

            foreach (IPassive passive in attackerPassives)
                passive.OnPreDamage(context);
            
            context.FinalDamage = (int)(context.BaseDamage * context.DamageMultiplier);
            
            int totalDamage = 0;
            for (int i = 0; i < context.HitCount && !context.Target.Stats.IsDead; i++)
            {
                int hitDamage = i == 0 ? context.FinalDamage : context.FinalDamage / context.HitCount;
                context.TargetDied = context.Target.Stats.TakeDamage(hitDamage);
                totalDamage += hitDamage;
            }
            
            context.FinalDamage = totalDamage;
            
            foreach (IPassive passive in attackerPassives)
                passive.OnPostDamage(context);
            
            foreach (IPassive passive in defenderPassives)
                passive.OnPostDamage(context);
            
            bool attackerMoves = context.AttackerMovesOnKill 
                && context.TargetDied 
                && AttackUtils.GetDistance(context.AttackerPosition, context.TargetPosition) == 1;
            
            _logger.Info($"Combat: {context.Attacker} -> {context.Target}, DMG:{totalDamage}, Died:{context.TargetDied}, Heal:{context.HealedAmount}, Crit:{context.IsCritical}");

            return new CombatResult
            {
                DamageDealt = totalDamage,
                TargetDied = context.TargetDied,
                HealedAmount = context.HealedAmount,
                AttackerMoves = attackerMoves,
                WasCritical = context.IsCritical
            };
        }
    }
}
