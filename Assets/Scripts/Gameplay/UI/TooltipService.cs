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
    public sealed class TooltipService : ITooltipService, IStartable, IDisposable
    {
        private readonly ISubscriber<TooltipShowRequestMessage> _showRequestPublisher;
        private readonly ISubscriber<TooltipHideRequestMessage> _hideRequestPublisher;
        private readonly ILogger<TooltipService> _logger;

        private TooltipWindow? _tooltipWindow;
        private IDisposable? _showSubscription;
        private IDisposable? _hideSubscription;
        
        // Задержка перед показом (чтобы не мелькало)
        private const float ShowDelay = 0.15f;
        private CancellationTokenSource? _showDelayCts;
        private bool _isInitializing;

        [Inject]
        private TooltipService(
            ISubscriber<TooltipShowRequestMessage> showRequestPublisher,
            ISubscriber<TooltipHideRequestMessage> hideRequestPublisher,
            ILogService logService)
        {
            _showRequestPublisher = showRequestPublisher;
            _hideRequestPublisher = hideRequestPublisher;
            _logger = logService.CreateLogger<TooltipService>();
        }

        void IStartable.Start()
        {
            _showSubscription = _showRequestPublisher.Subscribe(OnTooltipShowRequested);
            _hideSubscription = _hideRequestPublisher.Subscribe(OnTooltipHideRequested);
            
            // Не инициализируем окно сразу - сделаем это при первом запросе
            _isInitializing = false;
        }

        private async UniTask EnsureInitializedAsync()
        {
            if (_isInitializing || _tooltipWindow != null)
                return;

            _isInitializing = true;

            try
            {
                await UIService.Initialized;
                _tooltipWindow = await UIService.GetOrCreateAsync<TooltipWindow>();
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

        private void OnTooltipShowRequested(TooltipShowRequestMessage message)
        {
            ShowTooltipAsync(message.Content, message.Position).Forget();
        }

        private void OnTooltipHideRequested(TooltipHideRequestMessage message)
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

        public UniTaskVoid ShowTooltipAsync(string content, System.Numerics.Vector2 position)
        {
            throw new NotImplementedException();
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
            _showSubscription?.Dispose();
            _hideSubscription?.Dispose();
            _showDelayCts?.Cancel();
            _showDelayCts?.Dispose();
        }
    }
}
