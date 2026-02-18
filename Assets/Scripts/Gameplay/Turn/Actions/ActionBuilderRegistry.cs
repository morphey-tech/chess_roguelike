using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Gameplay.Gameplay.Turn.Actions
{
    /// <summary>
    /// Registry for action builders. Maps action type strings to IActionBuilder instances.
    /// </summary>
    public sealed class ActionBuilderRegistry
    {
        private readonly Dictionary<string, IActionBuilder> _builders;

        public ActionBuilderRegistry(IEnumerable<IActionBuilder> builders)
        {
            _builders = builders.ToDictionary(b => b.ActionType, b => b);
        }

        public void RegisterSequentialBuilder()
        {
            var sequentialBuilder = new Builders.SequentialActionBuilder(this);
            _builders[sequentialBuilder.ActionType] = sequentialBuilder;
        }

        public IActionBuilder GetBuilder(string actionType)
        {
            if (_builders.TryGetValue(actionType, out IActionBuilder? builder))
                return builder;

            throw new Exception($"Unknown action type: {actionType}");
        }

        public bool HasBuilder(string actionType)
        {
            return _builders.ContainsKey(actionType);
        }
    }
}
