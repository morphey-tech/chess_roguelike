using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnTurnStart
    {
        void OnTurnStart(Figure figure, TurnContext context);
    }
}
