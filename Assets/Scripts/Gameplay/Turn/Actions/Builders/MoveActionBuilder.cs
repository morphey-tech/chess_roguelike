using Project.Gameplay.Gameplay.Turn.Actions.Impl;

namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class MoveActionBuilder : IActionBuilder
    {
        public string ActionType => "move";

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            string id = string.IsNullOrEmpty(config.Id) ? config.Type : config.Id;
            return new MoveAction(id, builderContext.MovementService, builderContext.VisualPipeline);
        }
    }
}
