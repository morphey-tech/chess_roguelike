using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

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
            // Only trigger if owner was the attacker
            if (owner != context.Attacker)
                return;

            var grid = context.Grid;
            float baseDamage = context.DamageDealt;

            // Calculate pierce direction (from attacker to target)
            int dirRow = context.TargetPosition.Row - context.AttackerPosition.Row;
            int dirCol = context.TargetPosition.Column - context.AttackerPosition.Column;

            if (dirRow != 0)
                dirRow = dirRow > 0 ? 1 : -1;
            if (dirCol != 0)
                dirCol = dirCol > 0 ? 1 : -1;

            // Damage percentages for first and second target behind
            float[] damagePercents = { 0.75f, 0.50f };
            
            GridPosition current = context.TargetPosition;
            for (int i = 0; i < damagePercents.Length; i++)
            {
                current = new GridPosition(current.Row + dirRow, current.Column + dirCol);

                if (!grid.IsInside(current))
                    break;

                var cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != context.Attacker.Team)
                {
                    int pierceDamage = (int)(baseDamage * damagePercents[i]);
                    context.AddEffect(new PierceDamageEffect(context.Attacker, cell.OccupiedBy, pierceDamage));
                }
            }
        }
    }
}
