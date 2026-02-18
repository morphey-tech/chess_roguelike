using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Turn;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Steps;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPatternFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly TurnStepFactory _stepFactory;
        private readonly ConditionRegistry _conditionRegistry;
        private readonly ILogger<TurnPatternFactory> _logger;

        private TurnPatternDescriptionConfigRepository? _descriptionConfigs;
        private TurnPatternsConfigRepository? _patternsConfigs;
        private ConditionConfigRepository? _conditionConfigs;
        private bool _initialized;

        [Inject]
        private TurnPatternFactory(
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
            _descriptionConfigs = await _configProvider.Get<TurnPatternDescriptionConfigRepository>("turn_pattern_descriptions_conf");
            _patternsConfigs = await _configProvider.Get<TurnPatternsConfigRepository>("turn_patterns_conf");

            foreach (var condConfig in _conditionConfigs.All)
            {
                _conditionRegistry.RegisterPreset(condConfig);
            }

            _initialized = true;
            _logger.Info("TurnPatternFactory initialized");
        }

        public TurnPatternDescription CreatePattern(string patternId)
        {
            TurnPatternDescriptionConfig config = _descriptionConfigs!.Get(patternId);
            if (config == null)
                throw new Exception($"Unknown pattern description: {patternId}");

            return CreatePatternFromConfig(config);
        }

        public TurnPattern CreatePatternSet(string setId)
        {
            TurnPatternsConfig? config = _patternsConfigs!.Get(setId);
            if (config == null)
                throw new Exception($"Unknown turn patterns: {setId}");

            var patterns = config.PatternIds
                .Select(CreatePattern)
                .ToList();

            return new TurnPattern(setId, patterns);
        }

        public void ResetCache()
        {
            _initialized = false;
            _conditionConfigs = null;
            _descriptionConfigs = null;
            _patternsConfigs = null;
            _conditionRegistry.ClearPresets();
            _logger.Info("TurnPatternFactory cache cleared");
        }

        private TurnPatternDescription CreatePatternFromConfig(TurnPatternDescriptionConfig config)
        {
            ConditionPreset preset = _conditionRegistry.GetPreset(config.ConditionId);

            ConditionParams finalParams = config.ConditionParams != null
                ? preset.DefaultParams.MergeWith(new ConditionParams(config.ConditionParams))
                : preset.DefaultParams;

            ITurnStep step = config.Steps.Length == 1
                ? _stepFactory.CreateStep(config.Steps[0], config.Id)
                : _stepFactory.CreateComposite(config.Id, config.Steps);

            return new TurnPatternDescription(
                config.Id,
                config.Priority,
                preset.Condition,
                finalParams,
                step);
        }
    }
}
