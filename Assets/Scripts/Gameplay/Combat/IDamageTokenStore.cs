using System;

namespace Project.Gameplay.Gameplay.Combat
{
    public interface IDamageTokenStore
    {
        void Add(DamageToken token);
        bool TryGet(Guid id, out DamageToken token);
        void Remove(Guid id);
        void Clear();
    }
}
