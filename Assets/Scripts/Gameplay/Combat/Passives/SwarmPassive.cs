using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Modifier;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// +percent% Attack per allied neighbour. Adds a timed modifier to Stats — no damage math here.
    /// </summary>
    public sealed class SwarmPassive : IPassive, IOnTurnStart
    {
        public string Id { get; }
        public int Priority => 100;
        
        private readonly float _percentPerAlly = 10.0f;
        private readonly int _duration;

        public SwarmPassive(string id, float percentPerAlly, int duration)
        {
            Id = id;
            _percentPerAlly = percentPerAlly;
            _duration = duration;
        }

        public void OnTurnStart(Figure figure, TurnContext context)
        {
            int allies = context.Grid.CountAlliesAround(figure);
            float percentTotal = allies * _percentPerAlly;
            PercentModifier modifier = new(percentTotal, 100);
            TimedStatModifier mod = new(modifier, _duration);
            figure.Stats.Attack.Add(mod);
        }
    }
}