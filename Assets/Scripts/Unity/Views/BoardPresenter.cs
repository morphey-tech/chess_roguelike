using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Cells;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Appear;
using Project.Gameplay.Gameplay.Board.Appear.Strategies;
using Project.Gameplay.Gameplay.Configs;
using UnityEngine;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity implementation of IBoardView.
    /// Effects are delegated to IBoardCellView components on prefabs.
    /// </summary>
    public sealed class BoardPresenter : IBoardPresenter
    {
        private readonly IAssetService _assetService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly BoardAppearAnimationFactory _animationFactory;
        private readonly ILogger<BoardPresenter> _logger;

        private readonly Dictionary<GridPosition, GameObject> _cells = new();
        private readonly Dictionary<GridPosition, IBoardCellView> _cellVisuals = new();
        private readonly List<GameObject> _cellsList = new();
        private CellConfigRepository _cellConfigCache;

        private const float CellSize = 1f;

        public BoardPresenter(
            IAssetService assetService,
            IWorldRoot worldRoot,
            ConfigProvider configProvider,
            BoardAppearAnimationFactory animationFactory,
            ILogService logService)
        {
            _assetService = assetService;
            _worldRoot = worldRoot;
            _configProvider = configProvider;
            _animationFactory = animationFactory;
            _logger = logService.CreateLogger<BoardPresenter>();

            _logger.Info("UnityBoardView created");
        }

        public async void CreateCell(GridPosition pos, string skinId)
        {
            _cellConfigCache ??= await _configProvider.Get<CellConfigRepository>("cells_conf");

            CellConfig cellConfig = _cellConfigCache.Cells.Find(c => c.Alias == skinId);

            if (cellConfig == null)
            {
                _logger.Error($"No config found for cell skin '{skinId}'");
                return;
            }

            Vector3 worldPos = new(
                pos.Column * CellSize,
                0f,
                pos.Row * CellSize);

            GameObject cell = await _assetService.InstantiateAsync(
                cellConfig.AssetKey,
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

            // Cache visual component if exists
            IBoardCellView view = cell.GetComponent<IBoardCellView>();
            if (view != null)
            {
                _cellVisuals[pos] = view;
            }

            _logger.Debug($"Cell '{skinId}' created at ({pos.Row}, {pos.Column})");
        }

        public void DestroyCell(GridPosition pos)
        {
            if (_cells.TryGetValue(pos, out GameObject cell))
            {
                _cells.Remove(pos);
                _cellVisuals.Remove(pos);
                _cellsList.Remove(cell);
                Object.Destroy(cell);
                _logger.Debug($"Cell destroyed at ({pos.Row}, {pos.Column})");
            }
        }

        public void PlayAppear(GridPosition pos)
        {
            if (_cellVisuals.TryGetValue(pos, out IBoardCellView visual))
            {
                visual.PlayAppearAsync().Forget();
            }
        }

        public void PlayHit(GridPosition pos)
        {
            if (_cellVisuals.TryGetValue(pos, out IBoardCellView visual))
            {
                visual.PlayHitAsync().Forget();
            }
        }

        public void Highlight(GridPosition pos, bool enabled)
        {
            if (_cellVisuals.TryGetValue(pos, out IBoardCellView visual))
            {
                visual.SetHighlight(enabled);
            }
        }

        public async UniTask PlayBoardAppearAsync(string strategyId)
        {
            await UniTask.Yield(); // Wait for all cells to instantiate

            IBoardAppearAnimationStrategy strategy = _animationFactory.Get(strategyId);
            _logger.Info($"Playing board appear: {strategy.Id}");

            await strategy.Appear(_cellsList);

            _logger.Info("Board appear completed");
        }

        public void Clear()
        {
            foreach (GameObject cell in _cells.Values)
            {
                if (cell != null)
                    Object.Destroy(cell);
            }
            _cells.Clear();
            _cellVisuals.Clear();
            _cellsList.Clear();
            _cellConfigCache = null;

            _logger.Debug("Board cleared");
        }
    }
}
