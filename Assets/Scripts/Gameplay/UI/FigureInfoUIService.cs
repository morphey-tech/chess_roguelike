using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.UI;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Сервис управления окном информации о фигуре.
    /// Открывает окно при клике на фигуру (ЛКМ).
    /// </summary>
    public sealed class FigureInfoUIService : IStartable, IDisposable
    {
        private readonly ISubscriber<FigureHoverChangedMessage> _figureHoverPublisher;
        private readonly ISubscriber<CellClickedMessage> _cellClickedPublisher;
        private readonly RunHolder _runHolder;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<FigureInfoUIService> _logger;

        private int? _hoveredFigureId;
        private IDisposable? _hoverSubscription;
        private IDisposable? _clickSubscription;
        private FigureInfoWindow? _window;
        private FigureInfoConfigRepository? _figureInfoCache;
        private PassiveConfigRepository? _passiveCache;

        [Inject]
        private FigureInfoUIService(
            ISubscriber<FigureHoverChangedMessage> figureHoverPublisher,
            ISubscriber<CellClickedMessage> cellClickedPublisher,
            RunHolder runHolder,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _figureHoverPublisher = figureHoverPublisher;
            _cellClickedPublisher = cellClickedPublisher;
            _runHolder = runHolder;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<FigureInfoUIService>();
        }

        void IStartable.Start()
        {
            _hoverSubscription = _figureHoverPublisher.Subscribe(OnFigureHoverChanged);
            _clickSubscription = _cellClickedPublisher.Subscribe(OnCellClicked);
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync()
        {
            _figureInfoCache = await _configProvider.Get<FigureInfoConfigRepository>("figures_info_conf");
            _passiveCache = await _configProvider.Get<PassiveConfigRepository>("passives_conf");
            _window = await UIService.GetOrCreateAsync<FigureInfoWindow>();
        }

        private void OnFigureHoverChanged(FigureHoverChangedMessage message)
        {
            _hoveredFigureId = message.FigureId;
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (!_hoveredFigureId.HasValue)
                return;

            var run = _runHolder.Current;
            if (run?.CurrentStage?.Grid == null)
                return;

            var grid = run.CurrentStage.Grid;
            var figure = grid.GetFigureById(_hoveredFigureId.Value);
            if (figure == null)
                return;

            ShowFigureInfo(figure);
        }

        private void ShowFigureInfo(Figure figure)
        {
            if (_window == null)
            {
                _logger.Debug("FigureInfoWindow is not initialized yet, skipping");
                return;
            }

            if (_passiveCache == null)
            {
                _logger.Debug("Passive cache is not loaded yet, skipping");
                return;
            }

            try
            {
                // Загружаем информацию о фигуре
                FigureInfoConfig? infoConfig = _figureInfoCache?.Get(figure.TypeId);

                // Загружаем конфиги пассивок
                var passiveConfigs = new List<PassiveConfig>();
                foreach (var passive in figure.BasePassives)
                {
                    var passiveConfig = _passiveCache.Get(passive.Id);
                    if (passiveConfig != null)
                    {
                        passiveConfigs.Add(passiveConfig);
                    }
                }

                var model = new FigureInfoWindow.FigureInfoModel
                {
                    Figure = figure,
                    InfoConfig = infoConfig,
                    PassiveConfigs = passiveConfigs
                };

                _window.Show(model);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to show figure info window: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _hoverSubscription?.Dispose();
            _clickSubscription?.Dispose();
        }
    }
}
