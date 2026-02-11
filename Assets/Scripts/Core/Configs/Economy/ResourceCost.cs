using System.Collections.Generic;

namespace Project.Core.Core.Configs.Economy
{
    /// <summary>
    /// Multi-resource cost for shops, exchanges, quests, rewards.
    /// </summary>
    public sealed class ResourceCost
    {
        public IReadOnlyDictionary<string, int> Costs { get; }

        public ResourceCost(IReadOnlyDictionary<string, int> costs)
        {
            Costs = costs ?? new Dictionary<string, int>();
        }

        public static ResourceCost Single(string id, int amount)
        {
            return new ResourceCost(new Dictionary<string, int> { { id, amount } });
        }

        public static ResourceCost Empty => new(new Dictionary<string, int>());
    }
}
