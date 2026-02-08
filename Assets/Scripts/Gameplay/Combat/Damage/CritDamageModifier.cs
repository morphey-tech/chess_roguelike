using System;

namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class CritDamageModifier : IDamageModifier
    {
        public int Order => 10;

        private const float CritMultiplier = 1.5f;

        public int Modify(DamageContext context, int value)
        {
            if (!context.IsCritical)
                return value;

            return (int)Math.Round(value * CritMultiplier);
        }
    }
}
