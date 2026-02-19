using System.Collections.Generic;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public class FirstShotPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly List<Figure> _cachedTargets = new(); 
        private readonly float _damage;
        
        
        public FirstShotPassive(string id, float damage)
        {
            Id = id;
            _damage = damage;
        }

        void IOnBeforeHit.OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            if (_cachedTargets.Contains(context.Target))
            {
                return;
            }
            _cachedTargets.Add(context.Target);
            context.BonusDamage = _damage;
        }
    }
}