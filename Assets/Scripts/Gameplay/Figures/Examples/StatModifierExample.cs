using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Examples
{
    /// <summary>
    /// Example usage of the new stat modifier system.
    /// </summary>
    public static class StatModifierExample
    {
        public static void DemonstrateUsage()
        {
            // Create a stat with base value
            FigureStat<float> attackStat = new(10f);

            // Add flat modifier (stackable) - item context
            FlatModifier<float> flatBuff = new("sword_bonus", 5f, 0, -1, true, ModifierSourceContext.Item);
            attackStat.AddModifier(flatBuff);

            // Add percentage modifier (not stackable) - combat effect context
            PercentModifier rageBuff = new("rage", 50f, 100, 3, false, ModifierSourceContext.CombatEffect);
            attackStat.AddModifier(rageBuff);

            // Value is calculated on-the-fly: (10 + 5) * 1.5 = 22.5
            float currentValue = attackStat.Value;

            // Try to add another rage buff (will replace the old one)
            PercentModifier newRageBuff = new("rage", 25f, 100, 5, false, ModifierSourceContext.CombatEffect);
            attackStat.AddModifier(newRageBuff);

            FlatModifier<float> strengthPotion1 = new("strength_potion", 2f, 0, 10, true, ModifierSourceContext.Item);
            FlatModifier<float> strengthPotion2 = new("strength_potion", 2f, 0, 10, true, ModifierSourceContext.Item);
            attackStat.AddModifier(strengthPotion1);
            attackStat.AddModifier(strengthPotion2);

            // Value: (10 + 5 + 2 + 2) * 1.25 = 23.75
            currentValue = attackStat.Value;

            // Tick to reduce duration
            attackStat.Tick();

            // Remove specific modifier
            attackStat.RemoveModifier(flatBuff);

            // Remove all modifiers by ID
            attackStat.RemoveModifiersById("strength_potion");

            // Clear all combat effect modifiers
            attackStat.ClearByContext(ModifierSourceContext.CombatEffect);
        }
    }
}
