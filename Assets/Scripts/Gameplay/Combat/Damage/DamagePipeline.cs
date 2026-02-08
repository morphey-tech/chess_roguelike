using System.Collections.Generic;
using System.Linq;

namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class DamagePipeline : IDamagePipeline
    {
        private readonly IReadOnlyList<IDamageModifier> _modifiers;

        public DamagePipeline(IEnumerable<IDamageModifier> modifiers)
        {
            _modifiers = modifiers
                .OrderBy(m => m.Order)
                .ToList();
        }

        public DamageResult Calculate(DamageContext context)
        {
            int current = context.RawDamage;
            int raw = current;

            foreach (IDamageModifier modifier in _modifiers)
            {
                current = modifier.Modify(context, current);
            }

            int final = current < 0 ? 0 : current;
            return new DamageResult(raw, final, blocked: false, dodged: false);
        }
    }
}
