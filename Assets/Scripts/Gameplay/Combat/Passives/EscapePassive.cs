using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Escape: When taking non-lethal damage, can move within 1 cell after being hit.
    /// Sets a flag to allow bonus movement.
    /// </summary>
    public class EscapePassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 100;

        public EscapePassive(string id)
        {
            Id = id;
        }

        void IOnAfterHit.OnAfterHit(Figure owner, AfterHitContext context)
        {
            // Only trigger if owner was the target and didn't die
            if (owner != context.Target || context.TargetDied)
            {
                return;
            }

            // Grant bonus move within 1 cell
            // This is handled by setting a flag that BonusMoveSession checks
            owner.MovedThisTurn = false; // Allow movement this turn
        }
    }
}
