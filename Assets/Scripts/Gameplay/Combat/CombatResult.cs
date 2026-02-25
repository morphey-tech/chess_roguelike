using System.Collections.Generic;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class CombatResult
    {
        public Figure Attacker { get; }
        public GridPosition From { get; }
        public DeliveryType Delivery { get; }
        public IReadOnlyList<HitResult> Hits { get; }

        /// <summary>
        /// All combat effects to be applied. Sorted by priority.
        /// </summary>
        public IReadOnlyList<ICombatEffect> Effects { get; }
        
        // Core info still available for queries (but effects handle the execution)
        public int DamageDealt { get; }
        public bool TargetDied { get; }
        public bool WasCritical { get; }

        public CombatResult(
            Figure attacker,
            GridPosition from,
            DeliveryType delivery,
            IReadOnlyList<HitResult> hits,
            IReadOnlyList<ICombatEffect> effects,
            int damageDealt,
            bool targetDied,
            bool wasCritical)
        {
            Attacker = attacker;
            From = from;
            Delivery = delivery;
            Hits = hits;
            Effects = effects;
            DamageDealt = damageDealt;
            TargetDied = targetDied;
            WasCritical = wasCritical;
        }
    }
}
