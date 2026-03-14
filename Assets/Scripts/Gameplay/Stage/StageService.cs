using System;
using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Threat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Stage.Analysis;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Stage
{
    public class StageService : IInitializable, IDisposable
    {
        private readonly IStageQueryService _query;
        private readonly IStageHighlightRenderer _renderer;
        private readonly ISubscriber<string, FigureSelectMessage> _figureSelectSubscriber;
        private readonly ISubscriber<TurnChangedMessage> _turnSubscriber;
        private readonly ISubscriber<string, BonusMoveMessage> _bonusMoveSubscriber;
        private readonly ISubscriber<FigureDiedMessage> _figureDiedSubscriber;
        private readonly ISubscriber<string, FigureBoardMessage> _figureBoardPublisher;
        private readonly StageAnalysisService _analysisService;
        private readonly ThreatMapService _threatMapService;
        private readonly MovementService _movementService;
        private readonly ILogger<StageService> _logger;

        private IDisposable _disposable = null!;
        private StageMode _mode;

        [Inject]
        private StageService(
            IStageQueryService query,
            IStageHighlightRenderer renderer,
            ISubscriber<string, FigureSelectMessage> figureSelectSubscriber,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ISubscriber<string, BonusMoveMessage> bonusMoveSubscriber,
            ISubscriber<FigureDiedMessage> figureDiedSubscriber,
            ISubscriber<string, FigureBoardMessage> figureBoardPublisher,
            StageAnalysisService analysisService,
            ThreatMapService threatMapService,
            MovementService movementService,
            ILogService logService)
        {
            _query = query;
            _renderer = renderer;
            _figureSelectSubscriber = figureSelectSubscriber;
            _turnSubscriber = turnSubscriber;
            _bonusMoveSubscriber = bonusMoveSubscriber;
            _figureDiedSubscriber = figureDiedSubscriber;
            _figureBoardPublisher = figureBoardPublisher;
            _analysisService = analysisService;
            _threatMapService = threatMapService;
            _movementService = movementService;
            _logger = logService.CreateLogger<StageService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _figureSelectSubscriber.Subscribe(FigureSelectMessage.SELECTED, OnFigureSelected).AddTo(bag);
            _figureSelectSubscriber.Subscribe(FigureSelectMessage.DESELECTED, OnFigureDeselected).AddTo(bag);
            _turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _bonusMoveSubscriber.Subscribe(BonusMoveMessage.STARTED, OnBonusMoveStarted).AddTo(bag);
            _bonusMoveSubscriber.Subscribe(BonusMoveMessage.COMPLETED, OnBonusMoveCompleted).AddTo(bag);
            _figureDiedSubscriber.Subscribe(OnFigureDied).AddTo(bag);
            _figureBoardPublisher.Subscribe(FigureBoardMessage.SPAWNED, OnFigureSpawned).AddTo(bag);
            _figureBoardPublisher.Subscribe(FigureBoardMessage.MOVED, OnFigureMoved).AddTo(bag);
            _disposable = bag.Build();
        }

        private void OnFigureDied(FigureDiedMessage message)
        {
            _threatMapService.RemoveFigureThreatById(message.FigureId, message.Team);
            _threatMapService.Invalidate();
        }

        private void OnFigureSpawned(FigureBoardMessage message)
        {
            _threatMapService.UpdateFigureThreat(message.Figure);
            _threatMapService.Invalidate();
        }

        private void OnFigureMoved(FigureBoardMessage message)
        {
            _threatMapService.UpdateFigureThreat(message.Figure);
            _threatMapService.Invalidate();
        }

        private void OnBonusMoveCompleted(BonusMoveMessage message)
        {
            _mode = StageMode.Normal;
            _renderer.Clear();
            _logger.Debug($"Bonus move completed for {message.Actor}, clearing highlights");
        }

        private void OnBonusMoveStarted(BonusMoveMessage message)
        {
            _mode = StageMode.BonusMove;
            IReadOnlyCollection<GridPosition> moveTargets = _query.GetBonusMoveTargets();
            _renderer.ShowMovesOnly(moveTargets);
            _logger.Debug($"Bonus move started for {message.Actor}, showing highlights");
        }

        private void OnFigureSelected(FigureSelectMessage message)
        {
            if (_mode == StageMode.BonusMove)
            {
                return;
            }

            if (message.Figure != null)
            {
                StageActorAnalysis analysis = _analysisService.AnalyzeActor(message.Figure, message.Position);
                _renderer.Show(analysis);
                message.Figure.EnsureComponent(new SelectTag());
                _logger.Debug($"Figure {message.Figure.Id} selected at ({message.Position.Row}, {message.Position.Column})");
            }
            else
            {
                _renderer.Clear();
                _logger.Debug("Selection cleared");
            }
        }

        private void OnFigureDeselected(FigureSelectMessage message)
        {
            message.Figure.Del<SelectTag>();
            if (_mode != StageMode.BonusMove)
            {
                _renderer.Clear();
            }
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _mode = StageMode.Normal;
            _renderer.Clear();
            _logger.Info($"Turn {message.TurnNumber}: {message.CurrentTeam}'s turn");
        }

        void IDisposable.Dispose()
        {
            _disposable?.Dispose();
        }

        private enum StageMode
        {
            Normal,
            BonusMove
        }
    }
}
