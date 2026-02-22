using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.ShrinkingZone.Messages;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Visual;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис применения урона от shrinking zone через правильные каналы
    /// </summary>
    public class ZoneDamageService : IDisposable
    {
        private readonly IFigureLifeService _figureLifeService;
        private readonly RunHolder _runHolder;
        private readonly VisualPipeline _visualPipeline;
        private readonly ICombatVisualPlanner _visualPlanner;
        private readonly ILogger<ZoneDamageService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private ZoneDamageService(
            IFigureLifeService figureLifeService,
            RunHolder runHolder,
            VisualPipeline visualPipeline,
            ICombatVisualPlanner visualPlanner,
            ISubscriber<UnitTakeZoneDamageMessage> damageSubscriber,
            ILogService logService)
        {
            _figureLifeService = figureLifeService;
            _runHolder = runHolder;
            _visualPipeline = visualPipeline;
            _visualPlanner = visualPlanner;
            _logger = logService.CreateLogger<ZoneDamageService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            damageSubscriber.Subscribe(OnZoneDamageTaken).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private async void OnZoneDamageTaken(UnitTakeZoneDamageMessage msg)
        {
            _logger.Debug($"[ZONE DAMAGE] Applying {msg.Damage} damage to target at ({msg.Position.Row},{msg.Position.Column})");

            // Находим фигуру на доске
            var grid = GetCurrentGrid();
            if (grid == null)
            {
                _logger.Warning("[ZONE DAMAGE] Grid is null, skipping damage");
                return;
            }

            BoardCell? cell = grid.GetBoardCell(msg.Position);
            if (cell.OccupiedBy == null)
            {
                _logger.Warning($"[ZONE DAMAGE] No figure at ({msg.Position.Row},{msg.Position.Column})");
                return;
            }

            Figure figure = cell.OccupiedBy;

            // Применяем урон через Stats
            bool died = figure.Stats.TakeDamage(msg.Damage);

            _logger.Debug($"[ZONE DAMAGE] {figure.Id} took {msg.Damage} damage, HP: {figure.Stats.CurrentHp}/{figure.Stats.MaxHp}, died: {died}");

            // Показываем визуал урона
            await ShowDamageVisual(figure.Id, msg.Damage);

            // Если умерла — обрабатываем смерть с визуалом
            if (died)
            {
                _logger.Info($"[ZONE DAMAGE] {figure.Id} died from zone damage");
                await _figureLifeService.HandleDeathDirectAsync(figure, cell);
            }
        }

        private async UniTask ShowDamageVisual(int figureId, float damage)
        {
            var damageEvent = new DamageVisualEvent(
                figureId,
                damage,
                isCritical: false,
                isDodged: false,
                damageType: "zone"
            );

            var plan = _visualPlanner.Build(null, new[] { (ICombatVisualEvent)damageEvent });
            foreach (var command in plan.Commands)
            {
                await _visualPipeline.PlayAsync(command);
            }
        }

        private BoardGrid? GetCurrentGrid()
        {
            return _runHolder.Current?.CurrentStage?.Grid;
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
