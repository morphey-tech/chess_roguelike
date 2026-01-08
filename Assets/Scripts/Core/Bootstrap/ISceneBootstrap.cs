using Cysharp.Threading.Tasks;
using Project.Core.Scene;

namespace Project.Core.Bootstrap
{
    /// <summary>
    /// Интерфейс для бутстраперов сцен.
    /// Каждая сцена может иметь свой бутстрапер для инициализации.
    /// </summary>
    public interface ISceneBootstrap
    {
        /// <summary>
        /// Запустить бутстрап сцены
        /// </summary>
        UniTask BootstrapAsync(SceneTransitionData? transitionData);
    }
}

