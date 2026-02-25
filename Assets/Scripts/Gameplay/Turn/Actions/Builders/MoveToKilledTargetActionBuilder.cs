using Project.Gameplay.Gameplay.Turn.Actions.Impl;

namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class MoveToKilledTargetActionBuilder : IActionBuilder
    {
        public string ActionType => "move_to_killed";

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            return new MoveToKilledTargetAction(
                builderContext.MovementService,
                builderContext.VisualPipeline,
                builderContext.LogService);
        }
    }
}
