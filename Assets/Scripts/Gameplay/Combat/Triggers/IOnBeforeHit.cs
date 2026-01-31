using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Triggers
{
    public interface IOnBeforeHit
    {
        /// <param name="owner">The figure that has this passive</param>
        /// <param name="context">Combat context</param>
        void OnBeforeHit(Figure owner, BeforeHitContext context);
    }
}
