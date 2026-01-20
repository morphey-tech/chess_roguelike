using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Logging;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Prepare;
using UnityEngine;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity implementation of IPreparePresenter.
    /// Spawns slots and figures in PrepareRoot.
    /// </summary>
    public sealed class PreparePresenter : IPreparePresenter
    {
        private readonly IAssetService _assetService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<PreparePresenter> _logger;

        private readonly Dictionary<string, GameObject> _figures = new();
        private readonly List<GameObject> _slots = new();
        
        private CellConfigRepository? _cellConfigCache;
        private int _totalFigures;

        private const float CellSize = 1f;
        private const float SlotOffsetZ = -2f;
        private const int MaxSlots = 8;

        //TODO: имеются тут дебаговые вещицы ... пока так
        public PreparePresenter(
            IAssetService assetService,
            IWorldRoot worldRoot,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _assetService = assetService;
            _worldRoot = worldRoot;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<PreparePresenter>();
        }

        public async UniTask CreateSlotWithFigureAsync(int index, int totalCount, string figureId, string figureTypeId)
        {
            Vector3 slotPos = CalculateSlotPosition(index, totalCount);
            
            GameObject slot = await SpawnSlotAsync(slotPos);
            if (slot != null)
            {
                _slots.Add(slot);
            }

            GameObject figure = await SpawnFigureAsync(figureTypeId, slotPos);
            if (figure != null)
            {
                _figures[figureId] = figure;
                HandFigureMarker marker = figure.GetComponent<HandFigureMarker>();
                if (marker == null)
                {
                    marker = figure.AddComponent<HandFigureMarker>();
                }
                marker.Initialize(figureId);
            }

            _logger.Debug($"Created slot {index}/{totalCount} with figure {figureTypeId} (id={figureId})");
        }

        public void RemoveFigure(string figureId)
        {
            if (_figures.Remove(figureId, out GameObject figure))
            {
                Object.Destroy(figure);
                _logger.Debug($"Removed figure {figureId} from prepare zone");
            }
        }

        public void SetSelected(string figureId, bool selected)
        {
            if (_figures.TryGetValue(figureId, out GameObject figure))
            {
                figure.transform.localScale = selected ? Vector3.one * 1.2f : Vector3.one;
            }
        }

        public void Clear()
        {
            foreach (GameObject figure in _figures.Values)
            {
                if (figure != null) Object.Destroy(figure);
            }
            _figures.Clear();

            foreach (GameObject slot in _slots)
            {
                if (slot != null) Object.Destroy(slot);
            }
            _slots.Clear();

            _logger.Debug("Prepare zone cleared");
        }

        private Vector3 CalculateSlotPosition(int index, int totalCount)
        {
            float startOffset = (MaxSlots - totalCount) / 2f;
            float x = (startOffset + index) * CellSize;
            
            return new Vector3(x, 0f, SlotOffsetZ);
        }

        private async UniTask<GameObject> SpawnSlotAsync(Vector3 position)
        {
            _cellConfigCache ??= await _configProvider.Get<CellConfigRepository>("cells_conf");
            
            CellConfig cellConfig = _cellConfigCache.Cells.Count > 0 
                ? _cellConfigCache.Cells[0] 
                : null;

            if (cellConfig == null)
            {
                _logger.Warning("No cell config for slots");
                return null;
            }

            return await _assetService.InstantiateAsync(
                cellConfig.AssetKey,
                position,
                Quaternion.identity,
                _worldRoot.PrepareRoot);
        }

        private async UniTask<GameObject> SpawnFigureAsync(string figureTypeId, Vector3 slotPosition)
        {
            FigureConfigRepository figureRepo = 
                await _configProvider.Get<FigureConfigRepository>("figures_conf");
            
            FigureConfig figureConfig = figureRepo.GetBy(figureTypeId);
            if (figureConfig == null)
            {
                _logger.Error($"Figure config not found: {figureTypeId}");
                return null;
            }

            return await _assetService.InstantiateAsync(
                figureConfig.AssetKey,
                slotPosition,
                Quaternion.identity,
                _worldRoot.PrepareRoot);
        }
    }
}
