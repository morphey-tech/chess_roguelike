using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Configs.Turn;
using Project.Gameplay.Gameplay.Turn.Conditions.Impl;

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
            if (!_conditionsByType.TryGetValue(config.Type, out var condition))
                throw new Exception($"Unknown condition type: {config.Type}");

            var preset = new ConditionPreset(
                config.Id,
                condition,
                new ConditionParams(config.Params));

            _presets[config.Id] = preset;
        }

        public ConditionPreset GetPreset(string id)
        {
            if (_presets.TryGetValue(id, out var preset))
                return preset;

            if (_conditionsByType.TryGetValue(id, out var condition))
                return new ConditionPreset(id, condition, ConditionParams.Empty);

            throw new Exception($"Unknown condition: {id}");
        }

        public ITurnCondition GetConditionByType(string type)
        {
            if (_conditionsByType.TryGetValue(type, out var condition))
                return condition;

            throw new Exception($"Unknown condition type: {type}");
        }
    }

    public sealed class ConditionPreset
    {
        public string Id { get; }
        public ITurnCondition Condition { get; }
        public ConditionParams DefaultParams { get; }

        public ConditionPreset(string id, ITurnCondition condition, ConditionParams defaultParams)
        {
            Id = id;
            Condition = condition;
            DefaultParams = defaultParams;
        }
    }
}
