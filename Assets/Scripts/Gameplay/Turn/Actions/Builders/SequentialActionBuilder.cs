using System.Linq;

namespace Project.Gameplay.Gameplay.Turn.Actions.Builders
{
    public sealed class SequentialActionBuilder : IActionBuilder
    {
        public string ActionType => "sequential";

        private readonly ActionBuilderRegistry _builderRegistry;

        public SequentialActionBuilder(ActionBuilderRegistry builderRegistry)
        {
            _builderRegistry = builderRegistry;
        }

        public ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext)
        {
            string id = string.IsNullOrEmpty(config.Id) ? config.Type : config.Id;

            if (config.SubActions == null || config.SubActions.Length == 0)
                throw new System.Exception($"SequentialAction requires SubActions");

            var actions = config.SubActions.Select(subConfig =>
            {
                IActionBuilder subBuilder = _builderRegistry.GetBuilder(subConfig.Type);
                return subBuilder.Build(subConfig, builderContext);
            }).ToList();

            return new Impl.SequentialAction(id, actions);
        }
    }
}
