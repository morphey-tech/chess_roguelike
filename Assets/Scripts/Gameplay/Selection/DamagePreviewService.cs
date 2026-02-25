using System;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack.Rules;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.Selection
{
    /// <summary>
    /// Shows damage preview on HP bar when hovering over enemy figure.
    /// Uses the same damage pipeline as real combat (in preview mode).
    /// </summary>
    public sealed class DamagePreviewService : IDisposable
    {
        private readonly IFigurePresenter _figurePresenter;
        private readonly RunHolder _runHolder;
        private readonly AttackRuleService _attackRuleService;
        private readonly IDamagePipeline _damagePipeline;
        private readonly IDisposable _subscriptions;
        private readonly ILogger<DamagePreviewService> _logger;

        private int? _hoveredFigureId;
        private int? _selectedFriendlyFigureId;
        private int? _lastPreviewFigureId;

        [Inject]
        private DamagePreviewService(
            IFigurePresenter figurePresenter,
            RunHolder runHolder,
            AttackRuleService attackRuleService,
            IDamagePipeline damagePipeline,
            ISubscriber<FigureSelectedMessage> selectedSubscriber,
            ISubscriber<FigureDeselectedMessage> deselectedSubscriber,
            ISubscriber<FigureHoverChangedMessage> hoverChangedSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            ISubscriber<FigureBoardRemovedMessage> figureRemovedSubscriber,
            ISubscriber<FigureAttackStartedMessage> attackStartedSubscriber,
            ILogService logService)
        {
            _figurePresenter = figurePresenter;
            _runHolder = runHolder;
            _attackRuleService = attackRuleService;
            _damagePipeline = damagePipeline;
            _logger = logService.CreateLogger<DamagePreviewService>();

            _logger.Debug("[DamagePreviewService] Initialized");

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            selectedSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            deselectedSubscriber.Subscribe(OnFigureDeselected).AddTo(bag);
            hoverChangedSubscriber.Subscribe(OnHoverChanged).AddTo(bag);
            turnChangedSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            figureRemovedSubscriber.Subscribe(OnFigureBoardRemoved).AddTo(bag);
            attackStartedSubscriber.Subscribe(OnAttackStarted).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureSelected: FigureId={message.Figure?.Id}, Team={message.Figure?.Team}");
            _selectedFriendlyFigureId = message.Figure?.Team == Team.Player 
                ? message.Figure?.Id : null;
            UpdateDamagePreview();
        }

        private void OnFigureDeselected(FigureDeselectedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureDeselected: FigureId={message.Figure?.Id}");
            
            if (message.Figure == null || _selectedFriendlyFigureId == message.Figure.Id)
            {
                _selectedFriendlyFigureId = null;
                _logger.Debug("[DamagePreview] Friendly selection cleared");
            }

            UpdateDamagePreview();
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

        private void OnFigureBoardRemoved(FigureBoardRemovedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureBoardRemoved: FigureId={message.FigureId}");
            
            // Если удалена фигура, на которой показан превью — очищаем
            if (_lastPreviewFigureId == message.FigureId)
            {
                _figurePresenter.SetDamagePreview(_lastPreviewFigureId.Value, null);
                _lastPreviewFigureId = null;
            }
            
            // Если удалена выбранная фигура — сбрасываем превью
            if (_selectedFriendlyFigureId == message.FigureId)
            {
                _selectedFriendlyFigureId = null;
                UpdateDamagePreview();
            }
        }

        private void OnAttackStarted(FigureAttackStartedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnAttackStarted: Attacker={message.AttackerId}, Target={message.TargetId}");
            
            // Скрываем превью с цели атаки
            _figurePresenter.SetDamagePreview(message.TargetId, null);
            
            // Если превью был показан на этой фигуре — сбрасываем
            if (_lastPreviewFigureId == message.TargetId)
            {
                _lastPreviewFigureId = null;
            }
        }

        private void UpdateDamagePreview()
        {
            _logger.Debug($"[DamagePreview] UpdatePreview: hovered={_hoveredFigureId?.ToString() ?? "null"}, selected={_selectedFriendlyFigureId?.ToString() ?? "null"}, lastPreview={_lastPreviewFigureId?.ToString() ?? "null"}");

            // Очищаем предыдущий превью при смене цели
            if (_lastPreviewFigureId.HasValue && _lastPreviewFigureId != _hoveredFigureId)
            {
                _logger.Debug($"[DamagePreview] Clearing old preview for FigureId={_lastPreviewFigureId}");
                _figurePresenter.SetDamagePreview(_lastPreviewFigureId.Value, null);
                _lastPreviewFigureId = null;
            }

            // Если нет условий для показа превью — выходим
            if (!_hoveredFigureId.HasValue || !_selectedFriendlyFigureId.HasValue)
            {
                _logger.Debug("[DamagePreview] No hover or selection — preview cleared");
                return;
            }

            // Пытаемся получить валидный превью
            if (!TryGetValidPreview(out var damage))
            {
                _logger.Debug("[DamagePreview] No valid preview — cleared");
                _figurePresenter.SetDamagePreview(_hoveredFigureId.Value, null);
                _lastPreviewFigureId = null;
                return;
            }

            // Показываем превью и запоминаем ID
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

            float rawDamage = Mathf.Max(1f, attacker.Stats.Attack.Value - target.Stats.Defence.Value);
            
            DamageContext dmgCtx = new(
                attacker,
                target,
                rawDamage,
                isPreview: true);

            DamageResult result = _damagePipeline.Calculate(dmgCtx);

            _logger.Debug($"[DamagePreview] PIPELINE: Raw={rawDamage}, Final={result.Final}, Cancelled={result.Cancelled}");

            if (result.Cancelled)
            {
                return false;
            }

            damage = result.Final;
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

            _subscriptions?.Dispose();
            _logger.Debug("[DamagePreviewService] Disposed");
        }
    }
}
