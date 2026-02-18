using System.Collections.Generic;
using System.Linq;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Generic stat with modifier support. Value is calculated on-the-fly.
    /// </summary>
    public sealed class FigureStat<T>
    {
        /// <summary>For debug: see which mods are applied (e.g. "+5 Aura", "x1.2 Rage").</summary>
        public IReadOnlyList<IStatModifier<T>> Mods => _mods.AsReadOnly();
        
        private readonly T _baseValue;
        private readonly List<IStatModifier<T>> _mods = new();

        public FigureStat(T baseValue)
        {
            _baseValue = baseValue;
        }

        public T Value
        {
            get
            {
                RemoveExpiredModifiers();
                T result = _baseValue;
                foreach (var mod in _mods.OrderBy(x => x.Priority))
                    result = mod.Apply(result);
                return result;
            }
        }

        /// <summary>
        /// Add modifier. Replaces existing if not stackable, otherwise adds to stack.
        /// </summary>
        public void AddModifier(IStatModifier<T> mod)
        {
            if (mod == null) return;

            var existing = _mods.FirstOrDefault(m => m.Id == mod.Id);
            if (existing != null && !existing.Stackable)
            {
                var index = _mods.IndexOf(existing);
                _mods[index] = mod;
            }
            else
            {
                _mods.Add(mod);
            }
        }

        /// <summary>
        /// Remove specific modifier instance.
        /// </summary>
        public bool RemoveModifier(IStatModifier<T> mod)
        {
            return _mods.Remove(mod);
        }

        /// <summary>
        /// Remove all modifiers with specific ID.
        /// </summary>
        public bool RemoveModifiersById(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            
            var removed = _mods.RemoveAll(m => m.Id == id);
            return removed > 0;
        }

        /// <summary>
        /// Called once per turn to update all modifiers.
        /// </summary>
        public void Tick()
        {
            foreach (var mod in _mods)
            {
                mod.Tick();
            }
            RemoveExpiredModifiers();
        }

        private void RemoveExpiredModifiers()
        {
            _mods.RemoveAll(m => m.IsExpired);
        }
    }
}