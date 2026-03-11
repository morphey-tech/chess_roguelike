using Project.Core.Core.Combat;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board.Messages;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Save.Service;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Board.Capacity
{
    public sealed class BoardCapacityService
    {
        public int Used => _model.Used;
        public int Capacity => _model.Capacity;
        public int Free => _model.Free;
        
        private readonly BoardCapacityModel _model;
        private readonly ConfigProvider _configProvider;
        private readonly PlayerRunStateService _runStateService;
        private readonly IPublisher<BoardCapacityChangedMessage> _publisher;
        private readonly ILogger<BoardCapacityService> _logger;

        private FigureConfigRepository _figureConfigRepository = null!;
        private FigureDescriptionConfigRepository _figureDescriptionRepository = null!;

        [Inject]
        private BoardCapacityService(
            BoardCapacityModel model,
            ConfigProvider configProvider,
            PlayerRunStateService runStateService,
            IPublisher<BoardCapacityChangedMessage> publisher,
            ILogService logService)
        {
            _model = model;
            _configProvider = configProvider;
            _runStateService = runStateService;
            _publisher = publisher;
            _logger = logService.CreateLogger<BoardCapacityService>();
        }

        public async UniTask InitializeForBoardAsync(BoardConfig boardConfig)
        {
            await EnsureFigureConfigsAsync();

            int configuredMax = boardConfig.MaxCapacity > 0 ? boardConfig.MaxCapacity : boardConfig.BaseCapacity;
            int defaultCapacity = boardConfig.BaseCapacity;

            if (_runStateService is { HasRun: true, Current: not null })
            {
                int savedCapacity = _runStateService.Current.BoardCapacity;
                int savedUsed = _runStateService.Current.UsedCapacity;

                int capacity = savedCapacity > 0 ? savedCapacity : defaultCapacity;
                _model.Configure(capacity, configuredMax);
                _model.SetUsed(savedUsed);
            }
            else
            {
                _model.Configure(defaultCapacity, configuredMax);
                _model.Reset();
            }

            SyncToRunState();
            Publish();
        }

        public bool CanSpawn(FigureDescriptionConfig config)
        {
            return _model.CanReserve(config.Load);
        }

        public bool CanSpawnByTypeAsync(string figureTypeId)
        {
            int load = GetLoadByTypeId(figureTypeId);
            return _model.CanReserve(load);
        }

        public bool TryReserve(FigureDescriptionConfig config)
        {
            bool ok = _model.TryReserve(config.Load);
            if (ok)
            {
                SyncToRunState();
                Publish();
            }

            return ok;
        }

        public bool TryReserveByTypeAsync(string figureTypeId)
        {
            int load = GetLoadByTypeId(figureTypeId);
            bool ok = _model.TryReserve(load);
            if (ok)
            {
                SyncToRunState();
                Publish();
            }

            return ok;
        }

        public void ReleaseByType(string figureTypeId)
        {
            int load = GetLoadByTypeId(figureTypeId);
            _model.Release(load);
            SyncToRunState();
            Publish();
        }

        public void Reset()
        {
            _model.Reset();
            SyncToRunState();
            Publish();
        }

        public void RecalculateFromBoard(IEnumerable<Figure> figures)
        {
            int sum = 0;
            foreach (Figure figure in figures)
            {
                if (figure.Team != Team.Player)
                    continue;

                sum += GetLoadByTypeId(figure.TypeId);
            }

            _model.SetUsed(sum);
            SyncToRunState();
            Publish();
        }

        private async UniTask EnsureFigureConfigsAsync()
        {
            _figureConfigRepository = await _configProvider.Get<FigureConfigRepository>("figures_conf");
            _figureDescriptionRepository = await _configProvider
                .Get<FigureDescriptionConfigRepository>("figure_descriptions_conf");
        }

        private int GetLoadByTypeId(string figureTypeId)
        {
            FigureConfig? figure = _figureConfigRepository.Get(figureTypeId);
            if (figure == null)
            {
                _logger.Warning($"Figure config '{figureTypeId}' not found; load fallback=0");
                return 0;
            }
            
            FigureDescriptionConfig? description = _figureDescriptionRepository.Get(figure.DescriptionId);
            if (description == null)
            {
                _logger.Warning($"Figure description config '{figure.DescriptionId}' not found; load fallback=0");
                return 0;
            }
            return description.Load < 0 ? 0 : description.Load;
        }

        private void SyncToRunState()
        {
            if (!_runStateService.HasRun || _runStateService.Current == null)
                return;

            _runStateService.Current.BoardCapacity = _model.Capacity;
            _runStateService.Current.UsedCapacity = _model.Used;
        }

        private void Publish()
        {
            _publisher.Publish(new BoardCapacityChangedMessage(_model.Used, _model.Capacity));
        }

    }
}

