namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class HasTargetCondition : ITurnCondition
    {
        public string Type => "has_target";

        public bool Evaluate(ActionContext context, ConditionParams parameters)
        {
            // In ActionContext, To is always set
            return true;
        }
    }
}
