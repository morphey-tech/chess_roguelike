using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class TurnContext
    {
        public Figure Figure { get; set; }
        public int TurnNumber { get; set; }
    }
}
