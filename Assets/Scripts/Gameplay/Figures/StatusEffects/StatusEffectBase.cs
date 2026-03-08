using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class StatusEffectBase : IStatusEffect
    {
        public virtual string Id { get; }
        public virtual int Priority => TriggerPriorities.Normal;
        public virtual TriggerGroup Group => TriggerGroup.Default;
        public virtual TriggerPhase Phase => TriggerPhase.Default;
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

        public virtual bool Matches(TriggerContext context)
        {
            return true; // Default: match all contexts for status effects
        }

        public virtual TriggerResult Execute(TriggerContext context)
        {
            switch (context.Type)
            {
                case TriggerType.OnTurnStart:
                    if (context.Data is TurnContext turnStart)
                    {
                        OnTurnStartInternal(turnStart);
                    }
                    break;
                case TriggerType.OnTurnEnd:
                    if (context.Data is TurnContext turnEnd)
                    {
                        OnTurnEndInternal(turnEnd);
                    }
                    break;
                case TriggerType.OnBeforeHit:
                    if (context.Data is BeforeHitContext beforeHit)
                    {
                        OnBeforeHitInternal(beforeHit);
                    }
                    break;
                case TriggerType.OnAfterHit:
                    if (context.Data is AfterHitContext afterHit)
                    {
                        OnAfterHitInternal(afterHit);
                    }
                    break;
            }
            return TriggerResult.Continue;
        }

        private void OnTurnStartInternal(TurnContext ctx)
        {
            // Only decrement turns at start of owner's team turn
            if (OwnerTeam == ctx.Team && RemainingTurns > 0)
            {
                RemainingTurns--;
            }
            OnTurnStart(null!, ctx);
        }

        private void OnTurnEndInternal(TurnContext ctx)
        {
            OnTurnEnd(null!, ctx);
        }

        private void OnBeforeHitInternal(BeforeHitContext ctx)
        {
            OnBeforeHit(null!, ctx);
        }

        private void OnAfterHitInternal(AfterHitContext ctx)
        {
            OnAfterHit(null!, ctx);
        }

        public virtual void OnTurnStart(Figure owner, TurnContext ctx)
        {
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