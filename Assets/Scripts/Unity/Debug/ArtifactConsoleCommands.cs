using System;
using System.Text;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Artifacts;
using Project.Gameplay.Gameplay.Configs;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Debug
{
    /// <summary>
    /// Console commands for debugging artifact system.
    /// Available in development builds only.
    /// </summary>
    public sealed class ArtifactConsoleCommands : IStartable, IDisposable
    {
        private readonly ArtifactService _artifactService;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger _logger;
        private bool _registered;

        [Inject]
        private ArtifactConsoleCommands(
            ArtifactService artifactService,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _artifactService = artifactService;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<ArtifactConsoleCommands>();
        }

        void IStartable.Start()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif

            DebugLogConsole.AddCommand(
                "list_artifacts",
                "List all available artifacts",
                ListArtifacts);

            DebugLogConsole.AddCommand(
                "list_owned",
                "List owned artifacts",
                ListOwned);

            DebugLogConsole.AddCommand<string>(
                "add_artifact",
                "Add artifact: add_artifact <configId>",
                v => AddArtifact(v).Forget());

            DebugLogConsole.AddCommand<string>(
                "remove_artifact",
                "Remove artifact: remove_artifact <configId>",
                RemoveArtifact);

            DebugLogConsole.AddCommand(
                "clear_artifacts",
                "Remove all artifacts",
                ClearArtifacts);

            _registered = true;
            _logger.Info("Artifact debug commands registered");
        }

        private void ListArtifacts()
        {
            try
            {
                var repository = _configProvider.GetSync<ArtifactConfigRepository>("artifacts_conf");
                var sb = new StringBuilder();
                sb.AppendLine($"Available artifacts ({repository.Content.Length}):");

                foreach (var config in repository.Content)
                {
                    sb.AppendLine($"  [{config.ParseRarity()}] {config.Id} - {config.Name}");
                    sb.AppendLine($"      Trigger: {config.ParseTrigger()} | {config.Description}");
                }

                _logger.Info(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load artifacts config: {ex.Message}");
            }
        }

        private void ListOwned()
        {
            var artifacts = _artifactService.Artifacts;
            if (artifacts.Count == 0)
            {
                _logger.Info("No artifacts owned");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Owned artifacts ({artifacts.Count}):");

            foreach (var instance in artifacts)
            {
                sb.AppendLine($"  [{instance.Id}] {instance.ConfigId}");
            }

            _logger.Info(sb.ToString());
        }

        private async UniTaskVoid AddArtifact(string configId)
        {
            try
            {
                var repository = _configProvider.GetSync<ArtifactConfigRepository>("artifacts_conf");
                var config = repository.Get(configId);
                if (config == null)
                {
                    _logger.Error($"Artifact '{configId}' not found in config");
                    return;
                }
                await _artifactService.Add(configId);
                _logger.Info($"Added artifact: {configId} ({config.Name})");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to add artifact: {ex.Message}");
            }
        }

        private void RemoveArtifact(string configId)
        {
            if (_artifactService.RemoveByConfigId(configId))
            {
                _logger.Info($"Removed artifact: {configId}");
            }
            else
            {
                _logger.Warning($"Artifact '{configId}' not found in owned artifacts");
            }
        }

        private void ClearArtifacts()
        {
            _artifactService.Clear();
            _logger.Info("All artifacts cleared");
        }

        void IDisposable.Dispose()
        {
            if (!_registered) return;

            DebugLogConsole.RemoveCommand("list_artifacts");
            DebugLogConsole.RemoveCommand("list_owned");
            DebugLogConsole.RemoveCommand("add_artifact");
            DebugLogConsole.RemoveCommand("remove_artifact");
            DebugLogConsole.RemoveCommand("clear_artifacts");
            _registered = false;
        }
    }
}
