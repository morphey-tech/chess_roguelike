using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat.Threat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Stage.Analysis
{
    /// <summary>
    /// Сервис анализа тактической ситуации на поле.
    /// </summary>
    public sealed class StageAnalysisService
    {
        private readonly ThreatMapService _threatMapService;
        private readonly IStageQueryService _stageQueryService;
        private readonly ILogger<StageAnalysisService> _logger;

        public StageAnalysisService(
            ThreatMapService threatMapService,
            IStageQueryService stageQueryService,
            ILogService logService)
        {
            _threatMapService = threatMapService;
            _stageQueryService = stageQueryService;
            _logger = logService.CreateLogger<StageAnalysisService>();
        }

        /// <summary>
        /// Проанализировать тактическую ситуацию для команды.
        /// </summary>
        public StageAnalysisResult AnalyzeStage(Team actorTeam)
        {
            Team enemyTeam = actorTeam == Team.Player
                ? Team.Enemy
                : Team.Player;

            ThreatMap threatMap = _threatMapService.GetThreatMap(enemyTeam);

            return new StageAnalysisResult(threatMap);
        }

        /// <summary>
        /// Проанализировать возможности конкретной фигуры.
        /// </summary>
        public StageActorAnalysis AnalyzeActor(Figure actor, GridPosition pos)
        {
            StageSelectionInfo selection = _stageQueryService.GetSelectionInfo(actor, pos);

            Team enemyTeam = actor.Team == Team.Player
                ? Team.Enemy
                : Team.Player;

            ThreatMap threatMap = _threatMapService.GetThreatMap(enemyTeam);
            List<GridPosition> dangerous = new();
            
            foreach (GridPosition move in selection.MoveTargets)
            {
                if (threatMap.IsThreatened(move))
                {
                    dangerous.Add(move);
                }
            }

            if (threatMap.IsThreatened(pos))
            {
                dangerous.Add(pos);
            }

            _logger.Debug($"AnalyzeActor: {actor.Id} team={actor.Team} pos=({pos.Row},{pos.Column}) moves={selection.MoveTargets.Count} attacks={selection.AttackTargets.Count} dangerous={dangerous.Count} enemyTeam={enemyTeam}");
            
            if (dangerous.Count > 0)
            {
                _logger.Debug($"  Dangerous cells: {string.Join(", ", dangerous.Select(p => $"({p.Row},{p.Column})"))}");
            }

            return new StageActorAnalysis(
                selection.MoveTargets,
                selection.AttackTargets,
                dangerous);
        }

        /// <summary>
        /// Принудительно инвалидировать кэш анализа.
        /// </summary>
        public void Invalidate()
        {
            _threatMapService.Invalidate();
        }
    }
}
