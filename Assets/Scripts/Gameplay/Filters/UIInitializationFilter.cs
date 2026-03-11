using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Filters;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.Filters
{
    /// <summary>
    /// Фильтр для инициализации UI системы перед запуском игровых сервисов.
    /// Явно вызывает инициализацию и контролирует весь флоу.
    /// </summary>
    public class UIInitializationFilter : IApplicationFilter
    {
        private readonly ILogger<UIInitializationFilter> _logger;
        private readonly IUIService _uiService;

        [Inject]
        public UIInitializationFilter(
            ILogService logService,
            IUIService uiService)
        {
            _logger = logService.CreateLogger<UIInitializationFilter>();
            _uiService = uiService;
        }

        public async UniTask RunAsync()
        {
            _logger.Debug("[UIInitializationFilter] Started");

            try
            {
                _logger.Debug("[UIInitializationFilter] Calling IUIService.InitializeAsync()...");
                await _uiService.InitializeAsync();
                
                _logger.Debug("[UIInitializationFilter] Waiting for IUIService.Initialized...");
                await _uiService.Initialized;
                
                _logger.Debug("[UIInitializationFilter] UI initialization completed successfully");
                _logger.Debug("[UIInitializationFilter] Completed");
            }
            catch (Exception ex)
            {
                _logger.Error($"[UIInitializationFilter] Failed: {ex.Message}", ex);
                throw;
            }
        }
    }
}
