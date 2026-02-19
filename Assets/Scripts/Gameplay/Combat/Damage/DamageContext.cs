using System.Collections.Generic;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class DamageContext
    {
        public Figure Attacker { get; }
        public Figure Target { get; }
        public float RawDamage { get; }
        public bool IsCritical { get; }
        public bool IsDodged { get; }
        public bool IsCancelled { get; }
        public string AttackId { get; }
        public IReadOnlyList<IDamageModifier> Modifiers { get; }

        public DamageContext(
            Figure attacker,
            Figure target,
            float rawDamage,
            bool isCritical,
            bool isDodged,
            bool isCancelled,
            string attackId,
            IReadOnlyList<IDamageModifier> modifiers)
        {
            Attacker = attacker;
            Target = target;
            RawDamage = rawDamage;
            IsCritical = isCritical;
            IsDodged = isDodged;
            IsCancelled = isCancelled;
            AttackId = attackId;
            Modifiers = modifiers;
        }
    }
}
