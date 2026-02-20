using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack.Rules;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.Selection
{
    /// <summary>
    /// Shows damage preview on HP bar when hovering over enemy figure.
    /// Calculates damage based on current selected friendly figure's attack.
    /// </summary>
    public sealed class DamagePreviewService : IDisposable
    {
        private readonly IFigurePresenter _figurePresenter;
        private readonly RunHolder _runHolder;
        private readonly AttackRuleService _attackRuleService;
        private readonly IDisposable _subscriptions;
        private readonly ILogger<DamagePreviewService> _logger;

        private int? _hoveredFigureId;
        private int? _selectedFriendlyFigureId;
        private int? _lastPreviewFigureId; // ✅ Храним ID фигуры, которой показан превью
        private readonly CancellationTokenSource _disposeCts = new();

        [Inject]
        private DamagePreviewService(
            IFigurePresenter figurePresenter,
            RunHolder runHolder,
            AttackRuleService attackRuleService,
            ISubscriber<FigureSelectedMessage> selectedSubscriber,
            ISubscriber<FigureDeselectedMessage> deselectedSubscriber,
            ISubscriber<FigureHoverChangedMessage> hoverChangedSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            ILogService logService)
        {
            _figurePresenter = figurePresenter;
            _runHolder = runHolder;
            _attackRuleService = attackRuleService;
            _logger = logService.CreateLogger<DamagePreviewService>();

            _logger.Debug("[DamagePreviewService] Initialized");

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            selectedSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            deselectedSubscriber.Subscribe(OnFigureDeselected).AddTo(bag);
            hoverChangedSubscriber.Subscribe(OnHoverChanged).AddTo(bag);
            turnChangedSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            _logger.Debug($"[DamagePreview] OnFigureSelected: FigureId={message.Figure?.Id}, Team={message.Figure?.Team}");

            if (message.Figure != null)
            {
                _selectedFriendlyFigureId = message.Figure.Id;
                _logger.Debug($"[DamagePreview] Selected figure: {_selectedFriendlyFigureId}, Team={message.Figure.Team}");
            }
            else
            {
                _selectedFriendlyFigureId = null;
                _logger.Debug("[DamagePreview] Selection cleared (null figure)");
            }

            UpdateDamagePreview();
        }

        private void OnFigureDeselected(FigureDeselectedMessage message)
        {
            // ✅ FIX: Безопасная проверка на null
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

        private void UpdateDamagePreview()
        {
            _logger.Debug($"[DamagePreview] UpdatePreview: hovered={_hoveredFigureId?.ToString() ?? "null"}, selected={_selectedFriendlyFigureId?.ToString() ?? "null"}, lastPreview={_lastPreviewFigureId?.ToString() ?? "null"}");

            // ✅ FIX: Очищаем ПРЕДЫДУЩИЙ превью при смене цели
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

            var grid = _runHolder.Current?.CurrentStage?.Grid;
            if (grid == null)
            {
                _logger.Warning("[DamagePreview] Grid is null!");
                return;
            }

            var attacker = grid.GetAllFigures().FirstOrDefault(f => f.Id == _selectedFriendlyFigureId.Value);
            var target = grid.GetAllFigures().FirstOrDefault(f => f.Id == _hoveredFigureId.Value);

            if (attacker == null)
            {
                _logger.Warning($"[DamagePreview] Attacker not found: Id={_selectedFriendlyFigureId}");
                return;
            }

            if (target == null)
            {
                _logger.Warning($"[DamagePreview] Target not found: Id={_hoveredFigureId}");
                return;
            }

            // Don't show preview when hovering over own figure or same figure
            if (target.Team == attacker.Team)
            {
                _logger.Debug($"[DamagePreview] Target is friendly ({target.Team}) - clearing preview");
                _figurePresenter.SetDamagePreview(_hoveredFigureId.Value, null);
                _lastPreviewFigureId = null;
                return;
            }

            _logger.Debug($"[DamagePreview] Attacker={attacker.Id}(Id:{attacker.Id}), Target={target.Id}(Id:{target.Id})");

            var attackerCell = grid.FindFigure(attacker);
            var targetCell = grid.FindFigure(target);

            if (attackerCell == null || targetCell == null)
            {
                _logger.Warning("[DamagePreview] AttackerCell or TargetCell is null!");
                return;
            }

            var attackContext = new AttackRuleContext(
                attacker,
                target,
                attackerCell.Position,
                targetCell.Position,
                grid);

            if (!_attackRuleService.CanAttack(attackContext))
            {
                _logger.Debug($"[DamagePreview] Attack not valid — clearing preview for FigureId={_hoveredFigureId}");
                _figurePresenter.SetDamagePreview(_hoveredFigureId.Value, null);
                return;
            }

            // === РАСЧЕТ УРОНА ===
            float attack = attacker.Stats.Attack.Value;
            float defence = target.Stats.Defence.Value;
            float damage = Mathf.Max(1f, attack - defence);

            _logger.Debug($"[DamagePreview] CALC: ATK={attack}, DEF={defence}, DMG={damage}, HP={target.Stats.CurrentHp}");

            // ✅ Показываем превью (передаем ВЕЛИЧИНУ УРОНА) и запоминаем ID
            _figurePresenter.SetDamagePreview(_hoveredFigureId.Value, damage);
            _lastPreviewFigureId = _hoveredFigureId;

            _logger.Debug($"[DamagePreview] Preview SET for FigureId={_hoveredFigureId}: DMG={damage}");
        }

        public void Dispose()
        {
            _logger.Debug("[DamagePreviewService] Disposing...");

            // Очищаем все активные превью при уничтожении сервиса
            if (_lastPreviewFigureId.HasValue)
            {
                _figurePresenter.SetDamagePreview(_lastPreviewFigureId.Value, null);
                _lastPreviewFigureId = null;
            }

            _disposeCts.Cancel();
            _disposeCts.Dispose();
            _subscriptions?.Dispose();

            _logger.Debug("[DamagePreviewService] Disposed");
        }
    }
}