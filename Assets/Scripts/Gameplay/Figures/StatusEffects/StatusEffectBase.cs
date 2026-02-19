using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class StatusEffectBase : IStatusEffect
    {
        public virtual string Id { get; }
        public virtual int Priority => 0;
        public bool IsExpired => (RemainingTurns == 0 && RemainingTurns != -1)
                                 || (RemainingUses == 0 && RemainingUses != -1);

        protected int RemainingTurns;
        protected int RemainingUses;

        public StatusEffectBase(int turns = -1, int uses = -1)
        {
            RemainingTurns = turns;
            RemainingUses = uses;
        }
        
        public virtual void OnApply(Figure owner)
        {
        }

        public virtual void OnRemove(Figure owner)
        {
        }

        public virtual void OnTurnStart(Figure owner, TurnContext ctx)
        {
            if (RemainingTurns > 0)
            {
                RemainingTurns--;
            }
        }

        public virtual void OnTurnEnd(Figure owner, TurnContext ctx)
        {
        }

        public virtual void OnBeforeHit(Figure owner, BeforeHitContext ctx)
        {
        }

        protected bool TryConsumeUse()
        {
            if (RemainingUses == 0)
            {
                return false;
            }
            if (RemainingUses > 0)
            {
                RemainingUses--;
            }
            return true;
        }
    }
}