namespace Project.Gameplay.Gameplay.Turn.Conditions
{
    public interface ITurnCondition
    {
        string Type { get; }
        bool Evaluate(ActionContext context, ConditionParams parameters);
    }
}
