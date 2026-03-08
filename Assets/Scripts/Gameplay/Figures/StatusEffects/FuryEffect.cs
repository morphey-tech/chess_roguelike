using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Fury: gain attack damage stacks when attacking. Stacks persist until death.
    /// </summary>
    public class FuryEffect : StackableStatusEffect, IOnBeforeHit, IOnAfterHit
    {
        public override string Id => "fury";

        private readonly float _damagePerStack;

        public FuryEffect(float damage, int stacks, int maxStacks, int turns = -1, int uses = -1)
            : base(stacks, maxStacks, turns, uses)
        {
            _damagePerStack = damage;
        }

        public override bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit && context.Type != TriggerType.OnAfterHit)
            {
                return false;
            }

            if (context.Type == TriggerType.OnBeforeHit)
            {
                if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
                {
                    return false;
                }
                return context.Actor == beforeHit.Attacker;
            }

            if (context.Type == TriggerType.OnAfterHit)
            {
                if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
                {
                    return false;
                }
                return context.Actor == afterHit.Attacker;
            }

            return false;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            switch (context.Type)
            {
                case TriggerType.OnBeforeHit:
                    ExecuteBeforeHit(context);
                    break;
                case TriggerType.OnAfterHit:
                    ExecuteAfterHit(context);
                    break;
            }
            return TriggerResult.Continue;
        }

        private void ExecuteBeforeHit(TriggerContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return;
            }
            if (context.Actor != beforeHit.Attacker)
            {
                return;
            }

            // Add bonus damage directly to the hit context
            float totalBonus = _damagePerStack * Stacks;
            beforeHit.BonusDamage += totalBonus;
        }

        private void ExecuteAfterHit(TriggerContext context)
        {
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return;
            }
            if (context.Actor != afterHit.Attacker)
            {
                return;
            }

            // Add stack after successful hit
            AddStack();
        }
    }
}