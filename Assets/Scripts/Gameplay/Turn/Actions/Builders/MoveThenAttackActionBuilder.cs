namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class MoveThenAttackActionBuilder : IActionBuilder
    {
        public string ActionType => "move_then_attack";

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            string id = string.IsNullOrEmpty(config.Id) ? config.Type : config.Id;

            // Build sub-actions
            var moveConfig = new ActionConfig { Type = "move", Id = $"{id}.move" };
            var attackConfig = new ActionConfig { Type = "attack", Id = $"{id}.attack" };

            var moveBuilder = new MoveActionBuilder();
            var attackBuilder = new AttackActionBuilder();

            var moveAction = moveBuilder.Build(moveConfig, builderContext);
            var attackAction = attackBuilder.Build(attackConfig, builderContext);

            return new Actions.Impl.MoveThenAttackAction(
                id,
                builderContext.MovementService,
                builderContext.AttackQueryService,
                moveAction,
                attackAction);
        }
    }
}
