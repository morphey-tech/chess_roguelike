using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Core.Core.World;
using Project.Gameplay;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Presentations;
using Project.Unity.Presentations;
using UnityEngine;

namespace Project.Unity.Unity.Views
{
    public sealed class FigurePresenter : IFigurePresenter
    {
        private const string FigureControllerAssetKey = "figure_controller";
        private const int MoveAnimationDurationMs = 300;
        private const int AttackAnimationDurationMs = 200;
        private const int DamageFlashDurationMs = 160;
        private const int HealEffectDurationMs = 300;
        private const int DeathAnimationDurationMs = 500;
        private const int PushAnimationDurationMs = 200;
        
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

        public async UniTask CreateFigure(Figure figure, string viewAssetKey, GridPosition pos, Team team)
        {
            Vector3 worldPos = GetCellTopPosition(pos);
            
            // Spawn controller prefab
            EntityLink controllerLink = await _presentationManager.SpawnView(
                figure,
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
            
            // IMMEDIATELY hide to prevent flicker - before any other operations
            controller.transform.localScale = Vector3.zero;
            
            _figures[figure.Id] = controller;
            _positions[figure.Id] = pos;

            // Spawn view as child of controller (still hidden because parent scale is 0)
            GameObject viewGO = await _presentationManager.InstantiateAsChild(viewAssetKey, controller.transform);
            if (viewGO == null)
            {
                _logger.Warning($"Failed to instantiate view '{viewAssetKey}' for {figure}");
            }
            
            var selectPresenter = controllerLink.GetComponent<FigureSelectPresenter>();
            if (selectPresenter != null)
                selectPresenter.InitSelecting();
                
            var view = controllerLink.GetComponent<IFigureView>();
            if (view != null)
                _figureViews[figure.Id] = view;

            // NOW play spawn animation (scale from 0 to 1)
            var spawnPresenter = controllerLink.GetComponent<FigureSpawnPresenter>();
            if (spawnPresenter != null)
            {
                spawnPresenter.PlaySpawnAsync().Forget();
            }
            else
            {
                // No spawn presenter - just set scale to 1
                controller.transform.localScale = Vector3.one;
            }

            _logger.Info($"Figure {figure} ({viewAssetKey}) created at ({pos.Row}, {pos.Column}), team: {team}");
        }

        public async UniTask MoveFigureAsync(int figureId, GridPosition to)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
            {
                _logger.Warning($"No figure {figureId} to move");
                return;
            }

            Vector3 newWorldPos = GetCellTopPosition(to);

            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                await view.PlayMoveAsync(newWorldPos);
            }
            else
            {
                figureGO.transform.position = newWorldPos;
                await UniTask.Delay(MoveAnimationDurationMs);
            }

            _positions[figureId] = to;
            _logger.Info($"Figure {figureId} moved to ({to.Row}, {to.Column})");
        }

        public async UniTask RemoveFigureAsync(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                await view.PlayDeathAsync();
            }
            
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

        public async UniTask PlayAttackAsync(int figureId, GridPosition target)
        {
            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                Vector3 targetPos = GetCellTopPosition(target);
                await view.PlayAttackAsync(targetPos);
            }
            else
            {
                await UniTask.Delay(AttackAnimationDurationMs);
            }
        }

        public async UniTask PlayDamageEffectAsync(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            await PlaySimpleDamageEffect(figureGO);
        }

        public async UniTask PlayHealEffectAsync(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            await PlaySimpleHealEffect(figureGO);
        }

        public async UniTask PlayDeathEffectAsync(int figureId)
        {
            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                await view.PlayDeathAsync();
            }
            else
            {
                await UniTask.Delay(DeathAnimationDurationMs);
            }
        }

        public async UniTask PlayPushEffectAsync(int figureId, GridPosition from, GridPosition to)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            Vector3 newWorldPos = GetCellTopPosition(to);

            if (_figureViews.TryGetValue(figureId, out IFigureView view))
            {
                // Could have a special push animation, for now use move
                await view.PlayMoveAsync(newWorldPos);
            }
            else
            {
                figureGO.transform.position = newWorldPos;
                await UniTask.Delay(PushAnimationDurationMs);
            }

            _positions[figureId] = to;
            _logger.Info($"Figure {figureId} pushed from ({from.Row}, {from.Column}) to ({to.Row}, {to.Column})");
        }

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private async UniTask PlaySimpleDamageEffect(GameObject figureGO)
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

        private async UniTask PlaySimpleHealEffect(GameObject figureGO)
        {
            if (figureGO == null) return;

            Renderer[] renderers = figureGO.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Color healColor = Color.green;
            const int flashDelayMs = 100;
            const int flashCount = 2;

            for (int i = 0; i < flashCount && figureGO != null; i++)
            {
                foreach (Renderer r in renderers)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, healColor);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
                if (figureGO == null) return;
                
                foreach (Renderer r in renderers)
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
