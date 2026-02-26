using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Run;
using Project.Core.Core.Configs.Suites;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Save.Service;

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
        private PlayerRunStateService _runStateService;
        private PlayerLoadoutService _loadoutService;
        private RandomService _randomService;

        private const string DefaultRunId = "tutorial";

        protected override void OnConstruct()
        {
            _configProvider = Resolve<ConfigProvider>();
            _runFactory = Resolve<RunFactory>();
            _runHolder = Resolve<RunHolder>();
            _runStateService = Resolve<PlayerRunStateService>();
            _loadoutService = Resolve<PlayerLoadoutService>();
            _randomService = Resolve<RandomService>();
        }

        protected override async UniTask OnBootstrapAsync()
        {
            Log.Info("Game bootstrap started");
            await _configProvider.PreloadAllAsync(this.GetCancellationTokenOnDestroy());

            RunConfig runConfig = await LoadRunConfigAsync(DefaultRunId);
            await InitializeRunStateAsync(runConfig);

            Run run = _runFactory.Create(runConfig);
            
            // Сохраняем в holder чтобы другие сервисы могли получить доступ
            _runHolder.Set(run);
            
            Log.Info($"Starting run: {runConfig.Id}");
            // Fire-and-forget: game loop runs indefinitely (phases wait for player input).
            // Awaiting here would block the bootstrap and prevent the previous scene from unloading.
            run.Begin().Forget();
        }

        private async UniTask InitializeRunStateAsync(RunConfig runConfig)
        {
            Log.Info($"InitializeRunStateAsync: HasRun={_runStateService.HasRun}");

            // Skip if run state already exists (e.g. loading from save)
            if (_runStateService.HasRun)
            {
                Log.Info($"Run state already exists (FiguresInHand={_runStateService.Current.FiguresInHand.Count}), skipping initialization");
                return;
            }

            Log.Info($"Loading suite config for suiteId: {_loadoutService.Current.SuiteId}");
            SuiteConfig suiteConfig = await LoadSuiteConfigAsync();

            string[] figureIds = suiteConfig.Figures;
            string firstStageId = runConfig.Stages[0];

            Log.Info($"Suite '{suiteConfig.Id}' has {figureIds.Length} figures: [{string.Join(", ", figureIds)}]");

            _runStateService.StartNew(_loadoutService.Current, figureIds, firstStageId);
            _randomService.SetSeed(_runStateService.Current.Seed);
            Log.Info($"Initialized run state with {_runStateService.Current.Figures.Count} figures in hand, seed={_runStateService.Current.Seed}, starting stage: {firstStageId}");
        }

        private async UniTask<RunConfig> LoadRunConfigAsync(string runId)
        {
            RunConfigRepository repository = 
                await _configProvider.Get<RunConfigRepository>("runs_conf");
            RunConfig config = repository.Get(runId);
            return config ?? throw new Exception($"Run config '{runId}' not found");
        }

        private async UniTask<SuiteConfig> LoadSuiteConfigAsync()
        {
            SuiteConfigRepository repository = 
                await _configProvider.Get<SuiteConfigRepository>("suites_conf");
            string suiteId = _loadoutService.Current.SuiteId;
            SuiteConfig config = repository.Get(suiteId);
            return config ?? throw new Exception($"Suite config '{suiteId}' not found");
        }
    }
}
