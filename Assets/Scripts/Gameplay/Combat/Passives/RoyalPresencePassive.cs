using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Royal Presence: allies within 2 cells gain +1 DMG if the King moved this turn.
    /// The buff is applied after the King moves and lasts until the King's next turn.
    /// </summary>
    public class RoyalPresencePassive : IPassive, IOnTurnStart, IOnMove
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly float _damageBonus;
        private readonly int _auraRadius;

        public RoyalPresencePassive(string id, float damageBonus = 1f, int auraRadius = 2)
        {
            Id = id;
            _damageBonus = damageBonus;
            _auraRadius = auraRadius;
        }

        void IOnTurnStart.OnTurnStart(Figure figure, TurnContext context)
        {
            figure.MovedThisTurn = false;
        }

        void IOnMove.OnMove(MoveContext context)
        {
            if (context.Actor.MovedThisTurn)
            {
                BoardGrid grid = context.Grid;
                BoardCell? kingCell = grid.FindFigure(context.Actor);
                if (kingCell == null)
                {
                    return;
                }

                List<Figure> alliesInRange = grid.GetFiguresInRadius(kingCell.Position, _auraRadius)
                    .Where(f => f.Team == context.Actor.Team && f != context.Actor)
                    .ToList();

                foreach (Figure? ally in alliesInRange)
                {
                    ally.Effects.Remove("royal_presence");
                    RoyalPresenceBuffEffect buff = new(context.Actor.Id.ToString(), _damageBonus, turns: 1);
                    ally.Effects.AddOrStack(buff);
                }
            }
        }
    }
}
