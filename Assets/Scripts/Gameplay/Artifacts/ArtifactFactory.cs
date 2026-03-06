using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Logging;
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
        
        private readonly Dictionary<string, IArtifact> _cache = new();
        private ArtifactConfigRepository? _repository;
        
        [Inject]
        private ArtifactFactory(ConfigProvider configProvider, ILogService logService)
        {
            _configProvider = configProvider;
            _logger = logService.CreateLogger<ArtifactFactory>();
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

        private static IArtifact CreateFromConfig(ArtifactConfig config)
        {
            ArtifactEffectType effectType = config.Effect.ParseType();
            return effectType switch
            {
                ArtifactEffectType.StatBuff => new StatBuffArtifact(config),
                ArtifactEffectType.Heal => new HealArtifact(config),
                ArtifactEffectType.Shield => new ShieldArtifact(config),
                ArtifactEffectType.ReflectDamage => new ReflectDamageArtifact(config),
                ArtifactEffectType.Revive => new ReviveArtifact(config),
                ArtifactEffectType.ExtraChoice => new ExtraChoiceArtifact(config),
                ArtifactEffectType.AllStatsBuff => new AllStatsBuffArtifact(config),
                _ => new GenericArtifact(config)
            };
        }

        void IDisposable.Dispose()
        {
            _cache.Clear();
        }
    }
}
