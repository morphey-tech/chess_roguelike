using Project.Core.Core.Combat;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Tests.Combat.Passives
{
    /// <summary>
    /// Factory for creating test figures with passives.
    /// </summary>
    public sealed class FigureFactory
    {
        private readonly TriggerService _triggerService;

        public FigureFactory(TriggerService triggerService)
        {
            _triggerService = triggerService;
        }

        public Figure CreateWithPassive<T>(T passive, FigureConfig? config = null) where T : IPassive
        {
            var figure = Create(config);
            figure.AddPassive(passive);
            return figure;
        }

        public Figure Create(FigureConfig? config = null)
        {
            FigureStats stats = new(100,
                System.Array.Empty<AttackProfile>(), 10f, 0f, 0f);
            Figure figure = new(
                id: System.Threading.Interlocked.Increment(ref _idCounter),
                typeId: config?.Id ?? "test",
                movementId: "default",
                attackId: "default",
                turnPatternsId: "default",
                stats: stats,
                team: Team.Player,
                triggerService: _triggerService);

            return figure;
        }

        private static int _idCounter;
    }
}