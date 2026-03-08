using Project.Core.Core.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Provocation: enemies can only target this figure if it's attackable.
    /// Forces enemy targeting to prioritize provokers.
    ///
    /// This is a passive marker ability - no trigger execution needed.
    /// The targeting logic is handled in the AI/selection system.
    /// </summary>
    public sealed class ProvocationPassive : IPassive
    {
        public string Id => "provocation";
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.Default;

        public ProvocationPassive()
        {
        }

        public bool Matches(TriggerContext context)
        {
            return false; // No trigger behavior - this passive is queried directly by targeting system
        }

        public TriggerResult Execute(TriggerContext context)
        {
            // No trigger behavior - this passive is queried directly by targeting system
            return TriggerResult.Continue;
        }
    }
}