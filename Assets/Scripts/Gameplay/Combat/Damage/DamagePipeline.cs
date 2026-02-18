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
            float current = context.RawDamage;
            float raw = current;

            foreach (IDamageModifier modifier in _modifiers)
            {
                current = modifier.Modify(context, current);
            }

            float final = current < 0 ? 0f : current;
            return new DamageResult(raw, final, blocked: false, dodged: false);
        }
    }
}
