using System.Collections.Generic;
using Project.Gameplay.Gameplay.Visual.Commands;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    public sealed class CombatVisualPlanner : ICombatVisualPlanner
    {
        private readonly Dictionary<System.Type, IVisualEventMapper> _mappers;

        public CombatVisualPlanner(IEnumerable<IVisualEventMapper> mappers)
        {
            _mappers = new Dictionary<System.Type, IVisualEventMapper>();
            foreach (IVisualEventMapper mapper in mappers)
            {
                _mappers[mapper.EventType] = mapper;
            }
        }

        public VisualCombatPlan Build(CombatResult result, IReadOnlyList<ICombatVisualEvent> visualEvents)
        {
            var commands = new List<IVisualCommand>();

            if (visualEvents != null && visualEvents.Count > 0)
            {
                foreach (ICombatVisualEvent visualEvent in visualEvents)
                {
                    System.Type eventType = visualEvent.GetType();
                    if (_mappers.TryGetValue(eventType, out IVisualEventMapper mapper))
                    {
                        commands.AddRange(mapper.Map(visualEvent));
                    }
                }
            }

            return new VisualCombatPlan(commands);
        }
    }
}
