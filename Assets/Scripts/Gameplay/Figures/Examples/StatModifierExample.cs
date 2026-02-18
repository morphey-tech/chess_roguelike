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
            var attackStat = new FigureStat<float>(10f);
            
            // Add flat modifier (stackable)
            var flatBuff = new FlatModifier<float>("sword_bonus", 5f, 0, -1, true);
            attackStat.AddModifier(flatBuff);
            
            // Add percentage modifier (not stackable)
            var rageBuff = new PercentModifier("rage", 50f, 100, 3, false);
            attackStat.AddModifier(rageBuff);
            
            // Value is calculated on-the-fly: (10 + 5) * 1.5 = 22.5
            float currentValue = attackStat.Value;
            
            // Try to add another rage buff (will replace the old one)
            var newRageBuff = new PercentModifier("rage", 25f, 100, 5, false);
            attackStat.AddModifier(newRageBuff);
            
            // Add multiple stackable flat buffs
            var strengthPotion1 = new FlatModifier<float>("strength_potion", 2f, 0, 10, true);
            var strengthPotion2 = new FlatModifier<float>("strength_potion", 2f, 0, 10, true);
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
        }
    }
}
