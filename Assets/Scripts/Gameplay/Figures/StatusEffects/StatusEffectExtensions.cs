using System.Collections.Generic;
using System.Linq;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Extension methods for filtering status effects by category.
    /// </summary>
    public static class StatusEffectExtensions
    {
        /// <summary>
        /// Get all buffs (positive effects).
        /// </summary>
        public static IEnumerable<IStatusEffect> GetBuffs(this StatusEffectSystem effects)
        {
            return effects.GetEffects().Where(e => e.Category == EffectCategory.Buff);
        }

        /// <summary>
        /// Get all debuffs (negative effects).
        /// </summary>
        public static IEnumerable<IStatusEffect> GetDebuffs(this StatusEffectSystem effects)
        {
            return effects.GetEffects().Where(e => e.Category == EffectCategory.Debuff);
        }

        /// <summary>
        /// Get all crowd control effects.
        /// </summary>
        public static IEnumerable<IStatusEffect> GetCrowdControl(this StatusEffectSystem effects)
        {
            return effects.GetEffects().Where(e => e.Category == EffectCategory.CrowdControl);
        }

        /// <summary>
        /// Get all neutral effects.
        /// </summary>
        public static IEnumerable<IStatusEffect> GetNeutral(this StatusEffectSystem effects)
        {
            return effects.GetEffects().Where(e => e.Category == EffectCategory.Neutral);
        }

        /// <summary>
        /// Remove all buffs from the figure.
        /// </summary>
        public static void RemoveAllBuffs(this StatusEffectSystem effects)
        {
            foreach (var buff in effects.GetBuffs().ToList())
            {
                effects.Remove(buff.Id);
            }
        }

        /// <summary>
        /// Remove all debuffs from the figure.
        /// </summary>
        public static void RemoveAllDebuffs(this StatusEffectSystem effects)
        {
            foreach (var debuff in effects.GetDebuffs().ToList())
            {
                effects.Remove(debuff.Id);
            }
        }

        /// <summary>
        /// Remove all crowd control effects from the figure.
        /// </summary>
        public static void RemoveAllCrowdControl(this StatusEffectSystem effects)
        {
            foreach (var cc in effects.GetCrowdControl().ToList())
            {
                effects.Remove(cc.Id);
            }
        }

        /// <summary>
        /// Check if the figure has any crowd control effects.
        /// </summary>
        public static bool HasCrowdControl(this StatusEffectSystem effects)
        {
            return effects.GetEffects().Any(e => e.Category == EffectCategory.CrowdControl);
        }

        /// <summary>
        /// Check if the figure has any effects of the specified category.
        /// </summary>
        public static bool HasEffectOfType(this StatusEffectSystem effects, EffectCategory category)
        {
            return effects.GetEffects().Any(e => e.Category == category);
        }

        /// <summary>
        /// Count effects of the specified category.
        /// </summary>
        public static int CountByCategory(this StatusEffectSystem effects, EffectCategory category)
        {
            return effects.GetEffects().Count(e => e.Category == category);
        }
    }
}
