using System;
using Cysharp.Threading.Tasks;

namespace Project.Core.Core.Scene
{
    public interface ISceneService
    {
        string CurrentScene { get; }
        IObservable<SceneLoadProgress> OnLoadProgress { get; }

        UniTask LoadAsync(
            string targetScene,
            SceneLoadParams loadParams,
            SceneTransitionData transitionData);
    }
}

