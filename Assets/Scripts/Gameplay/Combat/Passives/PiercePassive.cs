using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Pierce: Deals damage to up to 2 enemies behind the primary target (75%, 50%).
    /// </summary>
    public class PiercePassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 100;

        public PiercePassive(string id)
        {
            Id = id;
        }

        void IOnAfterHit.OnAfterHit(Figure owner, AfterHitContext context)
        {
            if (owner != context.Attacker)
            {
                return;
            }

            BoardGrid grid = context.Grid;
            float baseDamage = context.DamageDealt;

            (int dirRow, int dirCol) = context.GetAttackDirection();
            float[] damagePercents = { 0.75f, 0.50f };

            GridPosition current = context.TargetPosition;
            for (int i = 0; i < damagePercents.Length; i++)
            {
                current = new GridPosition(current.Row + dirRow, current.Column + dirCol);

                if (!grid.IsInside(current))
                    break;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != context.Attacker.Team)
                {
                    int pierceDamage = (int)(baseDamage * damagePercents[i]);
                    context.AddEffect(new PierceDamageEffect(context.Attacker, cell.OccupiedBy, pierceDamage));
                }
            }
        }
    }
}
