using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FiguresSpawnPhase : IStagePhase
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly IFiguresSpawnProvider _spawnProvider;
        private readonly ILogger<FiguresSpawnPhase> _logger;

        public FiguresSpawnPhase(
            FigureSpawnService figureSpawnService,
            IFiguresSpawnProvider spawnProvider,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _spawnProvider = spawnProvider;
            _logger = logService.CreateLogger<FiguresSpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            IReadOnlyList<FigureSpawnEntry> figures =
                await _spawnProvider.BuildAsync(context);

            foreach (FigureSpawnEntry figure in figures)
            {
                if (!context.Grid.IsInside(figure.Position))
                {
                    _logger.Warning($"Spawn outside grid: {figure.Position}");
                    continue;
                }

                await _figureSpawnService.SpawnAsync(
                    context.Grid,
                    figure.Position,
                    figure.Id,
                    figure.Team);
            }

            return PhaseResult.Continue;
        }
    }
}