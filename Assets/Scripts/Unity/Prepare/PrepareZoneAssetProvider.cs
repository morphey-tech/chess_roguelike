using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Prepare
{
    public sealed class PrepareZoneAssetProvider : IPrepareZoneAssetProvider
    {
        private const string FigureControllerAssetKey = "figure_controller";

        private readonly IPrepareZonePrefabCache _cache;
        private readonly ConfigProvider _configProvider;
        private readonly IAssetService _assetService;
        private readonly ILogger<PrepareZoneAssetProvider> _logger;

        [Inject]
        public PrepareZoneAssetProvider(
            IPrepareZonePrefabCache cache,
            ConfigProvider configProvider,
            IAssetService assetService,
            ILogService logService)
        {
            _cache = cache;
            _configProvider = configProvider;
            _assetService = assetService;
            _logger = logService.CreateLogger<PrepareZoneAssetProvider>();
        }

        public async UniTask<PrepareZonePrefabs> GetPrefabsAsync(IReadOnlyList<string> figureTypeIds)
        {
            if (_cache.TryGet(figureTypeIds, out PrepareZonePrefabs prefabs))
            {
                _logger.Debug("Prepare prefabs: from cache (no await)");
                return prefabs;
            }

            _logger.Warning($"Prepare prefabs: cache miss, loading [{string.Join(", ", figureTypeIds)}] — будет задержка");
            var cellsRepo = await _configProvider.Get<CellConfigRepository>("cells_conf");
            var figuresRepo = await _configProvider.Get<FigureConfigRepository>("figures_conf");

            GameObject cellPrefab = null;
            if (cellsRepo.Cells.Count > 0)
                cellPrefab = await _assetService.LoadAssetAsync<GameObject>(cellsRepo.Cells[0].AssetKey);
            var controllerPrefab = await _assetService.LoadAssetAsync<GameObject>(FigureControllerAssetKey);

            var figurePrefabs = new Dictionary<string, GameObject>();
            foreach (string typeId in figureTypeIds.Distinct())
            {
                var cfg = figuresRepo.GetBy(typeId);
                if (cfg == null || string.IsNullOrEmpty(cfg.AssetKey)) continue;
                var prefab = await _assetService.LoadAssetAsync<GameObject>(cfg.AssetKey);
                if (prefab != null)
                    figurePrefabs[typeId] = prefab;
            }

            return new PrepareZonePrefabs(cellPrefab, controllerPrefab, figurePrefabs);
        }
    }
}
