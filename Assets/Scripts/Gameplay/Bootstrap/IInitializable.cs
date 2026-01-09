using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Bootstrap
{
    public interface IInitializable
    {
        UniTask InitializeAsync();
    }
}