using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Sirenix.Utilities;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public sealed class StatusEffectSystem
    {
        private readonly Figure _owner;
        private readonly Dictionary<string, IStatusEffect> _effects = new();
       
        public  StatusEffectSystem(Figure owner)
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
                    //Update duration
                    return;
                }
            }
            _effects[effect.Id] = effect;
        }

        public void TriggerBeforeHit(BeforeHitContext ctx)
        {
            _effects.ForEach(e =>
            {
                e.Value.OnBeforeHit(_owner, ctx);
            });
            Cleanup();
        }

        public void TriggerAfterHit(AfterHitContext ctx)
        {
            _effects.ForEach(e =>
            {
                e.Value.OnAfterHit(_owner, ctx);
            });
            Cleanup();
        }
        
        private void Cleanup()
        {
            List<string> expired = _effects
                .Where(e => e.Value.IsExpired)
                .Select(e => e.Key)
                .ToList();
            
            foreach (string key in expired)
            {
                _effects.Remove(key);
            }
        }
    }
}