using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnAfterHit
    {
        void OnAfterHit(AfterHitContext context);
    }
}
