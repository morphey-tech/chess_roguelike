using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Royal Presence: allies within 2 cells gain +1 DMG if the King moved this turn.
    /// The buff is applied after the King moves and lasts until the King's next turn.
    /// </summary>
    public class RoyalPresencePassive : IPassive, IOnMove
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterMove;

        private readonly float _damageBonus;
        private readonly int _auraRadius;

        public RoyalPresencePassive(string id, float damageBonus = 1f, int auraRadius = 2)
        {
            Id = id;
            _damageBonus = damageBonus;
            _auraRadius = auraRadius;
        }

        public bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnMove;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context.Type == TriggerType.OnMove)
            {
                ExecuteMove(context);
            }

            return TriggerResult.Continue;
        }

        private void ExecuteMove(TriggerContext context)
        {
            if (!(context.Actor is Figure actor))
            {
                return;
            }
            if (!actor.MovedThisTurn)
            {
                return;
            }
            if (!(context.Data is MoveContext move))
            {
                return;
            }

            BoardGrid grid = move.Grid;
            BoardCell? kingCell = grid.FindFigure(actor);
            if (kingCell == null)
            {
                return;
            }

            List<Figure> alliesInRange = grid.GetFiguresInRadius(kingCell.Position, _auraRadius)
                .Where(f => f.Team == actor.Team && f != actor)
                .ToList();

            foreach (Figure? ally in alliesInRange)
            {
                ally.Effects.Remove("royal_presence");
                RoyalPresenceBuffEffect buff = new(actor.Id.ToString(), _damageBonus, turns: 1);
                ally.Effects.AddOrStack(buff);
            }
        }
    }
}
