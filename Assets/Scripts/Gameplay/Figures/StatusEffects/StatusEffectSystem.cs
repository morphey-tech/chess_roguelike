using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public sealed class StatusEffectSystem
    {
        private readonly Figure _owner;
        private readonly TriggerService _triggerService;
        private readonly Dictionary<string, IStatusEffect> _effects = new();

        public StatusEffectSystem(Figure owner, TriggerService triggerService)
        {
            _owner = owner;
            _triggerService = triggerService;
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
            
            // Register as trigger
            _triggerService.Register(effect);
        }

        public void Remove(string id)
        {
            if (_effects.TryGetValue(id, out IStatusEffect? effect))
            {
                // Unregister from triggers
                _triggerService.Unregister(effect);
                
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
    }
}