using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public sealed class StatusEffectSystem
    {
        public IReadOnlyList<IStatusEffect> ActiveEffects => _effects; 
        
        private readonly Figure _owner;
        private readonly List<IStatusEffect> _effects = new();
       
        public  StatusEffectSystem(Figure owner)
        {
            _owner = owner;
        }
        
        public void Add(IStatusEffect effect)
        {
            IStatusEffect? existing = _effects.FirstOrDefault(e => e.Id == effect.Id);
            if (existing != null)
            {
                Debug.Log($"{_owner} already has effect {effect.Id}, skipping");
                //Temporary not stackable
                return;
            }
            Debug.Log($"{_owner} gained effect {effect.Id}");
            _effects.Add(effect);
            effect.OnApply(_owner);
        }
        
        public void Remove(IStatusEffect effect)
        {
            effect.OnRemove(_owner);
            _effects.Remove(effect);
        }
        
        public void TriggerBeforeHit(BeforeHitContext ctx)
        {
            _effects.ForEach(e => e.OnBeforeHit(_owner, ctx));
            Cleanup();
        }
        
        private void Cleanup()
        {
            foreach (IStatusEffect statusEffect in _effects
                         .Where(e => e.IsExpired).ToList())
            {
                Remove(statusEffect);
            }
        }

    }
}