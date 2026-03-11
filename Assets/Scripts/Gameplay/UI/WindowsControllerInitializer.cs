#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Инициализирует WindowsController при старте приложения.
    /// Загружает префаб из Addressables и создаёт контроллер.
    /// </summary>
    public sealed class WindowsControllerInitializer
    {
        private readonly IUIAssetService _uiAssetService;
        private readonly ILogService _logService;
        private readonly ILogger<WindowsControllerInitializer> _logger;

        private WindowsController? _controller;
        private bool _initialized;

        [Inject]
        private WindowsControllerInitializer(
            IUIAssetService uiAssetService,
            ILogService logService)
        {
            _uiAssetService = uiAssetService;
            _logService = logService;
            _logger = logService.CreateLogger<WindowsControllerInitializer>();
        }

        public WindowsController Controller
        {
            get
            {
                if (!_initialized || _controller == null)
                {
                    throw new InvalidOperationException(
                        "WindowsController not initialized yet. Call InitializeAsync() first.");
                }
                return _controller;
            }
        }

        public async UniTask InitializeAsync()
        {
            if (_initialized)
            {
                _logger.Trace("[WindowsControllerInitializer] Already initialized");
                return;
            }

            _logger.Debug("[WindowsControllerInitializer] InitializeAsync started");

            try
            {
                _logger.Debug("[WindowsControllerInitializer] Loading WindowsController from Addressables...");

                // Загружаем префаб через AssetService
                GameObject? prefab = await _uiAssetService.LoadPrefabAsync("UI/Windows");
                
                // Инстанцируем и не выгружаем префаб (WindowsController живёт всегда)
                _controller = Object.Instantiate(prefab).GetComponent<WindowsController>();
                Object.DontDestroyOnLoad(_controller.gameObject);

                _logger.Debug("[WindowsControllerInitializer] WindowsController instantiated");

                await _controller.InitAsync(_uiAssetService, _logService);

                _initialized = true;
                _logger.Debug("[WindowsControllerInitializer] InitializeAsync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"[WindowsControllerInitializer] InitializeAsync failed: {ex.Message}", ex);
                throw;
            }
        }
    }
}
