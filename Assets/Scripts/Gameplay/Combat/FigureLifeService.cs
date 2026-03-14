using Project.Core.Core.Combat;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Единая точка смерти: снять с доски, визуал, лут, сообщение.
    /// Combat — добавляет события в контекст. Direct — сразу вызывает презентеры.
    /// </summary>
    public sealed class FigureLifeService : IFigureLifeService
    {
        private readonly IFigurePresenter _figurePresenter;
        private readonly LootService _lootService;
        private readonly ILootPresenter _lootPresenter;
        private readonly IPublisher<string, FigureBoardMessage> _figureBoardPublisher;
        private readonly IPublisher<FigureDiedMessage> _figureDiedPublisher;
        private readonly BoardCapacityService _capacityService;

        [Inject]
        public FigureLifeService(
            IFigurePresenter figurePresenter,
            LootService lootService,
            ILootPresenter lootPresenter,
            IPublisher<string, FigureBoardMessage> figureBoardPublisher,
            IPublisher<FigureDiedMessage> figureDiedPublisher,
            BoardCapacityService capacityService)
        {
            _figurePresenter = figurePresenter;
            _lootService = lootService;
            _lootPresenter = lootPresenter;
            _figureDiedPublisher = figureDiedPublisher;
            _figureBoardPublisher = figureBoardPublisher;
            _capacityService = capacityService;
        }

        public void HandleDeathFromCombat(CombatEffectContext context, Figure unit, BoardCell cell)
        {
            cell.RemoveFigure();
            _figureBoardPublisher.Publish(FigureBoardMessage.REMOVED,
                new FigureBoardMessage(unit, cell.Position));
            context.AddVisualEvent(new DeathVisualEvent(unit.Id, null));

            if (!string.IsNullOrEmpty(unit.LootTableId))
            {
                LootResult? lootResult = context.LootService.Roll(unit.LootTableId);
                if (lootResult != null && !lootResult.IsEmpty)
                {
                    context.AddVisualEvent(new LootVisualEvent(cell.Position, lootResult));
                }
            }

            _figureDiedPublisher.Publish(new FigureDiedMessage(unit.Id, unit.Team,
                unit.LootTableId, fromCombat: true));
            if (unit.Team == Team.Player)
            {
                _capacityService.ReleaseByType(unit.TypeId);
            }
            context.ActionContext.LastAttackKilledTarget = true;
            context.Logger?.Info($"{unit} died!");
        }

        public void HandleDeathDomainOnly(Figure unit, BoardCell cell)
        {
            cell.RemoveFigure();
            _figureBoardPublisher.Publish(FigureBoardMessage.REMOVED,
                new FigureBoardMessage(unit, cell.Position));
            _figureDiedPublisher.Publish(new FigureDiedMessage(unit.Id, unit.Team,
                unit.LootTableId, fromCombat: true));
            if (unit.Team == Team.Player)
            {
                _capacityService.ReleaseByType(unit.TypeId);
            }
        }

        public async UniTask HandleDeathDirectAsync(Figure unit, BoardCell cell)
        {
            cell.RemoveFigure();
            _figureBoardPublisher.Publish(FigureBoardMessage.REMOVED, 
                new FigureBoardMessage(unit, cell.Position));

            _figurePresenter.HideFigureHealthBar(unit.Id);
            await _figurePresenter.PlayDeathEffectAsync(unit.Id);
            await _figurePresenter.RemoveFigureAsync(unit.Id);

            if (!string.IsNullOrEmpty(unit.LootTableId))
            {
                LootResult? lootResult = _lootService.Roll(unit.LootTableId);
                if (lootResult is { IsEmpty: false })
                {
                    await _lootPresenter.PresentAsync(new LootVisualContext(cell.Position, lootResult));
                }
            }

            _figureDiedPublisher.Publish(new FigureDiedMessage(unit.Id, unit.Team,
                unit.LootTableId, fromCombat: true));
            if (unit.Team == Team.Player)
            {
                _capacityService.ReleaseByType(unit.TypeId);
            }
        }
    }
}
