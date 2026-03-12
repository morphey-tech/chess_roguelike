using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Сервис управления tooltip через MessagePipe.
    /// </summary>
    public sealed class TooltipService : ITooltipService, IInitializable, IDisposable
    {
        // Задержка перед показом (чтобы не мелькало)
        private const float ShowDelay = 0.15f;
        
        private readonly ISubscriber<string, TooltipMessage> _tooltipSubscriber;
        private readonly IUIService _uiService;
        private readonly ILogger<TooltipService> _logger;

        private TooltipWindow? _tooltipWindow;
        private IDisposable? _disposable;

        private CancellationTokenSource? _showDelayCts;
        private bool _isInitializing;

        [Inject]
        private TooltipService(
            ISubscriber<string, TooltipMessage> tooltipSubscriber,
            IUIService uiService,
            ILogService logService)
        {
            _tooltipSubscriber = tooltipSubscriber;
            _uiService = uiService;
            _logger = logService.CreateLogger<TooltipService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _tooltipSubscriber.Subscribe(TooltipMessage.SHOW, OnTooltipShow).AddTo(bag);
            _tooltipSubscriber.Subscribe(TooltipMessage.HIDE, OnTooltipHide ).AddTo(bag);
            _disposable = bag.Build();
            _isInitializing = false;
        }
        
        private async UniTask EnsureInitializedAsync()
        {
            if (_isInitializing || _tooltipWindow != null)
            {
                return;
            }

            _isInitializing = true;

            try
            {
                await _uiService.Initialized;
                _tooltipWindow = await _uiService.GetOrCreateAsync<TooltipWindow>();
                _tooltipWindow.Hide();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize tooltip: {ex.Message}");
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void OnTooltipShow(TooltipMessage message)
        {
            ShowTooltipAsync(message.Content, message.Position).Forget();
        }

        private void OnTooltipHide(TooltipMessage message)
        {
            HideTooltip();
        }

        public async UniTaskVoid ShowTooltipAsync(string content, Vector2 position)
        {
            // Ленивая инициализация
            await EnsureInitializedAsync();

            if (_tooltipWindow == null)
            {
                _logger.Debug("TooltipWindow is not initialized yet");
                return;
            }

            // Отменяем предыдущую задержку
            _showDelayCts?.Cancel();
            _showDelayCts = new CancellationTokenSource();

            var cts = _showDelayCts;

            try
            {
                // Показываем с задержкой
                await UniTask.Delay(TimeSpan.FromSeconds(ShowDelay), ignoreTimeScale: false, cancellationToken: cts.Token);

                if (cts.Token.IsCancellationRequested)
                    return;

                if (_tooltipWindow != null && !_tooltipWindow.IsVisible())
                {
                    _tooltipWindow.Show(content, position);
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное поведение при отмене
            }
        }

        public void HideTooltip()
        {
            // Отменяем показ если был в задержке
            _showDelayCts?.Cancel();
            
            if (_tooltipWindow != null && _tooltipWindow.IsVisible())
            {
                _tooltipWindow.Hide();
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _showDelayCts?.Cancel();
            _showDelayCts?.Dispose();
        }

    }
}
