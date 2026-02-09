using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Configs.Turn;

namespace Project.Gameplay.Gameplay.Turn.Conditions
{
    public sealed class ConditionRegistry
    {
        private readonly Dictionary<string, ITurnCondition> _conditionsByType;
        private readonly Dictionary<string, ConditionPreset> _presets;

        public ConditionRegistry(IEnumerable<ITurnCondition> conditions)
        {
            _conditionsByType = conditions.ToDictionary(c => c.Type);
            _presets = new Dictionary<string, ConditionPreset>();
        }

        public void RegisterPreset(ConditionConfig config)
        {
            if (!_conditionsByType.TryGetValue(config.Type, out ITurnCondition? condition))
                throw new Exception($"Unknown condition type: {config.Type}");

            ConditionPreset preset = new(
                config.Id,
                condition,
                new ConditionParams(config.Params));

            _presets[config.Id] = preset;
        }

        public ConditionPreset GetPreset(string id)
        {
            if (_presets.TryGetValue(id, out ConditionPreset? preset))
                return preset;

            if (_conditionsByType.TryGetValue(id, out ITurnCondition? condition))
                return new ConditionPreset(id, condition, ConditionParams.Empty);

            throw new Exception($"Unknown condition: {id}");
        }

        public ITurnCondition GetConditionByType(string type)
        {
            if (_conditionsByType.TryGetValue(type, out ITurnCondition? condition))
                return condition;

            throw new Exception($"Unknown condition type: {type}");
        }

        public void ClearPresets()
        {
            _presets.Clear();
        }
    }
}
