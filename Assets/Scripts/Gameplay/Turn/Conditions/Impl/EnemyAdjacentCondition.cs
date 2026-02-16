using Project.Gameplay.Gameplay.Attack;

namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    /// <summary>
    /// Checks if there is an enemy adjacent to the actor AND the target cell contains an enemy.
    /// This ensures the attack is directed at a specific target, not just any adjacent enemy.
    /// </summary>
    public sealed class EnemyAdjacentCondition : ITurnCondition
    {
        public string Type => "enemy_adjacent";

        public bool Evaluate(ActionContext context, ConditionParams parameters)
        {
            // First check: is there an enemy at the target position?
            var targetCell = context.Grid.GetBoardCell(context.To);
            var targetFigure = targetCell?.OccupiedBy;
            
            // Target must contain an enemy
            if (targetFigure == null || targetFigure.Team == context.Actor.Team)
                return false;
            
            // Second check: is that enemy adjacent to the actor?
            int distance = AttackUtils.GetDistance(context.ActorPosition, targetCell.Position);
            return distance == 1;
        }
    }
}
