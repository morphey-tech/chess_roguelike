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
                return;

            Figure actor = context.Figure;
            BoardGrid grid = context.Grid;

            GridPosition from = context.From;
            GridPosition to = context.To;

            GridPosition? enemyPos = FindNearestEnemy(actor, grid);

            if (!enemyPos.HasValue)
                return;

            int oldDist = Attack.AttackUtils.GetDistance(from, enemyPos.Value);
            int newDist = Attack.AttackUtils.GetDistance(to, enemyPos.Value);

            if (newDist >= oldDist)
            {
                return;
            }

            int moved = oldDist - newDist;
            float bonus = moved * _damagePerCell;
            var modifier = new FlatModifier<float>($"{Id}_momentum", bonus, 0, 1, true);
            actor.Stats.Attack.AddModifier(modifier);
        }

        private static GridPosition? FindNearestEnemy(
            Figure actor,
            BoardGrid grid)
        {
            GridPosition? nearest = null;
            int min = int.MaxValue;

            foreach (BoardCell cell in grid.AllCells())
            {
                Figure other = cell.OccupiedBy;

                if (other == null)
                {
                    continue;
                }
                if (other.Team == actor.Team)
                {
                    continue;
                }

                int d = Attack.AttackUtils.GetDistance(
                    grid.FindFigure(actor).Position,
                    cell.Position
                );

                if (d < min)
                {
                    min = d;
                    nearest = cell.Position;
                }
            }

            return nearest;
        }
    }
}
