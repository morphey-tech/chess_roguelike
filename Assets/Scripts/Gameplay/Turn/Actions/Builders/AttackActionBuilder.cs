namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class AttackActionBuilder : IActionBuilder
    {
        public string ActionType => "attack";

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            string id = string.IsNullOrEmpty(config.Id) ? config.Type : config.Id;
            return new Actions.Impl.AttackAction(
                id,
                builderContext.AttackFactory,
                builderContext.AttackResolver,
                builderContext.CombatResolver,
                builderContext.VisualPlanner,
                builderContext.Passives,
                builderContext.VisualPipeline,
                builderContext.DeathPublisher,
                builderContext.LootService,
                builderContext.DamageApplier,
                builderContext.FigureLifeService,
                builderContext.ContextAccessor,
                builderContext.AttackQueryService,
                builderContext.LogService.CreateLogger<Actions.Impl.AttackAction>());
        }
    }
}
