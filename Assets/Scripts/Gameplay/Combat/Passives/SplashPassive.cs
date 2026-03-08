using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
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
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        public SplashPassive(string id)
        {
            Id = id;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnAfterHit)
            {
                return false;
            }
            if (!context.TryGetData(out AfterHitContext? afterHit))
            {
                return false;
            }
            return context.Actor == afterHit.Attacker;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData(out AfterHitContext? afterHit))
            {
                return TriggerResult.Continue;
            }

            BoardGrid grid = afterHit.Grid;
            float baseDamage = afterHit.Attacker.Stats.Attack.Value;
            int splashDamage = (int)(baseDamage * 0.5f);

            BoardCell? targetCell = grid.FindFigure(afterHit.Target);
            if (targetCell == null)
            {
                return TriggerResult.Continue;
            }

            Team enemyTeam = afterHit.Attacker.Team == Team.Player ? Team.Enemy : Team.Player;

            // Get enemies to the left and right of the target
            List<Figure> splashTargets = grid
                .GetAdjacentEnemies(targetCell.Position, enemyTeam, (0, -1), (0, 1))
                .Take(2)
                .ToList();

            // Add single splash effect with all targets — they will be hit simultaneously
            if (splashTargets.Count > 0)
            {
                afterHit.AddEffect(new SplashDamageEffect(
                    afterHit.Attacker,
                    splashTargets.ToArray(),
                    splashDamage));
            }

            return TriggerResult.Continue;
        }
    }
}
