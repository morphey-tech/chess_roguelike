using System;
using UniRx;

namespace Project.Core.Core.Blockers
{
    public sealed class PlayerBlockerService : IPlayerBlockerService, IDisposable
    {
        IReadOnlyReactiveProperty<PlayerBlocker> IPlayerBlockerService.Blockers => _blockers;

        private readonly ReactiveProperty<PlayerBlocker> _blockers = new(PlayerBlocker.None);
        
        void IPlayerBlockerService.Add(PlayerBlocker blocker)
        {
            _blockers.Value |= blocker;
        }

        void IPlayerBlockerService.Remove(PlayerBlocker blocker)
        {
            _blockers.Value &= ~blocker;
        }

        bool IPlayerBlockerService.Has(PlayerBlocker blocker)
        {
            return _blockers.Value.HasFlag(blocker);
        }

        void IDisposable.Dispose()
        {
            _blockers.Dispose();
        }
    }
}