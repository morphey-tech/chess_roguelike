using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnBeforeHit
    {
        void OnBeforeHit(BeforeHitContext context);
    }
}
