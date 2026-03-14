using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.UI.Combat;
using Project.Gameplay.UI;
using UnityEngine;
using VContainer;

namespace Project.Unity.UI
{
    public sealed class GameUiService : IGameUiService
    {
        private readonly ILogger<GameUiService> _logger;
        private readonly IUIService _uiService;

        [Inject]
        public GameUiService(ILogService logService, IUIService uiService)
        {
            _logger = logService.CreateLogger<GameUiService>();
            _uiService = uiService;
        }

        public async UniTask ShowWorldUiAsync()
        {
            await _uiService.ShowAsync<WorldUIWindow>();
        }

        public async UniTask ShowPreparePhase()
        {
            await _uiService.ShowAsync<PrepareWindow>();
            // Board capacity window disabled temporarily
            // await _uiService.ShowAsync<BoardCapacityWindow>();
        }

        public async UniTask ShowBattlePhase()
        {
            _uiService.Hide<PrepareWindow>();
            //await _uiService.ShowAsync<TurnWindow>();
            await _uiService.ShowAsync<CombatHUDWindow>();
        }

        public UniTask HideBattlePhase()
        {
            _uiService.Hide<CombatHUDWindow>();
            // UIService.Hide<BoardCapacityWindow>();
            return UniTask.CompletedTask;
        }

        public UniTask<StageFlowAction> ShowVictoryScreenAsync(StageResult result)
        {
            _logger.Info($"Victory! turns={result.TurnCount}, kills={result.EnemiesKilled}");
            return ShowOutcomeScreenAsync(
                "Victory",
                $"Turns: {result.TurnCount}\nKills: {result.EnemiesKilled}",
                ("Next stage", StageFlowAction.NextStage),
                ("Restart stage", StageFlowAction.RestartStage),
                ("Back to hub", StageFlowAction.GoHub));
        }

        public UniTask<StageFlowAction> ShowDefeatScreenAsync(StageResult result)
        {
            _logger.Info($"Defeat! turns={result.TurnCount}, kills={result.EnemiesKilled}");
            return ShowOutcomeScreenAsync(
                "Defeat",
                $"Turns: {result.TurnCount}\nKills: {result.EnemiesKilled}",
                ("Restart stage", StageFlowAction.RestartStage),
                ("Back to hub", StageFlowAction.GoHub));
        }

        public void ShowWarning(string message)
        {
            _logger.Warning($"[UI Warning] {message}");
        }

        /// <summary>
        /// Show artifacts window (player's owned artifacts).
        /// </summary>
        public void ShowArtifacts()
        {
            _uiService.Show<ArtifactsWindow>();
        }

        private async UniTask<StageFlowAction> ShowOutcomeScreenAsync(
            string title,
            string body,
            params (string label, StageFlowAction action)[] actions)
        {
            StageOutcomeWindow? window = await _uiService.GetOrCreateAsync<StageOutcomeWindow>();
            return await window.ShowAsync(new StageOutcomeWindow.Model
            {
                Title = title,
                Body = body,
                Actions = actions
            });
        }
    }
}
