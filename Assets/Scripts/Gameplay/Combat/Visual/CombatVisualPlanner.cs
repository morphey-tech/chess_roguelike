using System.Collections.Generic;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    public sealed class CombatVisualPlanner : ICombatVisualPlanner
    {
        private readonly Dictionary<System.Type, IVisualEventMapper> _mappers;

        [Inject]
        private CombatVisualPlanner(IEnumerable<IVisualEventMapper> mappers)
        {
            _mappers = new Dictionary<System.Type, IVisualEventMapper>();
            foreach (IVisualEventMapper mapper in mappers)
            {
                _mappers[mapper.EventType] = mapper;
            }
        }

        public VisualCombatPlan Build(CombatResult result, IReadOnlyList<ICombatVisualEvent> visualEvents)
        {
            List<IVisualCommand> commands = new();

            if (visualEvents is { Count: > 0 })
            {
                List<ICombatVisualEvent> sorted = new(visualEvents);
                sorted.Sort((a, b) => a.Stage != b.Stage ? a.Stage.CompareTo(b.Stage) : a.OrderInStage.CompareTo(b.OrderInStage));

                foreach (ICombatVisualEvent visualEvent in sorted)
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
