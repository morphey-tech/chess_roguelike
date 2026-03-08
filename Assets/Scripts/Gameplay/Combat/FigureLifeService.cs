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
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly IPublisher<FigureBoardRemovedMessage> _boardRemovedPublisher;
        private readonly BoardCapacityService _capacityService;

        [Inject]
        public FigureLifeService(
            IFigurePresenter figurePresenter,
            LootService lootService,
            ILootPresenter lootPresenter,
            IPublisher<FigureDeathMessage> deathPublisher,
            IPublisher<FigureBoardRemovedMessage> boardRemovedPublisher,
            BoardCapacityService capacityService)
        {
            _figurePresenter = figurePresenter;
            _lootService = lootService;
            _lootPresenter = lootPresenter;
            _deathPublisher = deathPublisher;
            _boardRemovedPublisher = boardRemovedPublisher;
            _capacityService = capacityService;
        }

        public void HandleDeathFromCombat(CombatEffectContext context, Figure unit, BoardCell cell)
        {
            context.Logger?.Info($"{unit} died!");

            cell?.RemoveFigure();
            _boardRemovedPublisher.Publish(new FigureBoardRemovedMessage(unit.Id, unit.Team));
            context.AddVisualEvent(new DeathVisualEvent(unit.Id, null));

            if (!string.IsNullOrEmpty(unit.LootTableId))
            {
                LootResult lootResult = context.LootService.Roll(unit.LootTableId);
                if (lootResult != null && !lootResult.IsEmpty && cell != null)
                    context.AddVisualEvent(new LootVisualEvent(cell.Position, lootResult));
            }

            _deathPublisher.Publish(new FigureDeathMessage(unit.Id, unit.Team, unit.LootTableId, fromCombat: true));
            if (unit.Team == Team.Player)
                _capacityService.ReleaseByType(unit.TypeId);
            context.ActionContext.LastAttackKilledTarget = true;
        }

        public void HandleDeathDomainOnly(Figure unit, BoardCell cell)
        {
            cell?.RemoveFigure();
            _boardRemovedPublisher.Publish(new FigureBoardRemovedMessage(unit.Id, unit.Team));
            _deathPublisher.Publish(new FigureDeathMessage(unit.Id, unit.Team, unit.LootTableId, fromCombat: true));
            if (unit.Team == Team.Player)
            {
                _capacityService.ReleaseByType(unit.TypeId);
            }
        }

        public async UniTask HandleDeathDirectAsync(Figure unit, BoardCell cell)
        {
            cell?.RemoveFigure();
            _boardRemovedPublisher.Publish(new FigureBoardRemovedMessage(unit.Id, unit.Team));

            _figurePresenter.HideFigureHealthBar(unit.Id);
            await _figurePresenter.PlayDeathEffectAsync(unit.Id);
            await _figurePresenter.RemoveFigureAsync(unit.Id);

            if (!string.IsNullOrEmpty(unit.LootTableId) && cell != null)
            {
                LootResult lootResult = _lootService.Roll(unit.LootTableId);
                if (lootResult is { IsEmpty: false } && _lootPresenter != null)
                    await _lootPresenter.PresentAsync(new LootVisualContext(cell.Position, lootResult));
            }

            _deathPublisher.Publish(new FigureDeathMessage(unit.Id, unit.Team, unit.LootTableId, fromCombat: true));
            if (unit.Team == Team.Player)
            {
                _capacityService.ReleaseByType(unit.TypeId);
            }
        }
    }
}
