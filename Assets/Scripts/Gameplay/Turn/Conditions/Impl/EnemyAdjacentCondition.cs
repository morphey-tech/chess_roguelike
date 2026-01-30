using Project.Gameplay.Gameplay.Attack;

namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class EnemyAdjacentCondition : ITurnCondition
    {
        public string Type => "enemy_adjacent";

        public bool Evaluate(ActionContext context, ConditionParams parameters)
        {
            foreach (var enemy in context.Enemies)
            {
                if (enemy.Team == context.Actor.Team)
                    continue;

                var enemyCell = context.Grid.FindFigure(enemy);
                if (enemyCell == null)
                    continue;

                int distance = AttackUtils.GetDistance(context.ActorPosition, enemyCell.Position);
                if (distance == 1)
                    return true;
            }

            return false;
        }
    }
}
