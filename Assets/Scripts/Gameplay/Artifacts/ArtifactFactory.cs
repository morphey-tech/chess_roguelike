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
                "worn_crown" => new WornCrownArtifact(config),
                "pawns_guard" => new PawnsGuardArtifact(config),
                "mercenaries_pouch" => new MercenariesPouchArtifact(config),
                "gamblers_die" => new GamblersDieArtifact(config, _randomService),
                "ambush_charge" => new AmbushChargeArtifact(config),
                "grandmaster_blade" => new GrandmasterBladeArtifact(config),
                "swift_strider" => new SwiftStriderArtifact(config),
                _ => new GenericArtifact(config)
            };
        }

        void IDisposable.Dispose()
        {
            _cache.Clear();
        }
    }
}
