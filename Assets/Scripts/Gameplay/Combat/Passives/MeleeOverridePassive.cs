using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Ranger Provocation: If enemy is adjacent, must use melee attack instead of ranged.
    /// Melee attacks deal reduced damage (multiplier).
    /// </summary>
    public class MeleeOverridePassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Multiplicative;
        public TriggerPhase Phase => TriggerPhase.BeforeHit;

        private readonly float _meleeDamageMultiplier;

        public MeleeOverridePassive(string id, float meleeDamageMultiplier)
        {
            Id = id;
            _meleeDamageMultiplier = meleeDamageMultiplier;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return false;
            }

            BoardGrid grid = beforeHit.Grid;
            BoardCell? attackerCell = grid.FindFigure(beforeHit.Attacker);
            if (attackerCell == null)
            {
                return false;
            }

            foreach (BoardCell? cell in grid.GetAdjacentCells(attackerCell.Position))
            {
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != beforeHit.Attacker.Team)
                {
                    return true;
                }
            }
            return false;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext damageContext)
            {
                return TriggerResult.Continue;
            }
            return HandleBeforeHit(damageContext);
        }

        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return TriggerResult.Continue;
            }

            BoardGrid grid = beforeHit.Grid;
            BoardCell? attackerCell = grid.FindFigure(beforeHit.Attacker);

            if (attackerCell == null)
            {
                return TriggerResult.Continue;
            }

            bool enemyAdjacent = false;
            foreach (BoardCell? cell in grid.GetAdjacentCells(attackerCell.Position))
            {
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != beforeHit.Attacker.Team)
                {
                    enemyAdjacent = true;
                    break;
                }
            }

            if (enemyAdjacent)
            {
                beforeHit.BonusDamage *= _meleeDamageMultiplier;
            }

            return TriggerResult.Continue;
        }
    }
}
