using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Stage
{
    public interface IStagePhase
    {
        /// <summary>
        /// Executes the phase. Returns result indicating what to do next.
        /// </summary>
        UniTask<PhaseResult> ExecuteAsync(StageContext context);
    }

    public enum PhaseResult
    {
        /// <summary>
        /// Phase completed, proceed to next phase.
        /// </summary>
        Continue,
        
        /// <summary>
        /// Phase requires waiting (e.g. player input). 
        /// Phase will call context.CompletePhase() when ready.
        /// </summary>
        WaitForCompletion,
        
        /// <summary>
        /// Stage should end with victory.
        /// </summary>
        Victory,
        
        /// <summary>
        /// Stage should end with defeat.
        /// </summary>
        Defeat
    }
}