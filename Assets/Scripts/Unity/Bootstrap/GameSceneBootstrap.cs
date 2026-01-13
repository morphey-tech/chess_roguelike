using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Run;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Run;

namespace Project.Unity.Unity.Bootstrap
{
    /// <summary>
    /// Бутстрап игровой сцены.
    /// Запускает Run из конфига.
    /// </summary>
    public class GameSceneBootstrap : MonoSceneBootstrap
    {
        private ConfigProvider _configProvider;
        private RunFactory _runFactory;
        private RunHolder _runHolder;

        private const string DefaultRunId = "0";

        protected override void OnConstruct()
        {
            _configProvider = Resolve<ConfigProvider>();
            _runFactory = Resolve<RunFactory>();
            _runHolder = Resolve<RunHolder>();
        }

        protected override async UniTask OnBootstrapAsync()
        {
            Log.Info("Game bootstrap started");

            RunConfig runConfig = await LoadRunConfigAsync(DefaultRunId);
            Run run = _runFactory.Create(runConfig);
            
            // Сохраняем в holder чтобы другие сервисы могли получить доступ
            _runHolder.Set(run);
            
            Log.Info($"Starting run: {runConfig.Id}");
            run.Begin();
        }

        private async UniTask<RunConfig> LoadRunConfigAsync(string runId)
        {
            RunConfigRepository repository = 
                await _configProvider.Get<RunConfigRepository>("runs_conf");
            RunConfig config = System.Array.Find(repository.Runs, r => r.Id == runId);
            return config ?? throw new System.Exception($"Run config '{runId}' not found");
        }
    }
}
