using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat.Damage;
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
        private readonly ILogger<CombatResolver> _logger;
        private readonly IDamageTokenStore _tokenStore;
        private readonly IDamagePipeline _damagePipeline;

        public CombatResolver(ILogService logService, IDamageTokenStore tokenStore, IDamagePipeline damagePipeline)
        {
            _logger = logService.CreateLogger<CombatResolver>();
            _tokenStore = tokenStore;
            _damagePipeline = damagePipeline;
        }

        public CombatResult Resolve(HitContext context)
        {
            var effects = new List<ICombatEffect>();

            effects.Add(new PrimaryHitEffect(
                context.Attacker,
                context.Target,
                context.AttackerPosition,
                context.TargetPosition,
                context.BaseDamage,
                context.AttackId,
                context.Delivery,
                context.ProjectileConfigId,
                _tokenStore,
                _damagePipeline));

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
