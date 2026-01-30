namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    public sealed class AlwaysTrueCondition : ITurnCondition
    {
        public string Type => "always";

        public bool Evaluate(ActionContext context, ConditionParams parameters)
        {
            return true;
        }
    }
}
