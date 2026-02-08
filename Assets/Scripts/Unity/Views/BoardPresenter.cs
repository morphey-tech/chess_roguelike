using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.World;
using Project.Gameplay;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Appear;
using Project.Gameplay.Gameplay.Board.Appear.Strategies;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Presentations;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity implementation of IBoardView.
    /// Effects are delegated to IBoardCellView components on prefabs.
    /// </summary>
    
    public sealed class BoardPresenter : IBoardPresenter
    {
        private const float CELL_SIZE = 1f;
        
        private readonly EntityService _entityService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly IAssetService _assetService;
        private readonly BoardAppearAnimationFactory _animationFactory;
        private readonly ILogger<BoardPresenter> _logger;

        private readonly Dictionary<GridPosition, EntityLink> _cells = new();
        private readonly List<EntityLink> _cellsList = new();
        private CellConfigRepository? _cellConfigCache;
        private readonly Dictionary<string, GameObject> _cellPrefabCache = new();
        
        [Inject]
        private BoardPresenter(
            EntityService entityService,
            IWorldRoot worldRoot,
            ConfigProvider configProvider,
            IAssetService assetService,
            BoardAppearAnimationFactory animationFactory,
            ILogService logService)
        {
            _entityService = entityService;
            _worldRoot = worldRoot;
            _configProvider = configProvider;
            _assetService = assetService;
            _animationFactory = animationFactory;
            _logger = logService.CreateLogger<BoardPresenter>();
        }

        async UniTask IBoardPresenter.CreateCell(Entity entity, GridPosition pos, string skinId)
        {
            try
            {
                _cellConfigCache ??= await _configProvider.Get<CellConfigRepository>("cells_conf");

                if (!_cellPrefabCache.TryGetValue(skinId, out GameObject prefab))
                {
                    CellConfig cellConfig = _cellConfigCache.Get(skinId);
                    if (cellConfig == null)
                    {
                        _logger.Error($"No config found for cell skin '{skinId}'");
                        return;
                    }
                    prefab = await _assetService.LoadAssetAsync<GameObject>(cellConfig.AssetKey);
                    _cellPrefabCache[skinId] = prefab;
                }

                Vector3 worldPos = new(
                    pos.Column * CELL_SIZE,
                    0f,
                    pos.Row * CELL_SIZE);

                EntityLink? cell = _entityService.SpawnViewFromPrefab(
                    entity,
                    prefab,
                    worldPos,
                    Quaternion.identity,
                    _worldRoot.BoardRoot);

                if (cell == null)
                {
                    _logger.Error($"Failed to instantiate cell for skin '{skinId}'");
                    return;
                }

                _cells[pos] = cell;
                _cellsList.Add(cell);
                _logger.Debug($"Cell '{skinId}' created at ({pos.Row}, {pos.Column})");
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
            }
        }

        async UniTask IBoardPresenter.CreateCellsBatchAsync(IReadOnlyList<CellSpawnRequest> requests, string? appearStrategyId)
        {
            if (requests.Count == 0)
            {
                return;
            }

            _cells.Clear();
            _cellsList.Clear();
            _cellConfigCache ??= await _configProvider.Get<CellConfigRepository>("cells_conf");

            List<string> uniqueSkins = requests.Select(r => r.SkinId).Distinct().ToList();
            var preloadTasks = new List<UniTask>(uniqueSkins.Count);
            foreach (string skinId in uniqueSkins)
            {
                if (_cellPrefabCache.ContainsKey(skinId))
                    continue;
                CellConfig? cellConfig = _cellConfigCache!.Get(skinId);
                if (cellConfig == null)
                {
                    _logger.Error($"No config found for cell skin '{skinId}'");
                    continue;
                }
                preloadTasks.Add(LoadAndCachePrefabAsync(skinId, cellConfig.AssetKey));
            }
            if (preloadTasks.Count > 0)
                await UniTask.WhenAll(preloadTasks);

            foreach (CellSpawnRequest req in requests)
            {
                if (!_cellPrefabCache.TryGetValue(req.SkinId, out GameObject prefab))
                    continue;
                Vector3 worldPos = new(
                    req.Position.Column * CELL_SIZE,
                    0f,
                    req.Position.Row * CELL_SIZE);
                EntityLink? cell = _entityService.SpawnViewFromPrefab(
                    req.Entity,
                    prefab,
                    worldPos,
                    Quaternion.identity,
                    _worldRoot.BoardRoot);
                if (cell != null)
                {
                    _cells[req.Position] = cell;
                    _cellsList.Add(cell);
                }
            }

            _logger.Debug($"Batch created {_cellsList.Count} cells");

            if (!string.IsNullOrEmpty(appearStrategyId))
            {
                IBoardAppearAnimationStrategy strategy = _animationFactory.Get(appearStrategyId);
                _logger.Info($"Playing board appear: {strategy.Id}");
                await strategy.Appear(_cellsList);
                _logger.Info("Board appear completed");
            }
            else
            {
                _logger.Warning("Board appear skipped: strategy id is null/empty");
            }
        }

        UniTask IBoardPresenter.DestroyCell(GridPosition pos)
        {
            if (_cells.Remove(pos, out EntityLink cell))
            {
                _cellsList.Remove(cell);
                _entityService.Destroy(cell.EntityId);
                _logger.Debug($"Cell destroyed at ({pos.Row}, {pos.Column})");
            }
            return UniTask.CompletedTask;
        }

        async UniTask IBoardPresenter.PlayBoardAppearAsync(string strategyId)
        {
            await UniTask.Yield();
            IBoardAppearAnimationStrategy strategy = _animationFactory.Get(strategyId);
            _logger.Info($"Playing board appear: {strategy.Id}");
            await strategy.Appear(_cellsList);
            _logger.Info("Board appear completed");
        }

        void IBoardPresenter.Clear()
        {
            foreach (EntityLink cell in _cells.Values)
            {
                if (cell != null)
                {
                    _entityService.Destroy(cell.EntityId);
                }
            }
            _cells.Clear();
            _cellsList.Clear();
            _cellConfigCache = null;
            _cellPrefabCache.Clear();
        }

        private async UniTask LoadAndCachePrefabAsync(string skinId, AssetKey assetKey)
        {
            GameObject prefab = await _assetService.LoadAssetAsync<GameObject>(assetKey);
            _cellPrefabCache[skinId] = prefab;
        }
    }
}
