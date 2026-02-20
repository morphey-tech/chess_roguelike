using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Splash: Deals 50% damage to up to 2 enemies in straight line (left/right) from the primary target.
    /// </summary>
    public class SplashPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 100;

        public SplashPassive(string id)
        {
            Id = id;
        }

        void IOnAfterHit.OnAfterHit(Figure owner, AfterHitContext context)
        {
            // Only trigger if owner was the attacker
            if (owner != context.Attacker)
                return;

            var grid = context.Grid;
            
            // Calculate splash damage from attacker's attack stat
            float baseDamage = context.Attacker.Stats.Attack.Value;
            int splashDamage = (int)(baseDamage * 0.5f);

            // Find enemies in straight line (left and right) from target
            var targetCell = grid.FindFigure(context.Target);
            if (targetCell == null)
                return;

            // Check left (-1 column)
            GridPosition leftPos = new(targetCell.Position.Row, targetCell.Position.Column - 1);
            if (grid.IsInside(leftPos))
            {
                var leftCell = grid.GetBoardCell(leftPos);
                if (leftCell.OccupiedBy != null && 
                    leftCell.OccupiedBy.Team != context.Attacker.Team &&
                    leftCell.OccupiedBy != context.Target)
                {
                    context.AddEffect(new Combat.Effects.Impl.SplashDamageEffect(
                        context.Attacker,
                        leftCell.OccupiedBy,
                        splashDamage));
                }
            }

            // Check right (+1 column)
            GridPosition rightPos = new(targetCell.Position.Row, targetCell.Position.Column + 1);
            if (grid.IsInside(rightPos))
            {
                var rightCell = grid.GetBoardCell(rightPos);
                if (rightCell.OccupiedBy != null && 
                    rightCell.OccupiedBy.Team != context.Attacker.Team &&
                    rightCell.OccupiedBy != context.Target)
                {
                    context.AddEffect(new Combat.Effects.Impl.SplashDamageEffect(
                        context.Attacker,
                        rightCell.OccupiedBy,
                        splashDamage));
                }
            }
        }
    }
}
