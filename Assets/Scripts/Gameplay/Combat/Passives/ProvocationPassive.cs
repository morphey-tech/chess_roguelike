using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Provocation: enemies can only target this figure if it's attackable.
    /// Forces enemy targeting to prioritize provokers.
    /// </summary>
    public sealed class ProvocationPassive : IPassive
    {
        public string Id { get; }
        public int Priority => 0;

        public ProvocationPassive(string id)
        {
            Id = id;
        }
    }
}