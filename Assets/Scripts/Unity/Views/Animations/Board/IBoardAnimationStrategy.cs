using Cysharp.Threading.Tasks;

namespace Project.Unity.Unity.Views.Animations.Board
{
    public interface IBoardAnimationStrategy
    {
        string Id { get; }
        UniTask Play(BoardAnimationTarget target);
    }
}