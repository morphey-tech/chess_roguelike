namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class HasTargetCondition : ITurnCondition
    {
        public string Type => "has_target";

        public bool Evaluate(TurnSelectionContext context, ConditionParams parameters)
        {
            return context.TargetPosition.HasValue;
        }
    }
}
