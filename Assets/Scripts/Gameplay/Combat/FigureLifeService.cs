using Cysharp.Threading.Tasks;
using MessagePipe;
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

        [Inject]
        public FigureLifeService(
            IFigurePresenter figurePresenter,
            LootService lootService,
            ILootPresenter lootPresenter,
            IPublisher<FigureDeathMessage> deathPublisher)
        {
            _figurePresenter = figurePresenter;
            _lootService = lootService;
            _lootPresenter = lootPresenter;
            _deathPublisher = deathPublisher;
        }

        public void HandleDeathFromCombat(CombatEffectContext context, Figure unit, BoardCell cell)
        {
            context.Logger?.Info($"{unit} died!");

            cell?.RemoveFigure();
            context.AddVisualEvent(new DeathVisualEvent(unit.Id, null));

            if (!string.IsNullOrEmpty(unit.LootTableId))
            {
                LootResult lootResult = context.LootService.Roll(unit.LootTableId);
                if (lootResult != null && !lootResult.IsEmpty && cell != null)
                    context.AddVisualEvent(new LootVisualEvent(cell.Position, lootResult));
            }

            _deathPublisher.Publish(new FigureDeathMessage(unit.Id, unit.Team, unit.LootTableId, fromCombat: true));
            context.ActionContext.LastAttackKilledTarget = true;
        }

        public void HandleDeathDomainOnly(Figure unit, BoardCell cell)
        {
            cell?.RemoveFigure();
            _deathPublisher.Publish(new FigureDeathMessage(unit.Id, unit.Team, unit.LootTableId, fromCombat: true));
        }

        public async UniTask HandleDeathDirectAsync(Figure unit, BoardCell cell)
        {
            cell?.RemoveFigure();

            _figurePresenter.HideFigureHealthBar(unit.Id);
            await _figurePresenter.PlayDeathEffectAsync(unit.Id);
            await _figurePresenter.RemoveFigureAsync(unit.Id);

            if (!string.IsNullOrEmpty(unit.LootTableId) && cell != null)
            {
                LootResult lootResult = _lootService.Roll(unit.LootTableId);
                if (lootResult != null && !lootResult.IsEmpty && _lootPresenter != null)
                    await _lootPresenter.PresentAsync(new LootVisualContext(cell.Position, lootResult));
            }

            _deathPublisher.Publish(new FigureDeathMessage(unit.Id, unit.Team, unit.LootTableId, fromCombat: true));
        }
    }
}
