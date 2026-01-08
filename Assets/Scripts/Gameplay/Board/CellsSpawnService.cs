using System;
using Cysharp.Threading.Tasks;
using Project.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Logging;
using Project.Gameplay.Configs;
using Object = UnityEngine.Object;

namespace Project.Gameplay.Gameplay.Board
{
    public class CellsSpawnService
    {
        private readonly IAssetService _assetService;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<CellsSpawnService> _logger;

        public CellsSpawnService(IAssetService assetService,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _logger = logService.CreateLogger<CellsSpawnService>();
        }

        public async UniTask SpawnAsync(int row, int col, char cellSymbol)
        {
            string alias = cellSymbol.ToString();
            CellConfigRepository? cellConfigRepository = await _configProvider.Get<CellConfigRepository>("cells_conf");
            CellConfig cellConfig = cellConfigRepository.Cells.Find(c => c.Alias == alias);

            if (cellConfig == null)
            {
                throw new Exception($"No config found for cell alias {alias}.");
            }

            Object? asset = await _assetService.LoadAssetAsync<Object>(cellConfig.AssetKey);

            if (asset == null)
            {
                throw new Exception($"Failed to load asset for {alias}.");
            }

            _logger.Debug($"Spawning asset: {asset} at position ({row}, {col})");
        }
    }
}