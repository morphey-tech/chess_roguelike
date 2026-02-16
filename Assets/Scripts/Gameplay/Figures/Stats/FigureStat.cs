using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Modifier;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Stat = Base + ordered modifiers. All math happens here only.
    /// </summary>
    public sealed class FigureStat
    {
        private readonly float _baseValue;
        private readonly List<IStatModifier<float>> _mods = new();

        public FigureStat(float baseValue)
        {
            _baseValue = baseValue;
        }

        public float Value
        {
            get
            {
                float v = _baseValue;
                foreach (var m in _mods.OrderBy(x => x.Priority))
                    v = m.Apply(v);
                return v;
            }
        }

        /// <summary>For debug: see which mods are applied (e.g. "+5 Aura", "x1.2 Rage").</summary>
        public IReadOnlyList<IStatModifier<float>> Mods => _mods;

        public void Add(IStatModifier<float> mod)
        {
            _mods.Add(mod);
        }

        public void Remove(IStatModifier<float> mod)
        {
            _mods.Remove(mod);
        }

        /// <summary>Call at end of turn: ticks timed mods and removes expired.</summary>
        public void Tick()
        {
            for (int i = _mods.Count - 1; i >= 0; i--)
            {
                if (_mods[i] is ITimedModifier timed)
                {
                    timed.Tick();
                    if (timed.IsExpired)
                        _mods.RemoveAt(i);
                }
            }
        }
    }
}