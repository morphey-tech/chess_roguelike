using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.Gameplay.Bootstrap;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;
using IInitializable = VContainer.Unity.IInitializable;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис применения урона от shrinking zone через правильные каналы
    /// </summary>
    public class StormDamageService : IInitializable, IDisposable
    {
        private readonly IFigureLifeService _figureLifeService;
        private readonly RunHolder _runHolder;
        private readonly VisualPipeline _visualPipeline;
        private readonly ICombatVisualPlanner _visualPlanner;
        private readonly ISubscriber<string, StormMessage> _stormSubscriber;
        private readonly ILogger<StormDamageService> _logger;

        private IDisposable _disposable = null!;

        [Inject]
        private StormDamageService(
            IFigureLifeService figureLifeService,
            RunHolder runHolder,
            VisualPipeline visualPipeline,
            ICombatVisualPlanner visualPlanner,
            ISubscriber<string, StormMessage> stormSubscriber,
            ILogService logService)
        {
            _figureLifeService = figureLifeService;
            _runHolder = runHolder;
            _visualPipeline = visualPipeline;
            _visualPlanner = visualPlanner;
            _stormSubscriber = stormSubscriber;
            _logger = logService.CreateLogger<StormDamageService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _stormSubscriber.Subscribe(StormMessage.FIGURE_DAMAGE, OnStormDamageTaken).AddTo(bag);
            _disposable = bag.Build();
        }

        private void OnStormDamageTaken(StormMessage msg)
        {
            HandleStormDamage(msg).Forget();
        }

        private async UniTaskVoid HandleStormDamage(StormMessage msg)
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

            await ShowDamageVisual(figure.Id, msg.Damage);
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
            _disposable?.Dispose();
        }
    }
}
