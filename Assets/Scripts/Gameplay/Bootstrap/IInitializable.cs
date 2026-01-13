using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Bootstrap
{
    public interface IInitializable
    {
        UniTask InitializeAsync();
    }
}