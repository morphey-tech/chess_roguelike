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
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Presentations;
using Project.Unity.Unity.Views.Animations.Board;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity implementation of IBoardView.
    /// Effects are delegated to IBoardCellView components on prefabs.
    /// </summary>
    
    public sealed class BoardPresenter : IBoardPresenter, IGameShutdownCleanup
    {
        private const float CELL_SIZE = 1f;
        
        private readonly EntityService _entityService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly IAssetService _assetService;
        private readonly BoardAnimationFactory _animationFactory;
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
            BoardAnimationFactory animationFactory,
            ILogService logService)
        {
            _entityService = entityService;
            _worldRoot = worldRoot;
            _configProvider = configProvider;
            _assetService = assetService;
            _animationFactory = animationFactory;
            _logger = logService.CreateLogger<BoardPresenter>();
        }

        async UniTask IBoardPresenter.CreateBoardAssetAsync(string? assetKey, string? appearStrategyId)
        {
            if (string.IsNullOrEmpty(assetKey))
            {
                _logger.Error("Asset key is empty");
                return;
            }
            
            try
            {
                Vector3 rootPosition = _worldRoot.BoardRoot.position;
                GameObject assetPrefab = await _assetService.LoadAsync<GameObject>(assetKey);
                GameObject instance = _assetService.Instantiate(assetPrefab, rootPosition,
                    Quaternion.identity,
                    _worldRoot.BoardRoot);

                if (string.IsNullOrEmpty(appearStrategyId))
                {
                    _logger.Warning("Board appear skipped: strategy id is null/empty");
                }
                else
                {
                    IBoardAnimationStrategy animation = _animationFactory.Get(appearStrategyId);
                    await animation.Play(BoardAnimationTarget.Single(instance.transform));
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to create board asset: {e.Message}");
            }
        }

        async UniTask IBoardPresenter.CreateCellsBatchAsync(IReadOnlyList<CellSpawnRequest> requests,
            string? appearStrategyId)
        {
            if (requests.Count == 0)
            {
                return;
            }

            _cells.Clear();
            _cellsList.Clear();
            _cellConfigCache ??= await _configProvider.Get<CellConfigRepository>("cells_conf");

            List<string> uniqueSkins = requests.Select(r => r.SkinId).Distinct().ToList();
            List<UniTask> preloadTasks = new(uniqueSkins.Count);
            foreach (string skinId in uniqueSkins)
            {
                if (_cellPrefabCache.ContainsKey(skinId))
                {
                    continue;
                }
                CellConfig? cellConfig = _cellConfigCache!.Get(skinId);
                if (cellConfig == null)
                {
                    _logger.Error($"No config found for cell skin '{skinId}'");
                    continue;
                }
                preloadTasks.Add(LoadAndCachePrefabAsync(skinId, cellConfig.AssetKey));
            }

            if (preloadTasks.Count > 0)
            {
                await UniTask.WhenAll(preloadTasks);
            }

            List<Transform> cellsTfm = new();
            foreach (CellSpawnRequest req in requests)
            {
                if (!_cellPrefabCache.TryGetValue(req.SkinId, out GameObject prefab))
                {
                    continue;
                }

                Vector3 worldPos = new(
                    req.Position.Column * CELL_SIZE,
                    0f,
                    req.Position.Row * CELL_SIZE);
                EntityLink? cell = await _entityService.SpawnViewFromPrefab(
                    req.Entity,
                    prefab,
                    worldPos,
                    Quaternion.identity,
                    _worldRoot.BoardRoot);

                if (cell != null)
                {
                    cellsTfm.Add(cell.transform);
                    _cells[req.Position] = cell;
                    _cellsList.Add(cell);
                }
            }
            _logger.Debug($"Batch created {_cellsList.Count} cells");

            if (string.IsNullOrEmpty(appearStrategyId))
            {
                _logger.Warning("Board appear skipped: strategy id is null/empty");
            }
            else
            {
                IBoardAnimationStrategy animation = _animationFactory.Get(appearStrategyId);
                _logger.Info($"Playing board appear: {animation.Id}");
                
                await animation.Play(BoardAnimationTarget.Group(cellsTfm));
                _logger.Info("Board appear completed");
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

        void IGameShutdownCleanup.Cleanup()
        {
            (this as IBoardPresenter).Clear();
        }

        private async UniTask LoadAndCachePrefabAsync(string skinId, AssetKey assetKey)
        {
            GameObject prefab = await _assetService.LoadAsync<GameObject>(assetKey);
            _cellPrefabCache[skinId] = prefab;
        }
    }
}
