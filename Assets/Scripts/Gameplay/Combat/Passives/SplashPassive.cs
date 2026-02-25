using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Splash: Deals 50% damage to up to 2 enemies in straight line (left/right) from the primary target.
    /// All splash targets are hit simultaneously.
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
            {
                return;
            }

            BoardGrid grid = context.Grid;
            float baseDamage = context.Attacker.Stats.Attack.Value;
            int splashDamage = (int)(baseDamage * 0.5f);

            BoardCell? targetCell = grid.FindFigure(context.Target);
            if (targetCell == null)
            {
                return;
            }

            Team enemyTeam = context.Attacker.Team == Team.Player ? Team.Enemy : Team.Player;

            // Get enemies to the left and right of the target
            List<Figure> splashTargets = grid
                .GetAdjacentEnemies(targetCell.Position, enemyTeam, (0, -1), (0, 1))
                .Take(2)
                .ToList();

            // Add single splash effect with all targets — they will be hit simultaneously
            if (splashTargets.Count > 0)
            {
                context.AddEffect(new SplashDamageEffect(
                    context.Attacker,
                    splashTargets.ToArray(),
                    splashDamage));
            }
        }
    }
}
