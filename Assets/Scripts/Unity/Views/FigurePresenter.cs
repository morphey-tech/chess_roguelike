using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using UnityEngine;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Unity implementation of IFigurePresenter.
    /// Effects are delegated to IFigureView components on prefabs.
    /// </summary>
    public sealed class FigurePresenter : IFigurePresenter
    {
        private readonly IAssetService _assetService;
        private readonly IWorldRoot _worldRoot;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<FigurePresenter> _logger;

        private readonly Dictionary<int, GameObject> _figures = new();
        private readonly Dictionary<int, IFigureView> _figureViews = new();
        private readonly Dictionary<int, GridPosition> _positions = new();
        private FigureConfigRepository? _figureConfigCache;

        public FigurePresenter(
            IAssetService assetService,
            IWorldRoot worldRoot,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _assetService = assetService;
            _worldRoot = worldRoot;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<FigurePresenter>();

            _logger.Info("FigurePresenter created");
        }

        public async UniTask CreateFigure(int figureId, string typeId, GridPosition pos, Team team)
        {
            _figureConfigCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");

            FigureConfig config = System.Array.Find(_figureConfigCache.Figures, f => f.Id == typeId);

            if (config == null)
            {
                _logger.Error($"No config found for figure type '{typeId}'");
                return;
            }

            Vector3 worldPos = GetCellTopPosition(pos);

            // Rotation based on team
            Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);

            GameObject figureGO = await _assetService.InstantiateAsync(
                config.AssetKey,
                worldPos,
                rotation,
                _worldRoot.FigureRoot);

            if (figureGO == null)
            {
                _logger.Error($"Failed to instantiate figure {figureId}");
                return;
            }

            _figures[figureId] = figureGO;
            _positions[figureId] = pos;

            // Cache visual component if exists
            IFigureView view = figureGO.GetComponent<IFigureView>();
            if (view != null)
            {
                _figureViews[figureId] = view;
                view.PlaySpawnAsync().Forget();
            }

            _logger.Info($"Figure {figureId} ({typeId}) created at ({pos.Row}, {pos.Column}), team: {team}");
        }

        public void MoveFigure(int figureId, GridPosition to)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
            {
                _logger.Warning($"No figure {figureId} to move");
                return;
            }

            Vector3 newWorldPos = GetCellTopPosition(to);

            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                view.PlayMoveAsync(newWorldPos).Forget();
            }
            else
            {
                figureGO.transform.position = newWorldPos;
            }

            _positions[figureId] = to;

            _logger.Info($"Figure {figureId} moved to ({to.Row}, {to.Column})");
        }

        public void RemoveFigure(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                PlayDeathAndDestroy(figureId, figureGO, view).Forget();
            }
            else
            {
                CleanupFigure(figureId, figureGO);
            }
        }

        private async UniTaskVoid PlayDeathAndDestroy(int figureId, GameObject figureGO, IFigureView view)
        {
            await view.PlayDeathAsync();
            CleanupFigure(figureId, figureGO);
        }

        private void CleanupFigure(int figureId, GameObject figureGO)
        {
            _figures.Remove(figureId);
            _figureViews.Remove(figureId);
            _positions.Remove(figureId);
            Object.Destroy(figureGO);
            _logger.Debug($"Figure {figureId} removed");
        }

        public void PlayAttack(int figureId, GridPosition target)
        {
            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                Vector3 targetPos = GetCellTopPosition(target);
                view.PlayAttackAsync(targetPos).Forget();
            }
        }

        public void Clear()
        {
            foreach (GameObject figure in _figures.Values)
            {
                if (figure != null)
                    Object.Destroy(figure);
            }
            _figures.Clear();
            _figureViews.Clear();
            _positions.Clear();
            _figureConfigCache = null;

            _logger.Debug("Figures cleared");
        }

        private Vector3 GetCellTopPosition(GridPosition gridPos)
        {
            const float surfaceY = 0f;

            Vector3 rayOrigin = new(gridPos.Column, PhysicsSettings.CellRaycastHeight, gridPos.Row);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                PhysicsSettings.CellRaycastHeight * 2f, PhysicsSettings.CellLayerMask))
            {
                return hit.point;
            }

            return new Vector3(gridPos.Column, surfaceY, gridPos.Row);
        }
    }
}
