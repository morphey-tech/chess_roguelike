using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class DeathContext
    {
        public Figure Victim { get; set; }
        public Figure Killer { get; set; }
    }
}
