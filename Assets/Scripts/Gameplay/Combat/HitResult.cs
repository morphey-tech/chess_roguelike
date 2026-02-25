using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class HitResult
    {
        public Figure Target { get; }
        public GridPosition Position { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
        public bool IsBlocked { get; }
        public bool IsKilled { get; }

        public HitResult(
            Figure target,
            GridPosition position,
            int damage,
            bool isCritical,
            bool isBlocked,
            bool isKilled)
        {
            Target = target;
            Position = position;
            Damage = damage;
            IsCritical = isCritical;
            IsBlocked = isBlocked;
            IsKilled = isKilled;
        }
    }
}