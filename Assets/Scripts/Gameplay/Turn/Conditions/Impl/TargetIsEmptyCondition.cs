namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class TargetIsEmptyCondition : ITurnCondition
    {
        public string Type => "target_is_empty";

        public bool Evaluate(ActionContext context, ConditionParams parameters)
        {
            var cell = context.Grid.GetBoardCell(context.To);
            return cell != null && cell.OccupiedBy == null;
        }
    }
}
