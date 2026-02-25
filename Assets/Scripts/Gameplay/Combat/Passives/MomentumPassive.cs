using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Gains +damage per cell moved closer to enemy this turn.
    /// Resets each turn.
    /// </summary>
    public sealed class MomentumPassive : IPassive, IOnMove
    {
        public string Id { get; }
        public int Priority => 50;

        private readonly float _damagePerCell;

        public MomentumPassive(string id, float damagePerCell)
        {
            Id = id;
            _damagePerCell = damagePerCell;
        }

        public void OnMove(MoveContext context)
        {
            if (!context.DidMove)
            {
                return;
            }

            Figure actor = context.Actor;
            BoardGrid grid = context.Grid;

            GridPosition from = context.From;
            GridPosition to = context.To;
            GridPosition? enemyPos = grid.GetNearestEnemy(actor);

            if (!enemyPos.HasValue)
            {
                return;
            }

            int oldDist = Attack.AttackUtils.GetDistance(from, enemyPos.Value);
            int newDist = Attack.AttackUtils.GetDistance(to, enemyPos.Value);

            if (newDist >= oldDist)
            {
                return;
            }

            int moved = oldDist - newDist;
            float bonus = moved * _damagePerCell;
            FlatModifier<float> modifier = new($"{Id}", bonus, 0, 1, true);
            actor.Stats.Attack.AddModifier(modifier);
        }
    }
}
