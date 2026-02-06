using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Prepare;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Фаза кэширования ассетов prepare-зоны. Добавляется в пайплайн только стейджей с PreparePlacementPhase
    /// (например Duel). На уровнях без prepare этой фазы нет — кэш не трогаем.
    /// Ставится перед BoardSpawnPhase, чтобы к моменту PreparePhase кэш был уже тёплый.
    /// </summary>
    public sealed class PrepareZoneCachePhase : IStagePhase
    {
        private readonly IPrepareZoneAssetPreloader _preloader;
        private readonly ILogger<PrepareZoneCachePhase> _logger;

        public PrepareZoneCachePhase(
            IPrepareZoneAssetPreloader preloader,
            ILogService logService)
        {
            _preloader = preloader;
            _logger = logService.CreateLogger<PrepareZoneCachePhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info("Prepare zone cache warming");
            await _preloader.PreloadAsync(context.RunState);
            _logger.Info("Prepare zone cache ready");
            return PhaseResult.Continue;
        }
    }
}
