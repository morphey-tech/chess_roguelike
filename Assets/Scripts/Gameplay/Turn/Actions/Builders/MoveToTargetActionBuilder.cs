namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class MoveToTargetActionBuilder : IActionBuilder
    {
        public string ActionType => "move_to_enemy";

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            return new Actions.Impl.MoveToTargetAction(
                builderContext.MovementService,
                builderContext.VisualPipeline,
                builderContext.AttackQueryService,
                builderContext.LogService);
        }
    }
}
