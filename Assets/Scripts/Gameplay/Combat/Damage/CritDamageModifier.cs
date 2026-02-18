using System;

namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class CritDamageModifier : IDamageModifier
    {
        public int Order => 10;

        private const float CritMultiplier = 1.5f;

        public float Modify(DamageContext context, float value)
        {
            if (!context.IsCritical)
                return value;

            return value * CritMultiplier;
        }
    }
}
