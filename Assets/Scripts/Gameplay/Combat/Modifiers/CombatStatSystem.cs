using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class CombatStatSystem : ICombatStatSystem
    {
        private readonly IEnumerable<ICombatModifierProvider> _providers;

        [Inject]
        private CombatStatSystem(IEnumerable<ICombatModifierProvider> providers)
        {
            _providers = providers;
        }

        public CalculatedHitStats Calculate(HitContext hit)
        {
            //TODO: add message in exceptions
            CombatStatContext ctx = new()
            {
                Attacker = hit.Attacker ?? throw new NullReferenceException(),
                Target = hit.Target ?? throw new NullReferenceException(),
                Profile = hit.Profile,
                Grid = hit.Grid ?? throw new NullReferenceException(),

                Damage = hit.Profile.Damage,
                Range = hit.Profile.Range
            };

            IOrderedEnumerable<ICombatStatModifier> mods = _providers
                .SelectMany(p => p.GetModifiers(hit))
                .OrderBy(m => m.Priority);

            foreach (ICombatStatModifier? mod in mods)
                mod.Modify(ctx);

            return new CalculatedHitStats
            {
                Damage = ctx.Damage,
                Range = ctx.Range
            };
        }
    }
}