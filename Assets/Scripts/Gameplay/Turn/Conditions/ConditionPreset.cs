namespace Project.Gameplay.Gameplay.Turn.Conditions
{
    public sealed class ConditionPreset
    {
        public string Id { get; }
        public ITurnCondition Condition { get; }
        public ConditionParams DefaultParams { get; }

        public ConditionPreset(string id, ITurnCondition condition, ConditionParams defaultParams)
        {
            Id = id;
            Condition = condition;
            DefaultParams = defaultParams;
        }
    }
}