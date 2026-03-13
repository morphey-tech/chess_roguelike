using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Save.Models;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Единственное место хранения загруженных префабов prepare-зоны.
    /// Заполняется во время появления доски (PreloadAsync / WarmAsync), отдаёт по TryGet без await.
    /// Реализует IPrepareZoneAssetPreloader для фазы и IPrepareZonePrefabCache для провайдера.
    /// </summary>
    public sealed class PrepareZonePrefabCache : IPrepareZoneAssetPreloader, IPrepareZonePrefabCache
    {
        private const string FigureControllerAssetKey = "figure_controller";

        private readonly ConfigProvider _configProvider;
        private readonly IAssetService _assetService;
        private readonly ILogger<PrepareZonePrefabCache> _logger;

        private GameObject _cellPrefab;
        private GameObject _controllerPrefab;
        private Dictionary<string, GameObject> _figurePrefabs;

        [Inject]
        public PrepareZonePrefabCache(
            ConfigProvider configProvider,
            IAssetService assetService,
            ILogService logService)
        {
            _configProvider = configProvider;
            _assetService = assetService;
            _logger = logService.CreateLogger<PrepareZonePrefabCache>();
        }

        public async UniTask PreloadAsync(PlayerRunStateModel runState)
        {
            if (runState == null || runState.FiguresInHand.Count == 0)
                return;
            var typeIds = runState.FiguresInHand.Select(f => f.TypeId).Distinct().ToList();
            await WarmAsync(typeIds);
        }

        public async UniTask WarmAsync(IReadOnlyList<string> figureTypeIds)
        {
            Clear();
            if (figureTypeIds == null || figureTypeIds.Count == 0)
            {
                return;
            }

            var cellsRepo = await _configProvider.Get<CellConfigRepository>("cells_conf");
            var figuresRepo = await _configProvider.Get<FigureConfigRepository>("figures_conf");

            var loadCell = cellsRepo.Cells.Length > 0
                ? _assetService.LoadAsync<GameObject>(cellsRepo.Cells[0].AssetKey)
                : UniTask.FromResult<GameObject>(null);
            var loadController = _assetService.LoadAsync<GameObject>(FigureControllerAssetKey);

            var figureTasks = new List<UniTask<(string typeId, GameObject prefab)>>();
            foreach (string typeId in figureTypeIds.Distinct())
            {
                var cfg = figuresRepo.Get(typeId);
                if (cfg == null || string.IsNullOrEmpty(cfg.AssetKey)) continue;
                figureTasks.Add(LoadFigureAsync(cfg.AssetKey, typeId));
            }

            if (figureTasks.Count == 0)
            {
                var (cell, ctrl) = await (loadCell, loadController);
                _cellPrefab = cell;
                _controllerPrefab = ctrl;
                _figurePrefabs = new Dictionary<string, GameObject>();
            }
            else
            {
                var (cell, ctrl, figureResults) = await (loadCell, loadController, UniTask.WhenAll(figureTasks));
                _cellPrefab = cell;
                _controllerPrefab = ctrl;
                _figurePrefabs = new Dictionary<string, GameObject>();
                foreach (var (typeId, prefab) in figureResults)
                {
                    if (prefab != null)
                        _figurePrefabs[typeId] = prefab;
                }
            }

            _logger.Debug($"Cache warmed: cell={_cellPrefab != null}, controller={_controllerPrefab != null}, figures={_figurePrefabs?.Count ?? 0}");
        }

        private async UniTask<(string typeId, GameObject prefab)> LoadFigureAsync(string assetKey, string typeId)
        {
            var go = await _assetService.LoadAsync<GameObject>(assetKey);
            return (typeId, go);
        }

        public bool TryGet(IReadOnlyList<string> figureTypeIds, out PrepareZonePrefabs prefabs)
        {
            prefabs = null;
            if (_cellPrefab == null || _controllerPrefab == null || _figurePrefabs == null)
            {
                _logger.Warning("Cache miss: cache not warmed (cell/controller/figures null)");
                return false;
            }
            if (figureTypeIds == null || figureTypeIds.Count == 0)
                return false;
            foreach (string typeId in figureTypeIds)
            {
                if (!_figurePrefabs.TryGetValue(typeId, out _))
                {
                    _logger.Warning($"Cache miss: figure type '{typeId}' not in cache (cached: [{string.Join(", ", _figurePrefabs.Keys)}])");
                    return false;
                }
            }
            prefabs = new PrepareZonePrefabs(_cellPrefab, _controllerPrefab, _figurePrefabs);
            return true;
        }

        public void Clear()
        {
            _cellPrefab = null;
            _controllerPrefab = null;
            _figurePrefabs = null;
        }
    }
}
