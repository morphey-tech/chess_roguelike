using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Configs.Run;
using Project.Core.Core.Configs.Suites;
using Project.Core.Core.Filters;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.Filters;
using Project.Gameplay.Gameplay.Filters.Impl;
using Project.Gameplay.UI;

namespace Project.Unity.Unity.Bootstrap
{
    /// <summary>
    /// Бутстрап игровой сцены.
    /// Запускает Run из конфига.
    /// </summary>
    public class GameSceneBootstrap : MonoSceneBootstrap
    {
        private const string DefaultRunId = "tutorial";
        
        private ConfigProvider _configProvider = null!;
        private RunFactory _runFactory = null!;
        private RunHolder _runHolder = null!;
        private PlayerRunStateService _runStateService = null!;
        private PlayerLoadoutService _loadoutService = null!;
        private RandomService _randomService = null!;
        private EconomyService _economyService = null!; 
        private IAppFilterService _filterService = null!;
        private IUIService _uiService = null!;

        protected override void OnConstruct()
        {
            _configProvider = Resolve<ConfigProvider>();
            _runFactory = Resolve<RunFactory>();
            _runHolder = Resolve<RunHolder>();
            _runStateService = Resolve<PlayerRunStateService>();
            _loadoutService = Resolve<PlayerLoadoutService>();
            _randomService = Resolve<RandomService>();
            _economyService = Resolve<EconomyService>();
            _filterService = Resolve<IAppFilterService>();
            _uiService = Resolve<IUIService>();
        }

        protected override async UniTask OnBootstrapAsync()
        {
            await RunFilters();
            //TODO: в фильтры надо вытащить думаю предзагрузку всякого в том числе ui базового
            await _configProvider.PreloadAllAsync(this.GetCancellationTokenOnDestroy());

            RunConfig runConfig = await LoadRunConfigAsync(DefaultRunId);
            await InitializeRunStateAsync(runConfig);

            _economyService.StartNewRun();
            await ShowResourcesWindowAsync();

            Run run = _runFactory.Create(runConfig);
            _runHolder.Set(run);
            run.Begin().Forget();
        }
        
        private async UniTask RunFilters()
        {
            _filterService.AddFilter<AddressablesInitFilter>();
            _filterService.AddFilter<AnnotationScanFilter>();
            _filterService.AddFilter<UIInitializationFilter>();

            try
            {
                await _filterService.RunAsync();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Log.Error("Error at run filters", e);
            }
            Log.Info("App filters executed");
        }

        private async UniTask ShowResourcesWindowAsync()
        {
            // UIService уже инициализирован фильтром UIInitializationFilter
            await _uiService.ShowAsync<ResourcesWindow>();
            Log.Info("Resources window shown");
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
