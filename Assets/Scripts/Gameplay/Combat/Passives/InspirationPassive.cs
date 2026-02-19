using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Inspiration: at turn start, grants a random buff to a random adjacent ally.
    /// </summary>
    public class InspirationPassive : IPassive, IOnTurnStart
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly float _attackBonus;
        private readonly float _defenceBonus;
        private readonly float _evasionBonus;
        private readonly int _buffDuration;

        public InspirationPassive(
            string id,
            float attackBonus,
            float defenceBonus,
            float evasionBonus,
            int buffDuration = 2)
        {
            Id = id;
            _attackBonus = attackBonus;
            _defenceBonus = defenceBonus;
            _evasionBonus = evasionBonus;
            _buffDuration = buffDuration;
        }

        void IOnTurnStart.OnTurnStart(Figure figure, TurnContext context)
        {
            if (context.Team != figure.Team)
                return;

            BoardGrid grid = context.Grid;
            BoardCell? ownerCell = grid.FindFigure(figure);
            if (ownerCell == null)
                return;

            List<Figure> adjacentAllies = grid.GetAdjacentCells(ownerCell.Position)
                .Select(c => c.OccupiedBy)
                .Where(f => f != null && f.Team == figure.Team && f != figure)
                .Cast<Figure>()
                .ToList();
                
            if (adjacentAllies.Count == 0)
                return;

            Figure target = adjacentAllies[Random.Range(0, adjacentAllies.Count)];
            BuffType buffType = (BuffType)Random.Range(0, 3);
            float value = buffType switch
            {
                BuffType.Attack => _attackBonus,
                BuffType.Defence => _defenceBonus,
                BuffType.Evasion => _evasionBonus,
                _ => _attackBonus
            };

            InspirationBuffEffect buff = new(buffType, value, _buffDuration);
            target.Effects.AddOrStack(buff);
        }
    }
}
