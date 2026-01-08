using UniRx;

namespace Project.Core.Core.Blockers
{
    public interface IPlayerBlockerService
    {
        IReadOnlyReactiveProperty<PlayerBlocker> Blockers { get; }
        void Add(PlayerBlocker blocker);
        void Remove(PlayerBlocker blocker);
        bool Has(PlayerBlocker blocker);
    }
}