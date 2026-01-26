using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Turn;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Steps;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPatternFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly TurnStepFactory _stepFactory;
        private readonly ConditionRegistry _conditionRegistry;
        private readonly ILogger<TurnPatternFactory> _logger;

        private TurnPatternConfigRepository _patternConfigs;
        private TurnPatternSetConfigRepository _setConfigs;
        private ConditionConfigRepository _conditionConfigs;
        private bool _initialized;

        public TurnPatternFactory(
            ConfigProvider configProvider,
            TurnStepFactory stepFactory,
            ConditionRegistry conditionRegistry,
            ILogService logService)
        {
            _configProvider = configProvider;
            _stepFactory = stepFactory;
            _conditionRegistry = conditionRegistry;
            _logger = logService.CreateLogger<TurnPatternFactory>();
        }

        public async UniTask InitializeAsync()
        {
            if (_initialized) return;

            _conditionConfigs = await _configProvider.Get<ConditionConfigRepository>("conditions_conf");
            _patternConfigs = await _configProvider.Get<TurnPatternConfigRepository>("turn_patterns_conf");
            _setConfigs = await _configProvider.Get<TurnPatternSetConfigRepository>("turn_pattern_sets_conf");

            foreach (var condConfig in _conditionConfigs.Conditions)
            {
                _conditionRegistry.RegisterPreset(condConfig);
            }

            _initialized = true;
            _logger.Info("TurnPatternFactory initialized");
        }

        public TurnPattern CreatePattern(string patternId)
        {
            TurnPatternConfig config = Array.Find(_patternConfigs.Patterns, p => p.Id == patternId);
            if (config == null)
                throw new Exception($"Unknown pattern: {patternId}");

            return CreatePatternFromConfig(config);
        }

        public TurnPatternSet CreatePatternSet(string setId)
        {
            TurnPatternSetConfig setConfig = Array.Find(_setConfigs.Sets, s => s.Id == setId);
            if (setConfig == null)
                throw new Exception($"Unknown pattern set: {setId}");

            var patterns = setConfig.PatternIds
                .Select(CreatePattern)
                .ToList();

            return new TurnPatternSet(setId, patterns);
        }

        private TurnPattern CreatePatternFromConfig(TurnPatternConfig config)
        {
            ConditionPreset preset = _conditionRegistry.GetPreset(config.ConditionId);

            ConditionParams finalParams = config.ConditionParams != null
                ? preset.DefaultParams.MergeWith(new ConditionParams(config.ConditionParams))
                : preset.DefaultParams;

            ITurnStep step = config.Steps.Length == 1
                ? _stepFactory.CreateStep(config.Steps[0], config.Id)
                : _stepFactory.CreateComposite(config.Id, config.Steps);

            return new TurnPattern(
                config.Id,
                config.Priority,
                preset.Condition,
                finalParams,
                step);
        }
    }
}
