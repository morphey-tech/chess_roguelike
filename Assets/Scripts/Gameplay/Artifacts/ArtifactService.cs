using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Configs.Gameplay;
using Project.Core.Core.Logging;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Artifacts.Effects;
using Project.Gameplay.Gameplay.Artifacts.Messages;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Service for managing player's artifacts during a run.
    /// Registers artifact triggers automatically.
    /// </summary>
    public sealed class ArtifactService : IDisposable
    {
        public int ArtifactCount => _artifacts.Count;
        public int TotalStackCount => _artifacts.Sum(a => a.Stack);
        public IReadOnlyList<ArtifactInstance> Artifacts => _artifacts;
        public int Count => _artifacts.Count;

        private readonly List<ArtifactInstance> _artifacts = new();
        private readonly ArtifactFactory _factory;
        private readonly TriggerService _triggerService;
        private readonly IPublisher<string, ArtifactMessage> _artifactPublisher;
        private readonly IPublisher<ArtifactChangedMessage> _changedPublisher;
        private readonly ArtifactSynergyRegistry _synergyRegistry;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<ArtifactService> _logger;

        private int _maxArtifacts = 8;  // Default

        [Inject]
        private ArtifactService(
            ArtifactFactory factory,
            TriggerService triggerService,
            IPublisher<string, ArtifactMessage> artifactPublisher,
            IPublisher<ArtifactChangedMessage> changedPublisher,
            ArtifactSynergyRegistry synergyRegistry,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _factory = factory;
            _triggerService = triggerService;
            _artifactPublisher = artifactPublisher;
            _changedPublisher = changedPublisher;
            _synergyRegistry = synergyRegistry;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<ArtifactService>();

            // Load max artifacts from config
            LoadMaxArtifacts().Forget();
        }

        private async UniTask LoadMaxArtifacts()
        {
            try
            {
                GameplayConfig? gameplayConfig = await _configProvider.Get<GameplayConfig>("gameplay_conf");
                _maxArtifacts = gameplayConfig.GetMaxArtifacts();
                _logger.Debug($"Max artifacts limit: {_maxArtifacts}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to load artifact limit from config, using default: {ex.Message}");
            }
        }

        /// <summary>
        /// Try to add an artifact. Returns null if limit reached.
        /// </summary>
        public async UniTask<ArtifactInstance?> TryAdd(string configId, int stackCount = 1)
        {
            if (_artifacts.Count >= _maxArtifacts)
            {
                _logger.Warning($"Cannot add artifact {configId}: limit reached ({_artifacts.Count}/{_maxArtifacts})");
                return null;
            }

            return await Add(configId, stackCount);
        }

        /// <summary>
        /// Add an artifact. Stacks if same artifact exists.
        /// Registers artifact triggers automatically.
        /// </summary>
        public async UniTask<ArtifactInstance> Add(string configId, int stackCount = 1)
        {
            IArtifact artifact = await _factory.Create(configId);
            ArtifactConfig? config = await GetConfig(configId);
            int maxStack = config?.GetMaxStack() ?? 1;  // Default max stack: 1

            // Check if we can stack with existing artifact
            ArtifactInstance? existing = _artifacts.FirstOrDefault(a => a.ConfigId == configId);
            if (existing != null)
            {
                if (maxStack != -1 && existing.Stack + stackCount > maxStack)
                {
                    _logger.Warning($"Cannot stack {configId}: would exceed max stack ({existing.Stack}+{stackCount}>{maxStack})");
                    return existing;
                }

                existing.Stack += stackCount;
                _logger.Info($"Artifact stacked: {configId} x{existing.Stack}");
                _artifactPublisher.Publish(ArtifactMessage.ADDED, ArtifactMessage.Added(existing));
                return existing;
            }

            // Create new instance
            ArtifactInstance instance = new(artifact, Guid.NewGuid().ToString())
            {
                Stack = stackCount
            };

            _artifacts.Add(instance);

            // Register trigger
            _triggerService.Register(artifact);

            artifact.OnAcquired(new ArtifactContext { OwnerId = 0, ArtifactId = instance.Id });
            _logger.Info($"Artifact acquired: {artifact.ConfigId} x{stackCount} ({artifact.GetType().Name})");

            _artifactPublisher.Publish(ArtifactMessage.ADDED, ArtifactMessage.Added(instance));
            _changedPublisher.Publish(new ArtifactChangedMessage(0, instance.Id, ArtifactChangeType.Acquired));
            
            // Check synergies
            CheckSynergies();
            
            return instance;
        }

        private async UniTask<ArtifactConfig?> GetConfig(string configId)
        {
            try
            {
                ArtifactConfigRepository? repository = await _configProvider.Get<ArtifactConfigRepository>("artifacts_conf");
                return repository.Get(configId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load artifact config {configId}: {ex.Message}");
                return null;
            }
        }

        public bool Remove(string instanceId)
        {
            ArtifactInstance? instance = Find(instanceId);
            if (instance == null)
            {
                return false;
            }

            // Unregister trigger
            _triggerService.Unregister(instance.Artifact);

            instance.Artifact.OnRemoved(new ArtifactContext { OwnerId = 0, ArtifactId = instance.Id });
            _artifacts.Remove(instance);
            _logger.Info($"Artifact removed: {instance.ConfigId}");

            _artifactPublisher.Publish(ArtifactMessage.REMOVED, ArtifactMessage.Removed(instance.ConfigId));
            _changedPublisher.Publish(new ArtifactChangedMessage(0, instanceId, ArtifactChangeType.Removed));
            
            // Check synergies
            CheckSynergies();

            return true;
        }

        public bool RemoveByConfigId(string configId)
        {
            ArtifactInstance? instance = _artifacts.FirstOrDefault(a => a.ConfigId == configId);
            return instance != null && Remove(instance.Id);
        }

        public ArtifactInstance? Find(string instanceId)
        {
            return _artifacts.FirstOrDefault(a => a.Id == instanceId);
        }

        public bool Has(string configId)
        {
            return _artifacts.Any(a => a.ConfigId == configId);
        }

        public IEnumerable<ArtifactInstance> GetEquipped() => _artifacts;

        private void CheckSynergies()
        {
            var activeSynergies = _synergyRegistry.GetActiveSynergies(this);
            foreach (var synergyId in activeSynergies)
            {
                _logger.Info($"Synergy activated: {synergyId}");
                // Apply synergy effect (e.g., register passive, apply buff, etc.)
                ApplySynergyEffect(synergyId);
            }
        }

        private void ApplySynergyEffect(string synergyId)
        {
            // TODO: Implement synergy effect application
            // This could register a passive, apply a buff, etc.
        }

        /// <summary>
        /// Count artifacts with a specific tag.
        /// </summary>
        public int CountByTag(ArtifactTag tag)
        {
            int count = 0;
            foreach (var instance in _artifacts)
            {
                if (instance.Artifact.Tags.HasTag(tag))
                {
                    count += instance.Stack;
                }
            }
            return count;
        }

        /// <summary>
        /// Get total stack count for all artifacts with a tag.
        /// </summary>
        public int GetTagStackCount(ArtifactTag tag)
        {
            return _artifacts
                .Where(a => a.Artifact.Tags.HasTag(tag))
                .Sum(a => a.Stack);
        }

        /// <summary>
        /// Check if player has any artifact with a specific tag.
        /// </summary>
        public bool HasTag(ArtifactTag tag)
        {
            return _artifacts.Any(a => a.Artifact.Tags.HasTag(tag));
        }

        public void Clear()
        {
            foreach (ArtifactInstance? instance in _artifacts)
            {
                _triggerService.Unregister(instance.Artifact);
                instance.Artifact.OnRemoved(new ArtifactContext { OwnerId = 0, ArtifactId = instance.Id });
            }
            _artifacts.Clear();
            _artifactPublisher.Publish(ArtifactMessage.CLEARED, ArtifactMessage.Cleared());
            _logger.Info("All artifacts cleared");
        }

        public void Dispose()
        {
            Clear();
            _triggerService.Dispose();
        }
    }
}
