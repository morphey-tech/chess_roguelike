using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Reflect damage back to attacker. Only triggers when the owner is being attacked (is the target).
    /// </summary>
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

        public void OnAfterHit(Figure owner, AfterHitContext context)
        {
            if (owner != context.Target)
            {
                return;
            }
            int reflect = (int)(context.DamageDealt * _reflectPercent);
            if (reflect > 0)
            {
                context.Effects.Add(new ThornsReflectEffect(owner, context.Attacker, reflect));
            }
        }
    }
}
