using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnTurnEnd
    {
        void OnTurnEnd(Figure figure, TurnContext context);
    }
}
