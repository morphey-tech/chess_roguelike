using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnTurnStart
    {
        void OnTurnStart(TurnContext context);
    }
}
