using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

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
        protected Team? OwnerTeam;

        public StatusEffectBase(int turns = -1, int uses = -1)
        {
            RemainingTurns = turns;
            RemainingUses = uses;
        }

        public virtual void OnApply(Figure owner)
        {
            OwnerTeam = owner.Team;
        }

        public virtual void OnRemove(Figure owner)
        {
        }

        public virtual void OnTurnStart(Figure owner, TurnContext ctx)
        {
            // Only decrement turns at start of owner's team turn
            if (OwnerTeam == ctx.Team && RemainingTurns > 0)
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

        public virtual void OnAfterHit(Figure owner, AfterHitContext ctx)
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