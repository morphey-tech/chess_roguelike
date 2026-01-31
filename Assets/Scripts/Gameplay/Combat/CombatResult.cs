using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class CombatResult
    {
        /// <summary>
        /// All combat effects to be applied. Sorted by priority.
        /// </summary>
        public IReadOnlyList<ICombatEffect> Effects { get; }
        
        // Core info still available for queries (but effects handle the execution)
        public int DamageDealt { get; }
        public bool TargetDied { get; }
        public bool WasCritical { get; }

        public CombatResult(
            IReadOnlyList<ICombatEffect> effects, 
            int damageDealt, 
            bool targetDied, 
            bool wasCritical)
        {
            Effects = effects;
            DamageDealt = damageDealt;
            TargetDied = targetDied;
            WasCritical = wasCritical;
        }
    }

    public struct AdditionalTargetResult
    {
        public Figure Target { get; set; }
        public int DamageDealt { get; set; }
        public bool Died { get; set; }
    }
}
