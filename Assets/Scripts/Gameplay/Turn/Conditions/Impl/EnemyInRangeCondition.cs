using Project.Gameplay.Gameplay.Attack;

namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class EnemyInRangeCondition : ITurnCondition
    {
        public string Type => "enemy_in_range";

        public bool Evaluate(TurnSelectionContext context, ConditionParams parameters)
        {
            int range = parameters.GetInt("range", context.Actor.Stats.AttackRange);

            foreach (var enemy in context.Enemies)
            {
                if (enemy.Team == context.Actor.Team)
                    continue;

                var enemyCell = context.Grid.FindFigure(enemy);
                if (enemyCell == null)
                    continue;

                int distance = AttackUtils.GetDistance(context.ActorPosition, enemyCell.Position);
                if (distance <= range)
                    return true;
            }

            return false;
        }
    }
}
