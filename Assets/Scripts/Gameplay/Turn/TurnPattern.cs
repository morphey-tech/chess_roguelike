using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Steps;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPattern
    {
        public string Id { get; }
        public int Priority { get; }
        public ITurnCondition Condition { get; }
        public ConditionParams ConditionParams { get; }
        public ITurnStep Step { get; }

        public TurnPattern(
            string id,
            int priority,
            ITurnCondition condition,
            ConditionParams conditionParams,
            ITurnStep step)
        {
            Id = id;
            Priority = priority;
            Condition = condition;
            ConditionParams = conditionParams ?? ConditionParams.Empty;
            Step = step;
        }

        public bool Evaluate(TurnSelectionContext context)
        {
            return Condition.Evaluate(context, ConditionParams);
        }
    }
}
