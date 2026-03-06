using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Logging;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Artifacts.Effects;
using Project.Gameplay.Gameplay.Configs;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Artifacts
{
    public sealed class ArtifactFactory : IDisposable
    {
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<ArtifactFactory> _logger;
        private readonly IRandomService _randomService;

        private readonly Dictionary<string, IArtifact> _cache = new();
        private ArtifactConfigRepository? _repository;

        [Inject]
        private ArtifactFactory(ConfigProvider configProvider, ILogService logService, IRandomService randomService)
        {
            _configProvider = configProvider;
            _logger = logService.CreateLogger<ArtifactFactory>();
            _randomService = randomService;
        }
        
        public async UniTask<IArtifact> Create(string configId)
        {
            if (_cache.TryGetValue(configId, out IArtifact? cached))
            {
                return cached;
            }
            if (_repository == null)
            {
                _repository = await _configProvider.Get<ArtifactConfigRepository>("artifacts_conf");
            }

            ArtifactConfig? config = _repository.Get(configId);
            if (config == null)
            {
                throw new ArgumentException($"Artifact config '{configId}' not found");
            }

            IArtifact artifact = CreateFromConfig(config);
            _cache[configId] = artifact;
            _logger.Debug($"Created artifact: {configId} ({config.Name})");
            return artifact;
        }

        private IArtifact CreateFromConfig(ArtifactConfig config)
        {
            return config.Id switch
            {
                "old_crown" => new OldCrownArtifact(config),
                "beginner_shield" => new BeginnerShieldArtifact(config),
                "mercenary_pouch" => new MercenaryPouchArtifact(config),
                "dice" => new DiceArtifact(config, _randomService),
                "trap_mine" => new TrapMineArtifact(config),
                "grandmaster_sword" => new GrandmasterSwordArtifact(config),
                "wind_boots" => new WindBootsArtifact(config),
                _ => new GenericArtifact(config)
            };
        }

        void IDisposable.Dispose()
        {
            _cache.Clear();
        }
    }
}
