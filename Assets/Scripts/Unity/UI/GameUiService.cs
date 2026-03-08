using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.UI;
using UnityEngine;

namespace Project.Unity.UI
{
    public sealed class GameUiService : IGameUiService
    {
        private readonly ILogger<GameUiService> _logger;

        public GameUiService(ILogService logService)
        {
            _logger = logService.CreateLogger<GameUiService>();
        }

        public async UniTask ShowWorldUiAsync()
        {
            await UIService.Initialized;
            await UIService.ShowAsync<WorldUIWindow>();
        }

        public async UniTask ShowPreparePhaseAsync()
        {
            await UIService.Initialized;
            TurnWindow? wnd = await UIService.ShowAsync<TurnWindow>();
            wnd?.SetPreparePhase();
            // Board capacity window disabled temporarily
            // await UIService.ShowAsync<BoardCapacityWindow>();
        }

        public async UniTask SetGamePhase()
        {
            if (!UIService.IsValid)
            {
                return;
            }

            TurnWindow? wnd = await UIService.GetOrCreateAsync<TurnWindow>();
            wnd?.SetGamePhase();
        }

        public UniTask HideCombatUiAsync()
        {
            if (UIService.IsValid)
            {
                UIService.Hide<TurnWindow>();
                // UIService.Hide<BoardCapacityWindow>();
            }
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
            if (UIService.IsValid)
            {
                UIService.Show<ArtifactsWindow>();
            }
        }

        private async UniTask<StageFlowAction> ShowOutcomeScreenAsync(
            string title,
            string body,
            params (string label, StageFlowAction action)[] actions)
        {
            if (!UIService.IsValid)
            {
                return StageFlowAction.RestartStage;
            }

            StageOutcomeWindow? window = await UIService.GetOrCreateAsync<StageOutcomeWindow>();
            return await window.ShowAsync(new StageOutcomeWindow.Model
            {
                Title = title,
                Body = body,
                Actions = actions
            });
        }
    }
}
