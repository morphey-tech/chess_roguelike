using Cysharp.Threading.Tasks;

namespace LiteUI.World
{
    public interface IActivatable
    {
        UniTask Activate();
        UniTask Deactivate();
    }
}

