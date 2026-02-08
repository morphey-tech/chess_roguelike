using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Configs.Stats;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat
{
    public class HitContext : ICombatEffectSink
    {
        public Figure Attacker { get; set; }
        public Figure Target { get; set; }
        public GridPosition AttackerPosition { get; set; }
        public GridPosition TargetPosition { get; set; }
        public BoardGrid Grid { get; set; }
        public string AttackId { get; set; }
        public int BaseDamage { get; set; }
        public HitType HitType { get; set; }
        public bool AttackerMovesOnKill { get; set; }
        public DeliveryType Delivery { get; set; }
        public HitPattern Pattern { get; set; }
        public string ProjectileConfigId { get; set; }
        
        /// <summary>
        /// Effects added by attack strategy (e.g., splash damage).
        /// </summary>
        public List<ICombatEffect> Effects { get; } = new();
        
        public void AddEffect(ICombatEffect effect) => Effects.Add(effect);
    }
}
