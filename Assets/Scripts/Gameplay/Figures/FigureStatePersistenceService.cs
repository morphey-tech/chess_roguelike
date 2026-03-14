using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Combat;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Messages;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Сервис для сохранения и восстановления состояния фигур между сражениями.
    /// </summary>
    public sealed class FigureStatePersistenceService : IInitializable, IDisposable
    {
        private readonly PlayerRunStateService _runStateService;
        private readonly StageRunStateResetService _resetService;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly IFigureRegistry _figureRegistry;
        private readonly ISubscriber<string, StagePhaseMessage> _stagePhaseSubscriber;
        private readonly ILogger<FigureStatePersistenceService> _logger;

        private IDisposable? _disposable;

        [Inject]
        private FigureStatePersistenceService(
            PlayerRunStateService runStateService,
            StageRunStateResetService resetService,
            FigureSpawnService figureSpawnService,
            IFigureRegistry figureRegistry,
            ISubscriber<string, StagePhaseMessage> stagePhaseSubscriber,
            ILogService logService)
        {
            _runStateService = runStateService;
            _resetService = resetService;
            _figureSpawnService = figureSpawnService;
            _figureRegistry = figureRegistry;
            _stagePhaseSubscriber = stagePhaseSubscriber;
            _logger = logService.CreateLogger<FigureStatePersistenceService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _stagePhaseSubscriber.Subscribe(StagePhaseMessage.STAGE_COMPLETED, OnStageCompleted).AddTo(bag);
            _stagePhaseSubscriber.Subscribe(StagePhaseMessage.STAGE_STARTED, OnStageStarted).AddTo(bag);
            _disposable = bag.Build();
        }

        private void OnStageCompleted(StagePhaseMessage msg)
        {
            if (_runStateService.Current == null)
            {
                _logger.Warning("OnStageCompleted: runState is null");
                return;
            }

            // Собираем состояние всех фигур на доске
            IEnumerable<Figure> figures = _figureRegistry.GetAll().Where(f => f.Team == Team.Player);
            _resetService.CollectFigureStates(_runStateService.Current, figures);

            _logger.Info($"Collected states for {figures.Count()} figures");
        }

        private void OnStageStarted(StagePhaseMessage msg)
        {
            if (_runStateService.Current == null)
            {
                _logger.Warning("OnStageStarted: runState is null");
                return;
            }

            // Применяем сохранённое состояние к фигурам на доске
            IEnumerable<Figure> figures = _figureRegistry.GetAll().Where(f => f.Team == Team.Player);
            foreach (Figure figure in figures)
            {
                FigureState? state = _runStateService.Current.GetFigure(figure.Id.ToString());
                if (state != null)
                {
                    _figureSpawnService.ApplyFigureState(figure, state);
                    _logger.Debug($"Applied state to {figure}: HP={state.CurrentHp}/{state.MaxHp}");
                }
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
