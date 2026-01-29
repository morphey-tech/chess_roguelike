using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPatternSet
    {
        public string Id { get; }
        public List<TurnPattern> Patterns { get; }

        public TurnPatternSet(string id, List<TurnPattern> patterns)
        {
            Id = id;
            Patterns = patterns ?? new List<TurnPattern>();
        }
    }
}
