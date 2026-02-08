using System;
using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class DamageTokenStore : IDamageTokenStore
    {
        private readonly Dictionary<Guid, DamageToken> _tokens = new();

        public void Add(DamageToken token)
        {
            _tokens[token.Id] = token;
        }

        public bool TryGet(Guid id, out DamageToken token)
        {
            return _tokens.TryGetValue(id, out token);
        }

        public void Remove(Guid id)
        {
            _tokens.Remove(id);
        }

        public void Clear()
        {
            _tokens.Clear();
        }
    }
}
