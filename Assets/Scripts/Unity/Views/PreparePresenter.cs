using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
    /// Handles all visual timing internally.
    /// </summary>
    public sealed class PreparePresenter : IPreparePresenter
    {
        private readonly IAssetService _assetService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<PreparePresenter> _logger;

        private readonly Dictionary<string, GameObject> _figures = new();
        private readonly List<GameObject> _slots = new();
        private readonly Dictionary<int, Vector3> _slotPositions = new();
        
        private CellConfigRepository _cellConfigCache;

        private const float CellSize = 1f;
        private const float SlotOffsetZ = -2f;
        private const int MaxSlots = 8;
        private const string FigureControllerAssetKey = "figure_controller";
        
        // Animation settings
        private const float SpawnDuration = 0.3f;
        private const Ease SpawnEase = Ease.OutBack;
        private const int SlotSpawnDelayMs = 50;
        private const int FigureSpawnDelayMs = 80;

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

        public async UniTask SpawnPrepareZoneAsync(IReadOnlyList<PrepareZoneFigureData> figures)
        {
            int totalCount = figures.Count;
            
            // First: spawn all slots with animation delay
            for (int i = 0; i < totalCount; i++)
            {
                Vector3 slotPos = CalculateSlotPosition(i, totalCount);
                _slotPositions[i] = slotPos;
                
                GameObject slot = await SpawnSlotAsync(slotPos);
                if (slot != null)
                {
                    _slots.Add(slot);
                }
                
                if (i < totalCount - 1)
                    await UniTask.Delay(SlotSpawnDelayMs);
            }
            
            _logger.Debug($"Spawned {totalCount} slots");
            
            // Second: spawn figures on slots with animation delay
            for (int i = 0; i < totalCount; i++)
            {
                PrepareZoneFigureData figData = figures[i];
                
                if (!_slotPositions.TryGetValue(i, out Vector3 slotPos))
                    continue;

                GameObject figure = await SpawnFigureAsync(figData.FigureTypeId, slotPos);
                if (figure != null)
                {
                    _figures[figData.FigureId] = figure;
                    HandFigureMarker marker = figure.GetComponent<HandFigureMarker>();
                    if (marker == null)
                    {
                        marker = figure.AddComponent<HandFigureMarker>();
                    }
                    marker.Initialize(figData.FigureId);
                }
                
                if (i < totalCount - 1)
                    await UniTask.Delay(FigureSpawnDelayMs);
            }

            _logger.Info($"Spawned {totalCount} figures in prepare zone");
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
            _slotPositions.Clear();

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

            GameObject slot = await _assetService.InstantiateAsync(
                cellConfig.AssetKey,
                position,
                Quaternion.identity,
                _worldRoot.PrepareRoot);

            if (slot != null)
            {
                // Spawn hidden, then animate
                slot.transform.localScale = Vector3.zero;
                slot.transform
                    .DOScale(Vector3.one, SpawnDuration)
                    .SetEase(SpawnEase);
            }

            return slot;
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

            // Spawn controller prefab
            GameObject controller = await _assetService.InstantiateAsync(
                FigureControllerAssetKey,
                slotPosition,
                Quaternion.identity,
                _worldRoot.PrepareRoot);

            if (controller == null)
            {
                _logger.Error($"Failed to instantiate figure controller for {figureTypeId}");
                return null;
            }
            
            // IMMEDIATELY hide to prevent flicker
            controller.transform.localScale = Vector3.zero;

            // Spawn view as child of controller (still hidden)
            GameObject view = await _assetService.InstantiateAsync(
                figureConfig.AssetKey,
                Vector3.zero,
                Quaternion.identity,
                controller.transform);

            if (view != null)
            {
                view.transform.localPosition = Vector3.zero;
                view.transform.localRotation = Quaternion.identity;
            }
            else
            {
                _logger.Warning($"Failed to instantiate view '{figureConfig.AssetKey}' for {figureTypeId}");
            }
            
            // Play spawn animation
            controller.transform
                .DOScale(Vector3.one, SpawnDuration)
                .SetEase(SpawnEase);

            return controller;
        }
    }
}
