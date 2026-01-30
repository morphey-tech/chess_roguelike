using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Presentations;
using Project.Unity.Presentations;
using UnityEngine;

namespace Project.Unity.Unity.Views
{
    public sealed class FigurePresenter : IFigurePresenter
    {
        private readonly PresentationManager _presentationManager;
        private readonly IWorldRoot _worldRoot;
        private readonly ILogger<FigurePresenter> _logger;

        private readonly Dictionary<int, GameObject> _figures = new();
        private readonly Dictionary<int, IFigureView> _figureViews = new();
        private readonly Dictionary<int, GridPosition> _positions = new();

        public FigurePresenter(
            PresentationManager presentationManager,
            IWorldRoot worldRoot,
            ILogService logService)
        {
            _presentationManager = presentationManager;
            _worldRoot = worldRoot;
            _logger = logService.CreateLogger<FigurePresenter>();
            _logger.Info("FigurePresenter created");
        }

        private const string FigureControllerAssetKey = "figure_controller";

        public async UniTask CreateFigure(Figure figure, string viewAssetKey, GridPosition pos, Team team)
        {
            Vector3 worldPos = GetCellTopPosition(pos);
            
            // Spawn controller prefab
            EntityLink controllerLink = await _presentationManager.SpawnView(
                figure.Id,
                FigureControllerAssetKey,
                worldPos,
                Quaternion.identity,
                _worldRoot.FigureRoot);

            if (controllerLink == null)
            {
                _logger.Error($"Failed to instantiate figure controller for {figure}");
                return;
            }

            GameObject controller = controllerLink.gameObject;
            _figures[figure.Id] = controller;
            _positions[figure.Id] = pos;

            // Spawn view as child of controller
            GameObject viewGO = await _presentationManager.InstantiateAsChild(viewAssetKey, controller.transform);
            if (viewGO == null)
            {
                _logger.Warning($"Failed to instantiate view '{viewAssetKey}' for {figure}");
            }

            var spawnPresenter = controllerLink.GetComponent<FigureSpawnPresenter>();
            if (spawnPresenter != null)
                spawnPresenter.PlaySpawnAsync().Forget();
                
            var view = controllerLink.GetComponent<IFigureView>();
            if (view != null)
                _figureViews[figure.Id] = view;

            _logger.Info($"Figure {figure} created at ({pos.Row}, {pos.Column}), team: {team}");
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
            
            _presentationManager.Destroy(figureId);
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

        public void PlayDamageEffect(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            PlaySimpleDamageEffect(figureGO).Forget();
        }

        public void PlayDeathEffect(int figureId)
        {
            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                view.PlayDeathAsync().Forget();
            }
        }

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private async UniTaskVoid PlaySimpleDamageEffect(GameObject figureGO)
        {
            if (figureGO == null) return;

            Renderer[] renderers = figureGO.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                _logger.Warning($"No renderers found on {figureGO.name}");
                return;
            }

            var originalMaterials = new List<(Renderer r, Material[] originals)>();
            Color flashColor = Color.red;
            
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                originalMaterials.Add((r, r.materials));
            }

            const int flashDelayMs = 80;
            const int flashCount = 2;

            for (int i = 0; i < flashCount && figureGO != null; i++)
            {
                foreach (var (r, _) in originalMaterials)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, flashColor);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
                if (figureGO == null) return;
                
                foreach (var (r, _) in originalMaterials)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, Color.white);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
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
