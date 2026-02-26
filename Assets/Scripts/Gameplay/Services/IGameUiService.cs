using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.UI
{
    public interface IGameUiService
    {
        UniTask ShowWorldUiAsync();
        UniTask ShowPreparePhaseAsync();
        UniTask SetGamePhase();
        UniTask HideCombatUiAsync();
        UniTask<StageFlowAction> ShowVictoryScreenAsync(StageResult result);
        UniTask<StageFlowAction> ShowDefeatScreenAsync(StageResult result);
        void ShowWarning(string message);
    }
}
