using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Stage.Flow
{
    public interface IRunTransitionService
    {
        UniTask PlayTransitionAsync();
    }
}
