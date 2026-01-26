namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class TargetIsEnemyCondition : ITurnCondition
    {
        public string Type => "target_is_enemy";

        public bool Evaluate(TurnSelectionContext context, ConditionParams parameters)
        {
            if (!context.TargetPosition.HasValue)
                return false;

            var cell = context.Grid.GetBoardCell(context.TargetPosition.Value);
            var figure = cell?.OccupiedBy;
            
            return figure != null && figure.Team != context.Actor.Team;
        }
    }
}
