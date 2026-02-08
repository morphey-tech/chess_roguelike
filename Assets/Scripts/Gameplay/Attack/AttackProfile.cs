using Project.Core.Core.Configs.Stats;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackProfile
    {
        public AttackType Type { get; }
        public int Damage { get; }
        public int Range { get; }
        public TargetingType Targeting { get; }

        public AttackProfile(AttackType type, int damage, int range, TargetingType targeting)
        {
            Type = type;
            Damage = damage;
            Range = range;
            Targeting = targeting;
        }

        public bool CanHit(int distance) => distance <= Range;
    }
}
