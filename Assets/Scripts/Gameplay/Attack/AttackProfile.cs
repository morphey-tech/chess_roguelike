using Project.Core.Core.Configs.Stats;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackProfile
    {
        public AttackType Type { get; }
        public int Damage { get; }
        public int Range { get; }
        public TargetingType Targeting { get; }
        public DeliveryType Delivery { get; }
        public HitPattern Pattern { get; }
        public string ProjectileConfigId { get; }

        public AttackProfile(
            AttackType type,
            int damage,
            int range,
            TargetingType targeting,
            DeliveryType delivery,
            HitPattern pattern,
            string projectileConfigId = null)
        {
            Type = type;
            Damage = damage;
            Range = range;
            Targeting = targeting;
            Delivery = delivery;
            Pattern = pattern;
            ProjectileConfigId = projectileConfigId;
        }

        public bool CanHit(int distance) => distance <= Range;
    }
}
