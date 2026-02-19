using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public interface IStatusEffect
    {
        string Id { get; }
        int Priority { get; }
        
        bool IsExpired { get; }
        
        void OnApply(Figure owner);
        void OnRemove(Figure owner);
        
        void OnTurnStart(Figure owner, TurnContext ctx);
        void OnTurnEnd(Figure owner, TurnContext ctx);
        void OnBeforeHit(Figure owner, BeforeHitContext ctx);
    }
}