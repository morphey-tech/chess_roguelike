using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// +percent% Attack per allied neighbour. Adds a timed modifier to Stats — no damage math here.
    /// </summary>
    public sealed class SwarmPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 100;
        
        private readonly float _percentPerAlly;
        private readonly int _duration;

        public SwarmPassive(string id, float percentPerAlly, int duration)
        {
            Id = id;
            _percentPerAlly = percentPerAlly;
            _duration = duration;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            int allies = context.Grid.CountAlliesAround(owner);
            float percentTotal = allies * _percentPerAlly;
            
            var modifier = new PercentModifier($"{Id}_swarm", percentTotal, 100, _duration, false);
            owner.Stats.Attack.AddModifier(modifier);
        }
    }
}