namespace Project.Gameplay.Gameplay.Turn.Conditions.Impl
{
    /// <summary>
    /// Checks if the actor can move to the target position according to movement rules.
    /// </summary>
    public sealed class CanMoveCondition : ITurnCondition
    {
        public string Type => "can_move";

        public bool Evaluate(TurnSelectionContext context, ConditionParams parameters)
        {
            if (!context.TargetPosition.HasValue)
                return false;

            if (context.MovementService == null)
                return false;

            return context.MovementService.CanMove(
                context.Actor, 
                context.ActorPosition, 
                context.TargetPosition.Value);
        }
    }
}
