using System;
using Cysharp.Threading.Tasks;
using Project.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Logging;
using Project.Core.World;
using Project.Gameplay.Configs;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board
{
    public sealed class CellsSpawnService
    {
        private readonly IAssetService _assetService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<CellsSpawnService> _logger;

        private const float CellSize = 1f;

        public CellsSpawnService(
            IAssetService assetService,
            IWorldRoot worldRoot,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _worldRoot = worldRoot ?? throw new ArgumentNullException(nameof(worldRoot));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _logger = logService.CreateLogger<CellsSpawnService>();
        }

        public async UniTask<GameObject> SpawnAsync(int row, int col, char cellSymbol)
        {
            string alias = cellSymbol.ToString();
            CellConfigRepository cellConfigRepository =
                await _configProvider.Get<CellConfigRepository>("cells_conf");
            CellConfig cellConfig =
                cellConfigRepository.Cells.Find(c => c.Alias == alias);

            if (cellConfig == null)
            {
                throw new Exception($"No config found for cell alias {alias}.");
            }

            Vector3 position = new(
                col * CellSize,
                0f,
                row * CellSize);

            GameObject cell =
                await _assetService.InstantiateAsync(
                    cellConfig.AssetKey,
                    position,
                    Quaternion.identity,
                    _worldRoot.BoardRoot);

            if (cell == null)
            {
                throw new Exception($"Failed to instantiate cell for {alias}.");
            }

            _logger.Debug($"Spawned cell '{alias}' at ({row}, {col})");
            return cell;
        }
    }
}
