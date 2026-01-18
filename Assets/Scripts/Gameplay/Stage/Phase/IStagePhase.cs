using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Stage
{
    public interface IStagePhase
    {
        UniTask<PhaseResult> ExecuteAsync(StageContext context);
    }

    public enum PhaseResult
    {
        Continue,
        WaitForCompletion,
        Victory,
        Defeat
    }
}