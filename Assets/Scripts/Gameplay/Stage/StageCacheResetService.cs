using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Resets hot-reload and runtime caches used by stage initialization.
    /// </summary>
    public sealed class StageCacheResetService
    {
        private readonly ConfigHotReloadService _configHotReload;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly FigureStatsFactory _figureStatsFactory;
        private readonly TurnPatternFactory _turnPatternFactory;

        public StageCacheResetService(
            ConfigHotReloadService configHotReload,
            FigureSpawnService figureSpawnService,
            FigureStatsFactory figureStatsFactory,
            TurnPatternFactory turnPatternFactory)
        {
            _configHotReload = configHotReload;
            _figureSpawnService = figureSpawnService;
            _figureStatsFactory = figureStatsFactory;
            _turnPatternFactory = turnPatternFactory;
        }

        public void ResetCaches()
        {
            _configHotReload.ReloadIfDirty();
            _figureSpawnService.ClearCache();
            _figureStatsFactory.ClearCache();
            _turnPatternFactory.ResetCache();
        }
    }
}
