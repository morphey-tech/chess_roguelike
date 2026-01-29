using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnTurnEnd
    {
        void OnTurnEnd(TurnContext context);
    }
}
