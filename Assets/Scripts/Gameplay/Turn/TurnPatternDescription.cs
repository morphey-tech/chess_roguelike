using Project.Gameplay.Gameplay.Turn.Actions;
using Project.Gameplay.Gameplay.Turn.Conditions;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPatternDescription
    {
        public string Id { get; }
        public int Priority { get; }
        public ITurnCondition Condition { get; }
        public ConditionParams ConditionParams { get; }
        public ICombatAction Action { get; }

        public TurnPatternDescription(
            string id,
            int priority,
            ITurnCondition condition,
            ConditionParams? conditionParams,
            ICombatAction action)
        {
            Id = id;
            Priority = priority;
            Condition = condition;
            ConditionParams = conditionParams ?? ConditionParams.Empty;
            Action = action;
        }

        public bool Evaluate(ActionContext context)
        {
            return Condition.Evaluate(context, ConditionParams);
        }
    }
}
