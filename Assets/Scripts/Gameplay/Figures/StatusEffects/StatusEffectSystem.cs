using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public sealed class StatusEffectSystem
    {
        private readonly Figure _owner;
        private readonly Dictionary<string, IStatusEffect> _effects = new();

        private List<IStatusEffect>? _currentEffects;

        public StatusEffectSystem(Figure owner)
        {
            _owner = owner;
        }

        public void AddOrStack(IStatusEffect effect)
        {
            if (_effects.TryGetValue(effect.Id, out IStatusEffect existing))
            {
                if (existing is StackableStatusEffect stackable
                    && effect is StackableStatusEffect)
                {
                    stackable.AddStack();
                    return;
                }
            }
            _effects[effect.Id] = effect;
            effect.OnApply(_owner);
        }

        public void Remove(string id)
        {
            if (_effects.TryGetValue(id, out IStatusEffect? effect))
            {
                effect.OnRemove(_owner);
                _effects.Remove(id);
            }
        }

        /// <summary>
        /// Get all active status effects.
        /// </summary>
        public IEnumerable<IStatusEffect> GetEffects()
        {
            return _effects.Values;
        }

        private void Cleanup()
        {
            List<string> expired = _effects
                .Where(e => e.Value.IsExpired)
                .Select(e => e.Key)
                .ToList();

            foreach (string key in expired)
            {
                _effects[key].OnRemove(_owner);
                _effects.Remove(key);
            }
        }
    }
}