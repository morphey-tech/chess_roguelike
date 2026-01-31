using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPattern
    {
        public string Id { get; }
        public List<TurnPatternDescription> Patterns { get; }

        public TurnPattern(string id, List<TurnPatternDescription> patterns)
        {
            Id = id;
            Patterns = patterns ?? new List<TurnPatternDescription>();
        }
    }
}
