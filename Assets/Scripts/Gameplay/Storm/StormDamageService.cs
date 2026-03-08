using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис применения урона от shrinking zone через правильные каналы
    /// </summary>
    public class StormDamageService : IDisposable
    {
        private readonly IFigureLifeService _figureLifeService;
        private readonly RunHolder _runHolder;
        private readonly VisualPipeline _visualPipeline;
        private readonly ICombatVisualPlanner _visualPlanner;
        private readonly ILogger<StormDamageService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private StormDamageService(
            IFigureLifeService figureLifeService,
            RunHolder runHolder,
            VisualPipeline visualPipeline,
            ICombatVisualPlanner visualPlanner,
            ISubscriber<FigureTakeStormDamageMessage> damageSubscriber,
            ILogService logService)
        {
            _figureLifeService = figureLifeService;
            _runHolder = runHolder;
            _visualPipeline = visualPipeline;
            _visualPlanner = visualPlanner;
            _logger = logService.CreateLogger<StormDamageService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            damageSubscriber.Subscribe(OnStormDamageTaken).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Debug("ZoneDamageService initialized and subscribed to FigureTakeZoneDamageMessage");
        }

        private void OnStormDamageTaken(FigureTakeStormDamageMessage msg)
        {
            HandleStormDamage(msg).Forget();
        }

        private async UniTaskVoid HandleStormDamage(FigureTakeStormDamageMessage msg)
        {
            _logger.Debug($"Received FigureTakeZoneDamageMessage: target={msg.Target}, damage={msg.Damage}, position=({msg.Position.Row},{msg.Position.Column})");

            BoardGrid? grid = GetCurrentGrid();
            if (grid == null)
            {
                _logger.Warning("Grid is null, skipping damage");
                return;
            }

            BoardCell? cell = grid.GetBoardCell(msg.Position);
            if (cell.OccupiedBy == null)
            {
                _logger.Warning($"No figure at ({msg.Position.Row},{msg.Position.Column})");
                return;
            }

            Figure figure = cell.OccupiedBy;
            _logger.Debug($"Found figure {figure.Id} at ({msg.Position.Row},{msg.Position.Column})");

            bool died = figure.Stats.TakeDamage(msg.Damage);
            _logger.Debug($"{figure.Id} took {msg.Damage} damage, HP: {figure.Stats.CurrentHp.Value}/{figure.Stats.MaxHp}, died: {died}");

            await ShowDamageVisual(figure.EntityId, msg.Damage);
            if (died)
            {
                _logger.Info($"{figure.Id} died from zone damage");
                await _figureLifeService.HandleDeathDirectAsync(figure, cell);
            }
        }

        private async UniTask ShowDamageVisual(int figureId, float damage)
        {
            DamageVisualEvent damageEvent = new(
                figureId,
                damage,
                isCritical: false,
                isDodged: false,
                damageType: "storm"
            );

            VisualCombatPlan plan = _visualPlanner.Build(null, new[] { (ICombatVisualEvent)damageEvent });
            foreach (IVisualCommand? command in plan.Commands)
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
