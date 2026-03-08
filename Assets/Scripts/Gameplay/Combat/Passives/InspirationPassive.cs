using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Random;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Inspiration: at turn start, grants a buff to a random adjacent ally.
    /// </summary>
    public class InspirationPassive : IPassive, IOnTurnStart
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.OnTurnStart;

        private readonly float _attackBonus;
        private readonly float _defenceBonus;
        private readonly float _evasionBonus;
        private readonly int _buffDuration;
        private readonly IRandomService _random;

        public InspirationPassive(string id,
            float attackBonus,
            float defenceBonus,
            float evasionBonus,
            IRandomService random,  
            int buffDuration = 2)
        {
            Id = id;
            _attackBonus = attackBonus;
            _defenceBonus = defenceBonus;
            _evasionBonus = evasionBonus;
            _buffDuration = buffDuration;
            _random = random;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnTurnStart)
            {
                return false;
            }
            if (!(context.Data is TurnContext turn))
            {
                return false;
            }
            if (!(context.Actor is Figure figure))
            {
                return false;
            }
            return turn.Team == figure.Team;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not ITurnContext turnContext)
            {
                return TriggerResult.Continue;
            }
            return HandleTurnStart(turnContext);
        }

        public TriggerResult HandleTurnStart(ITurnContext context)
        {
            if (!(context.Actor is Figure figure))
            {
                return TriggerResult.Continue;
            }
            if (!context.TryGetData<TurnContext>(out TurnContext turn))
            {
                return TriggerResult.Continue;
            }

            if (turn.Team != figure.Team)
            {
                return TriggerResult.Continue;
            }

            BoardGrid grid = turn.Grid;
            BoardCell? ownerCell = grid.FindFigure(figure);
            if (ownerCell == null)
            {
                return TriggerResult.Continue;
            }

            List<Figure> adjacentAllies = grid.GetAdjacentCells(ownerCell.Position)
                .Select(c => c.OccupiedBy)
                .Where(f => f != null && f.Team == figure.Team && f != figure)
                .Cast<Figure>()
                .ToList();

            if (adjacentAllies.Count == 0)
            {
                return TriggerResult.Continue;
            }

            Figure target = adjacentAllies[_random.Range(0, adjacentAllies.Count - 1)];
            BuffType buffType = (BuffType)_random.Range(0, 2);
            float value = buffType switch
            {
                BuffType.Attack => _attackBonus,
                BuffType.Defence => _defenceBonus,
                BuffType.Evasion => _evasionBonus,
                _ => _attackBonus
            };

            InspirationBuffEffect buff = new(buffType, value, _buffDuration);
            target.Effects.AddOrStack(buff);

            return TriggerResult.Continue;
        }
    }
}
