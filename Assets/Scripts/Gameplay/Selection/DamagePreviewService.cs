using Project.Core.Core.Combat;
using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack.Rules;
using Project.Gameplay.Gameplay.Bootstrap;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn;
using VContainer;
using IInitializable = VContainer.Unity.IInitializable;

namespace Project.Gameplay.Gameplay.Selection
{
    /// <summary>
    /// Shows damage preview on HP bar when hovering over enemy figure.
    /// Uses the same damage pipeline as real combat (in preview mode).
    /// </summary>
    public sealed class DamagePreviewService : IInitializable, IDisposable
    {
        private readonly IFigurePresenter _figurePresenter;
        private readonly RunHolder _runHolder;
        private readonly CombatResolver _combatResolver;
        private readonly AttackRuleService _attackRuleService;
        private readonly ISubscriber<string, FigureSelectMessage> _figureSelectSubscriber;
        private readonly ISubscriber<string, FigureBoardMessage> _figureBoardSubscriber;
        private readonly ISubscriber<FigureAttackMessage> _figureAttackSubscriber;
        private readonly ISubscriber<FigureHoverChangedMessage> _hoverChangedSubscriber;
        private readonly ISubscriber<TurnChangedMessage> _turnChangedSubscriber;
        private readonly ILogger<DamagePreviewService> _logger;

        private int? _hoveredFigureId;
        private int? _selectedFriendlyFigureId;
        private int? _lastPreviewFigureId;
        private IDisposable _disposable;

        [Inject]
        private DamagePreviewService(
            IFigurePresenter figurePresenter,
            RunHolder runHolder,
            CombatResolver combatResolver,
            AttackRuleService attackRuleService,
            ISubscriber<string, FigureSelectMessage> figureSelectSubscriber,
            ISubscriber<string, FigureBoardMessage> figureBoardSubscriber,
            ISubscriber<FigureAttackMessage> figureAttackSubscriber,
            ISubscriber<FigureHoverChangedMessage> hoverChangedSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            ILogService logService)
        {
            _figurePresenter = figurePresenter;
            _runHolder = runHolder;
            _combatResolver = combatResolver;
            _attackRuleService = attackRuleService;
            _figureSelectSubscriber = figureSelectSubscriber;
            _figureBoardSubscriber = figureBoardSubscriber;
            _figureAttackSubscriber = figureAttackSubscriber;
            _hoverChangedSubscriber = hoverChangedSubscriber;
            _turnChangedSubscriber = turnChangedSubscriber;
            _logger = logService.CreateLogger<DamagePreviewService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _figureSelectSubscriber.Subscribe(FigureSelectMessage.SELECTED, OnFigureSelected).AddTo(bag);
            _figureSelectSubscriber.Subscribe(FigureSelectMessage.DESELECTED, OnFigureDeselected).AddTo(bag);
            _figureBoardSubscriber.Subscribe(FigureBoardMessage.REMOVED, OnFigureBoardRemoved).AddTo(bag);
            _hoverChangedSubscriber.Subscribe(OnHoverChanged).AddTo(bag);
            _figureAttackSubscriber.Subscribe(OnAttackStarted).AddTo(bag);
            _turnChangedSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _disposable = bag.Build();
        }

        private void OnFigureSelected(FigureSelectMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureSelected: FigureId={message.Figure?.Id}, Team={message.Figure?.Team}");
            _selectedFriendlyFigureId = message.Figure?.Team == Team.Player 
                ? message.Figure?.Id : null;
            UpdateDamagePreview();
        }

        private void OnFigureDeselected(FigureSelectMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureDeselected: FigureId={message.Figure?.Id}");
            
            if (message.Figure == null || _selectedFriendlyFigureId == message.Figure.Id)
            {
                _selectedFriendlyFigureId = null;
                _logger.Debug("[DamagePreview] Friendly selection cleared");
            }

            UpdateDamagePreview();
        }

        private void OnFigureBoardRemoved(FigureBoardMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureBoardRemoved: FigureId={message.Figure.Id}");
            
            if (_lastPreviewFigureId == message.Figure.Id)
            {
                _figurePresenter.SetDamagePreview(_lastPreviewFigureId.Value, null);
                _lastPreviewFigureId = null;
            }
            
            if (_selectedFriendlyFigureId == message.Figure.Id)
            {
                _selectedFriendlyFigureId = null;
                UpdateDamagePreview();
            }
        }
        
        private void OnHoverChanged(FigureHoverChangedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnHoverChanged: FigureId={message.FigureId}");
            _hoveredFigureId = message.FigureId;
            UpdateDamagePreview();
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnTurnChanged: NewTurn={message.TurnNumber}");
            _selectedFriendlyFigureId = null;
            _hoveredFigureId = null;
            UpdateDamagePreview();
        }


        private void OnAttackStarted(FigureAttackMessage message)
        {
            _logger.Debug($"[DamagePreview] OnAttackStarted: Attacker={message.ActorID}," +
                          $" Target={message.TargetID}");
            
            _figurePresenter.SetDamagePreview(message.TargetID, null);
            if (_lastPreviewFigureId == message.TargetID)
            {
                _lastPreviewFigureId = null;
            }
        }

        private void UpdateDamagePreview()
        {
            _logger.Debug($"[DamagePreview] UpdatePreview: hovered={_hoveredFigureId?.ToString() ?? "null"}, selected={_selectedFriendlyFigureId?.ToString() ?? "null"}, lastPreview={_lastPreviewFigureId?.ToString() ?? "null"}");

            if (_lastPreviewFigureId.HasValue && _lastPreviewFigureId != _hoveredFigureId)
            {
                _logger.Debug($"[DamagePreview] Clearing old preview for FigureId={_lastPreviewFigureId}");
                _figurePresenter.SetDamagePreview(_lastPreviewFigureId.Value, null);
                _lastPreviewFigureId = null;
            }

            if (!_hoveredFigureId.HasValue || !_selectedFriendlyFigureId.HasValue)
            {
                _logger.Debug("[DamagePreview] No hover or selection — preview cleared");
                return;
            }

            if (!TryGetValidPreview(out float damage))
            {
                _logger.Debug("[DamagePreview] No valid preview — cleared");
                _figurePresenter.SetDamagePreview(_hoveredFigureId.Value, null);
                _lastPreviewFigureId = null;
                return;
            }

            _figurePresenter.SetDamagePreview(_hoveredFigureId.Value, damage);
            _lastPreviewFigureId = _hoveredFigureId;
            _logger.Debug($"[DamagePreview] Preview SET for FigureId={_hoveredFigureId}: DMG={damage}");
        }

        /// <summary>
        /// Tries to get valid damage preview. Returns true if preview is available.
        /// </summary>
        private bool TryGetValidPreview(out float damage)
        {
            damage = 0f;
            BoardGrid? grid = _runHolder.Current?.CurrentStage?.Grid;
            if (grid == null)
            {
                _logger.Warning("[DamagePreview] Grid is null!");
                return false;
            }

            Figure? attacker = grid.GetFigureById(_selectedFriendlyFigureId.Value);
            Figure? target = grid.GetFigureById(_hoveredFigureId.Value);

            if (attacker == null)
            {
                _logger.Warning($"[DamagePreview] Attacker not found: Id={_selectedFriendlyFigureId}");
                return false;
            }

            if (target == null)
            {
                _logger.Warning($"[DamagePreview] Target not found: Id={_hoveredFigureId}");
                return false;
            }

            // Не показываем превью для своих фигур
            if (target.Team == attacker.Team)
            {
                _logger.Debug($"[DamagePreview] Target is friendly ({target.Team}) - clearing preview");
                return false;
            }

            _logger.Debug($"[DamagePreview] Attacker={attacker.Id}, Target={target.Id}");

            BoardCell? attackerCell = grid.FindFigure(attacker);
            BoardCell? targetCell = grid.FindFigure(target);

            if (attackerCell == null || targetCell == null)
            {
                _logger.Warning("[DamagePreview] AttackerCell or TargetCell is null!");
                return false;
            }

            AttackRuleContext attackContext = new(
                attacker,
                target,
                attackerCell.Position,
                targetCell.Position,
                grid);

            if (!_attackRuleService.CanAttack(attackContext))
            {
                _logger.Debug($"[DamagePreview] Attack not valid — clearing preview for FigureId={_hoveredFigureId}");
                return false;
            }

            damage = _combatResolver.CalculatePreviewDamage(
                attacker,
                target,
                grid);

            _logger.Debug($"[DamagePreview] CombatResolver: Final={damage}");

            if (damage <= 0)
            {
                return false;
            }

            return true;
        }

        void IDisposable.Dispose()
        {
            _logger.Debug("[DamagePreviewService] Disposing...");

            if (_lastPreviewFigureId.HasValue)
            {
                _figurePresenter.SetDamagePreview(_lastPreviewFigureId.Value, null);
                _lastPreviewFigureId = null;
            }

            _disposable?.Dispose();
            _logger.Debug("[DamagePreviewService] Disposed");
        }

    }
}
