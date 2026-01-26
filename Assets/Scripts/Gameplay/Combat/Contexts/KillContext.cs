using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class KillContext
    {
        public Figure Killer { get; set; }
        public Figure Victim { get; set; }
    }
}
