namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class TargetIsEnemyCondition : ITurnCondition
    {
        public string Type => "target_is_enemy";

        public bool Evaluate(ActionContext context, ConditionParams parameters)
        {
            var cell = context.Grid.GetBoardCell(context.To);
            var figure = cell?.OccupiedBy;
            
            return figure != null && figure.Team != context.Actor.Team;
        }
    }
}
