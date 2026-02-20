using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Royal Presence buff: +damage to attack stat.
    /// </summary>
    public class RoyalPresenceBuffEffect : StatusEffectBase
    {
        public override string Id => "royal_presence";

        private readonly string _modifierId;
        private readonly float _damageBonus;

        public RoyalPresenceBuffEffect(string sourceId, float damageBonus, int turns = -1, int uses = -1)
            : base(turns, uses)
        {
            _damageBonus = damageBonus;
            _modifierId = $"royal_presence_{sourceId}";
        }

        public override void OnApply(Figure owner)
        {
            CombatFlatModifier modifier = new(_modifierId, _damageBonus, 0, -1, false);
            owner.Stats.Attack.AddModifier(modifier);
        }

        public override void OnRemove(Figure owner)
        {
            owner.Stats.Attack.RemoveModifiersById(_modifierId);
        }
    }
}
