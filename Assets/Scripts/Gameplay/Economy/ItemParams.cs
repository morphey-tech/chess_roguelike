using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Typed view over item config params. JSON keeps flexible Dictionary; code uses strong types.
    /// Map keys: heal_amount -> Heal, crit_chance -> CritChance, crit_multiplier -> CritMultiplier, etc.
    /// </summary>
    public sealed class ItemParams
    {
        public float Heal { get; }
        public float CritChance { get; }
        public float CritMultiplier { get; }
        public float Damage { get; }
        public float Duration { get; }
        public float Radius { get; }

        public ItemParams(
            float heal = 0f,
            float critChance = 0f,
            float critMultiplier = 0f,
            float damage = 0f,
            float duration = 0f,
            float radius = 0f)
        {
            Heal = heal;
            CritChance = critChance;
            CritMultiplier = critMultiplier;
            Damage = damage;
            Duration = duration;
            Radius = radius;
        }

        public static ItemParams FromDict(IReadOnlyDictionary<string, float>? dict)
        {
            if (dict == null || dict.Count == 0)
                return new ItemParams();

            return new ItemParams(
                heal: Get(dict, "heal_amount", "heal"),
                critChance: Get(dict, "crit_chance", "crit"),
                critMultiplier: Get(dict, "crit_multiplier", "crit_mult"),
                damage: Get(dict, "damage", "dmg"),
                duration: Get(dict, "duration", "dur"),
                radius: Get(dict, "radius", "rad"));
        }

        private static float Get(IReadOnlyDictionary<string, float> dict, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (dict.TryGetValue(key, out float v))
                    return v;
            }
            return 0f;
        }
    }
}
