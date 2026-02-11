using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Stage.Flow
{
    /// <summary>
    /// Placeholder transition service for stage flow operations.
    /// Hook fade animation here when dedicated transition UI is ready.
    /// </summary>
    public sealed class RunTransitionService : IRunTransitionService
    {
        public async UniTask PlayTransitionAsync()
        {
            await UniTask.Yield();
        }
    }
}
