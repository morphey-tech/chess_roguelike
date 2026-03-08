using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Escape: When taking non-lethal damage, can move within 1 cell after being hit.
    /// Sets a flag to allow bonus movement.
    /// </summary>
    public class EscapePassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        public EscapePassive(string id)
        {
            Id = id;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnAfterHit)
            {
                return false;
            }
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return false;
            }
            return context.Target == afterHit.Target && !afterHit.TargetDied;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return TriggerResult.Continue;
            }

            afterHit.Target.MovedThisTurn = false;

            return TriggerResult.Continue;
        }
    }
}
