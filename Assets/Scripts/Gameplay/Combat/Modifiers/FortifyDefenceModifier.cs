using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Modifier;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Adds flat defence only when the owner has not moved this turn (for Fortify passive).
    /// </summary>
    public sealed class FortifyDefenceModifier : IStatModifier<float>
    {
        public int Priority { get; }
        private readonly Figure _owner;
        private readonly int _bonus;

        public FortifyDefenceModifier(Figure owner, int bonus, int priority = 100)
        {
            _owner = owner;
            _bonus = bonus;
            Priority = priority;
        }

        public float Apply(float value) => value + (_owner.MovedThisTurn ? 0 : _bonus);
    }
}
