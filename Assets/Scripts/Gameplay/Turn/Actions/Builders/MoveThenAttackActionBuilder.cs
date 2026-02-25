using Project.Gameplay.Gameplay.Turn.Actions.Impl;

namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class MoveThenAttackActionBuilder : IActionBuilder
    {
        public string ActionType => "move_then_attack";

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            string id = string.IsNullOrEmpty(config.Id) ? config.Type : config.Id;

            // Build sub-actions
            ActionConfig moveConfig = new() { Type = "move", Id = $"{id}.move" };
            ActionConfig attackConfig = new() { Type = "attack", Id = $"{id}.attack" };

            MoveActionBuilder moveBuilder = new();
            AttackActionBuilder attackBuilder = new();

            ICombatAction moveAction = moveBuilder.Build(moveConfig, builderContext);
            ICombatAction attackAction = attackBuilder.Build(attackConfig, builderContext);

            return new MoveThenAttackAction(
                id,
                builderContext.MovementService,
                builderContext.AttackQueryService,
                moveAction,
                attackAction);
        }
    }
}
