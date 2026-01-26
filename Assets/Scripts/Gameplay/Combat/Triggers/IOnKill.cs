using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnKill
    {
        void OnKill(KillContext context);
    }
}
