using Project.Core.Core.Grid;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Pierce: Deals damage to up to 2 enemies behind the primary target (75%, 50%).
    /// </summary>
    public class PiercePassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        public PiercePassive(string id)
        {
            Id = id;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnAfterHit)
            {
                return false;
            }
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return false;
            }
            return context.Actor == afterHit.Attacker;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return TriggerResult.Continue;
            }

            BoardGrid grid = afterHit.Grid;
            float baseDamage = afterHit.DamageDealt;

            (int dirRow, int dirCol) = afterHit.GetAttackDirection();
            float[] damagePercents = { 0.75f, 0.50f };

            GridPosition current = afterHit.TargetPosition;
            for (int i = 0; i < damagePercents.Length; i++)
            {
                current = new GridPosition(current.Row + dirRow, current.Column + dirCol);

                if (!grid.IsInside(current))
                {
                    break;
                }

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != afterHit.Attacker.Team)
                {
                    int pierceDamage = (int)(baseDamage * damagePercents[i]);
                    afterHit.AddEffect(new PierceDamageEffect(afterHit.Attacker, cell.OccupiedBy, pierceDamage));
                }
            }

            return TriggerResult.Continue;
        }
    }
}
