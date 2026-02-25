using Cysharp.Threading.Tasks;
using Project.Core.Core.Scene;

namespace Project.Core.Core.Bootstrap
{
    public interface ISceneBootstrap
    {
        UniTask BootstrapAsync(SceneTransitionData? transitionData);
    }
}

