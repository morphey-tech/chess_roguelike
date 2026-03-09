using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Project.Core.Core.Logging;
using VContainer;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Registry for artifact synergies.
    /// </summary>
    [UsedImplicitly]
    public class ArtifactSynergyRegistry : IDisposable
    {
        private readonly Dictionary<string, ArtifactSynergyConfig> _synergies = new();
        private readonly ILogger<ArtifactSynergyRegistry> _logger;

        [Inject]
        private ArtifactSynergyRegistry(ILogService logService)
        {
            _logger = logService.CreateLogger<ArtifactSynergyRegistry>();
        }

        public void Register(ArtifactSynergyConfig config)
        {
            _synergies[config.Id] = config;
            _logger.Info($"Registered synergy: {config.Id}");
        }

        public bool CheckSynergy(ArtifactService service, string synergyId)
        {
            if (!_synergies.TryGetValue(synergyId, out var config))
                return false;

            var equipped = service.GetEquipped();
            var matchCount = equipped.Count(a => config.RequiredArtifactIds.Contains(a.ConfigId));

            return matchCount >= config.RequiredCount;
        }

        public IEnumerable<string> GetActiveSynergies(ArtifactService service)
        {
            foreach (var kvp in _synergies)
            {
                if (CheckSynergy(service, kvp.Key))
                    yield return kvp.Key;
            }
        }

        public ArtifactSynergyConfig? Get(string synergyId)
        {
            _synergies.TryGetValue(synergyId, out var config);
            return config;
        }

        void IDisposable.Dispose()
        {
            _synergies.Clear();
        }
    }
}
