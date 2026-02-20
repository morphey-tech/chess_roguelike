using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Splash: Deals 50% damage to up to 2 enemies adjacent to the primary target.
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
            float baseDamage = context.DamageDealt;
            int splashDamage = (int)(baseDamage * 0.5f);
            int found = 0;

            // Find up to 2 enemies adjacent to the target
            var targetCell = grid.FindFigure(context.Target);
            if (targetCell == null)
                return;

            foreach (var cell in grid.GetAdjacentCells(targetCell.Position))
            {
                if (found >= 2)
                    break;

                if (cell.OccupiedBy != null && 
                    cell.OccupiedBy.Team != context.Attacker.Team && 
                    cell.OccupiedBy != context.Target)
                {
                    // Add splash damage effect
                    context.AddEffect(new Combat.Effects.Impl.SplashDamageEffect(
                        context.Attacker, 
                        cell.OccupiedBy, 
                        splashDamage));
                    found++;
                }
            }
        }
    }
}
