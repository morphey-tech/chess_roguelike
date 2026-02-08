using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Unity.Unity.Prepare;
using Project.Unity.Unity.Views.Components;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Оркестратор prepare-зоны. Не грузит ассеты, не считает позиции, не знает про DOTween.
    /// Build (prefabs) → Layout (positions) → Factory (create) → Animation (play).
    /// </summary>
    public sealed class PreparePresenter : IPreparePresenter, IGameShutdownCleanup
    {
        private readonly IPrepareZoneAssetProvider _provider;
        private readonly PrepareLayoutService _layout;
        private readonly PrepareViewFactory _factory;
        private readonly IWorldRoot _worldRoot;
        private readonly ILogger<PreparePresenter> _logger;

        private readonly Dictionary<string, GameObject> _figures = new();
        private readonly List<GameObject> _slots = new();
        private readonly Dictionary<int, Vector3> _slotPositions = new();
        private readonly Dictionary<string, int> _figureSlots = new();
        private readonly Dictionary<string, string> _figureTypes = new();

        [Inject]
        public PreparePresenter(
            IPrepareZoneAssetProvider provider,
            PrepareLayoutService layout,
            PrepareViewFactory factory,
            IWorldRoot worldRoot,
            ILogService logService)
        {
            _provider = provider;
            _layout = layout;
            _factory = factory;
            _worldRoot = worldRoot;
            _logger = logService.CreateLogger<PreparePresenter>();
        }

        public async UniTask SpawnPrepareZoneAsync(IReadOnlyList<PrepareZoneFigureData> figures)
        {
            int count = figures.Count;
            if (count == 0)
            {
                return;
            }

            PrepareZonePrefabs prefabs = await _provider.GetPrefabsAsync(GetUniqueFigureTypeIds(figures));
            if (prefabs.CellPrefab == null || prefabs.ControllerPrefab == null)
            {
                _logger.Error("Prepare prefabs missing (cell or controller)");
                return;
            }

            Transform root = _worldRoot.PrepareRoot;
            IReadOnlyList<Vector3> positions = _layout.BuildLayout(count);

            for (int i = 0; i < count; i++)
            {
                _slotPositions[i] = positions[i];
                GameObject slot = _factory.CreateSlot(prefabs.CellPrefab, positions[i], root);
                if (slot != null)
                {
                    _slots.Add(slot);
                    await PrepareAnimationPlayer.PlaySpawnAsync(slot);
                }
            }

            for (int i = 0; i < count; i++)
            {
                var fig = figures[i];
                if (!_slotPositions.TryGetValue(i, out Vector3 pos)) continue;
                if (!prefabs.FigurePrefabsByTypeId.TryGetValue(fig.FigureTypeId, out GameObject viewPrefab)) continue;

                GameObject controller = _factory.CreateFigure(prefabs.ControllerPrefab, viewPrefab, pos, root);
                if (controller != null)
                {
                    _figures[fig.FigureId] = controller;
                    _figureSlots[fig.FigureId] = i;
                    _figureTypes[fig.FigureId] = fig.FigureTypeId;
                    HandFigureMarker? marker = controller.GetComponent<HandFigureMarker>() 
                                               ?? controller.AddComponent<HandFigureMarker>();
                    marker.Initialize(fig.FigureId);
                    await PrepareAnimationPlayer.PlaySpawnAsync(controller);
                }
            }

            _logger.Info($"Prepare zone spawned: {count} slots, {_figures.Count} figures");
        }

        public void RemoveFigure(string figureId)
        {
            if (!_figures.Remove(figureId, out GameObject figure))
            {
                return;
            }
            Object.Destroy(figure);
            _logger.Debug($"Removed figure {figureId}");
        }

        public async UniTask RestoreFigureAsync(string figureId)
        {
            if (_figures.ContainsKey(figureId))
                return;
            if (!_figureSlots.TryGetValue(figureId, out int slotIndex))
            {
                _logger.Warning($"Restore failed: no slot for {figureId}");
                return;
            }
            if (!_figureTypes.TryGetValue(figureId, out string figureTypeId))
            {
                _logger.Warning($"Restore failed: no type for {figureId}");
                return;
            }
            if (!_slotPositions.TryGetValue(slotIndex, out Vector3 pos))
            {
                _logger.Warning($"Restore failed: no slot position for {figureId}");
                return;
            }

            PrepareZonePrefabs prefabs = await _provider.GetPrefabsAsync(new List<string> { figureTypeId });
            if (prefabs.ControllerPrefab == null)
            {
                _logger.Warning($"Restore failed: controller prefab missing for {figureId}");
                return;
            }
            if (!prefabs.FigurePrefabsByTypeId.TryGetValue(figureTypeId, out GameObject viewPrefab))
            {
                _logger.Warning($"Restore failed: view prefab missing for {figureTypeId}");
                return;
            }

            Transform root = _worldRoot.PrepareRoot;
            GameObject controller = _factory.CreateFigure(prefabs.ControllerPrefab, viewPrefab, pos, root);
            if (controller == null)
            {
                _logger.Warning($"Restore failed: controller not created for {figureId}");
                return;
            }

            _figures[figureId] = controller;
            HandFigureMarker? marker = controller.GetComponent<HandFigureMarker>() 
                                       ?? controller.AddComponent<HandFigureMarker>();
            marker.Initialize(figureId);
            await PrepareAnimationPlayer.PlaySpawnAsync(controller);
        }

        public void SetSelected(string figureId, bool selected)
        {
            if (_figures.TryGetValue(figureId, out GameObject figure))
                figure.transform.localScale = selected ? Vector3.one * 1.2f : Vector3.one;
        }

        public void Clear()
        {
            foreach (GameObject? figure in _figures.Values)
                if (figure != null)
                {
                    Object.Destroy(figure);
                }
            _figures.Clear();
            foreach (GameObject? slot in _slots)
                if (slot != null)
                {
                    Object.Destroy(slot);
                }
            _slots.Clear();
            _slotPositions.Clear();
            _figureSlots.Clear();
            _figureTypes.Clear();
            _logger.Debug("Prepare zone cleared");
        }

        void IGameShutdownCleanup.Cleanup()
        {
            Clear();
        }

        private static List<string> GetUniqueFigureTypeIds(IReadOnlyList<PrepareZoneFigureData> figures)
        {
            var ids = new List<string>(figures.Count);
            var seen = new HashSet<string>();
            foreach (PrepareZoneFigureData f in figures)
            {
                if (seen.Add(f.FigureTypeId))
                    ids.Add(f.FigureTypeId);
            }
            return ids;
        }
    }
}
