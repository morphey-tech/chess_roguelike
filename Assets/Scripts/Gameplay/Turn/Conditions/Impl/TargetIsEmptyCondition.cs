namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class TargetIsEmptyCondition : ITurnCondition
    {
        public string Type => "target_is_empty";

        public bool Evaluate(TurnSelectionContext context, ConditionParams parameters)
        {
            if (!context.TargetPosition.HasValue)
                return false;

            var cell = context.Grid.GetBoardCell(context.TargetPosition.Value);
            return cell != null && cell.OccupiedBy == null;
        }
    }
}
