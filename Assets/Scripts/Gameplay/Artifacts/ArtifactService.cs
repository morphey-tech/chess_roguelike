using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Artifacts.Effects;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.Artifacts
{
    public sealed class ArtifactService
    {
        private readonly List<ArtifactInstance> _artifacts = new();
        private readonly ArtifactFactory _factory;
        private readonly ILogger<ArtifactService> _logger;

        public IReadOnlyList<ArtifactInstance> Artifacts => _artifacts;
        public int Count => _artifacts.Count;

        [Inject]
        private ArtifactService(ArtifactFactory factory, ILogService logService)
        {
            _factory = factory;
            _logger = logService.CreateLogger<ArtifactService>();
        }

        public async UniTask<ArtifactInstance> Add(string configId)
        {
            IArtifact artifact = await _factory.Create(configId);
            ArtifactInstance instance = new(artifact, Guid.NewGuid().ToString());
            _artifacts.Add(instance);
            artifact.OnAcquired(new ArtifactContext { OwnerId = 0, ArtifactId = instance.Id });
            _logger.Info($"Artifact acquired: {artifact.ConfigId} ({artifact.GetType().Name})");
            // Try to open artifacts window, need remove from that class
            TryOpenArtifactsWindow();
            return instance;
        }

        private static void TryOpenArtifactsWindow()
        {
            try
            {
                if (UIService.IsValid && !UIService.IsVisible<ArtifactsWindow>())
                {
                    UIService.ShowAsync<ArtifactsWindow>().Forget();
                }
            }
            catch (Exception ex)
            {
                //To logger
                // UI service may not be available in all contexts
                UnityEngine.Debug.LogWarning($"Failed to open artifacts window: {ex.Message}");
            }
        }

        public bool Remove(string instanceId)
        {
            ArtifactInstance? instance = Find(instanceId);
            if (instance == null)
            {
                return false;
            }
            instance.Artifact.OnRemoved(new ArtifactContext { OwnerId = 0, ArtifactId = instance.Id });
            _artifacts.Remove(instance);
            _logger.Info($"Artifact removed: {instance.ConfigId}");
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

        public void Clear()
        {
            foreach (ArtifactInstance? instance in _artifacts)
            {
                instance.Artifact.OnRemoved(new ArtifactContext { OwnerId = 0, ArtifactId = instance.Id });
            }
            _artifacts.Clear();
        }
    }
}
