using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class ThornsPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 200;
        
        private readonly float _reflectPercent;

        public ThornsPassive(string id, float reflectPercent)
        {
            Id = id;
            _reflectPercent = reflectPercent;
        }

        public void OnAfterHit(AfterHitContext context)
        {
            // Only trigger when this figure is the TARGET
            if (context.Target.Passives.Contains(this))
            {
                int reflect = (int)(context.DamageDealt * _reflectPercent);
                if (reflect > 0)
                    context.Attacker.Stats.TakeDamage(reflect);
            }
        }
    }
}
