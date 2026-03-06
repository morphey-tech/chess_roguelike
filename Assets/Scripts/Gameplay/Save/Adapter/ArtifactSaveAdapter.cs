using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Artifacts;
using Project.Gameplay.Gameplay.Save.Models;
using VContainer;

namespace Project.Gameplay.Gameplay.Save.Adapter
{
    /// <summary>
    /// Bridges ArtifactService state to/from SaveSnapshot.
    /// Saves artifact config IDs and restores them on load.
    /// </summary>
    public sealed class ArtifactSaveAdapter : ISaveDataProvider, ISaveDataApplier
    {
        private readonly ArtifactService _artifactService;

        [Inject]
        private ArtifactSaveAdapter(ArtifactService artifactService)
        {
            _artifactService = artifactService;
        }

        public void Populate(SaveSnapshot snapshot)
        {
            // Artifacts (run-scoped)
            var artifacts = _artifactService.Artifacts;
            if (artifacts.Count > 0)
            {
                List<string> artifactIds = new(artifacts.Count);
                foreach (var instance in artifacts)
                {
                    artifactIds.Add(instance.ConfigId);
                }
                snapshot.Artifacts = artifactIds;
            }
            else
            {
                snapshot.Artifacts = null;
            }
        }

        public void Apply(SaveSnapshot snapshot)
        {
            // Clear existing artifacts
            _artifactService.Clear();

            // Restore artifacts from config IDs
            if (snapshot.Artifacts != null)
            {
                foreach (string configId in snapshot.Artifacts)
                {
                    try
                    {
                        //TODO: careful
                        _artifactService.Add(configId).Forget();
                    }
                    catch
                    {
                        // Skip artifacts whose config no longer exists
                    }
                }
            }
        }
    }
}
