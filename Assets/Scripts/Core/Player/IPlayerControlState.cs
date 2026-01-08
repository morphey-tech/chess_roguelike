using UniRx;

namespace Project.Core.Player
{
    public interface IPlayerControlState
    {
        IReadOnlyReactiveProperty<bool> CanMove { get; }
        IReadOnlyReactiveProperty<bool> CanLook { get; }
    }
}