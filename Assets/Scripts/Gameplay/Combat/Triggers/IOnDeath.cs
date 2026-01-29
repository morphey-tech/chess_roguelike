using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnDeath
    {
        void OnDeath(DeathContext context);
    }
}
