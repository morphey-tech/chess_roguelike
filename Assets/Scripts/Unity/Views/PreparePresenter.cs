using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Prepare;
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
    public sealed class PreparePresenter : IPreparePresenter
    {
        private readonly IPrepareZoneAssetProvider _provider;
        private readonly PrepareLayoutService _layout;
        private readonly PrepareViewFactory _factory;
        private readonly PrepareAnimationPlayer _anim;
        private readonly IWorldRoot _worldRoot;
        private readonly ILogger<PreparePresenter> _logger;

        private readonly Dictionary<string, GameObject> _figures = new();
        private readonly List<GameObject> _slots = new();
        private readonly Dictionary<int, Vector3> _slotPositions = new();

        [Inject]
        public PreparePresenter(
            IPrepareZoneAssetProvider provider,
            PrepareLayoutService layout,
            PrepareViewFactory factory,
            PrepareAnimationPlayer anim,
            IWorldRoot worldRoot,
            ILogService logService)
        {
            _provider = provider;
            _layout = layout;
            _factory = factory;
            _anim = anim;
            _worldRoot = worldRoot;
            _logger = logService.CreateLogger<PreparePresenter>();
        }

        public async UniTask SpawnPrepareZoneAsync(IReadOnlyList<PrepareZoneFigureData> figures)
        {
            int count = figures.Count;
            if (count == 0) return;

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
                    await _anim.PlaySpawnAsync(slot);
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
                    var marker = controller.GetComponent<HandFigureMarker>() ?? controller.AddComponent<HandFigureMarker>();
                    marker.Initialize(fig.FigureId);
                    await _anim.PlaySpawnAsync(controller);
                }
            }

            _logger.Info($"Prepare zone spawned: {count} slots, {_figures.Count} figures");
        }

        public void RemoveFigure(string figureId)
        {
            if (_figures.Remove(figureId, out GameObject figure))
            {
                Object.Destroy(figure);
                _logger.Debug($"Removed figure {figureId}");
            }
        }

        public void SetSelected(string figureId, bool selected)
        {
            if (_figures.TryGetValue(figureId, out GameObject figure))
                figure.transform.localScale = selected ? Vector3.one * 1.2f : Vector3.one;
        }

        public void Clear()
        {
            foreach (var figure in _figures.Values)
                if (figure != null) Object.Destroy(figure);
            _figures.Clear();
            foreach (var slot in _slots)
                if (slot != null) Object.Destroy(slot);
            _slots.Clear();
            _slotPositions.Clear();
            _logger.Debug("Prepare zone cleared");
        }

        private static List<string> GetUniqueFigureTypeIds(IReadOnlyList<PrepareZoneFigureData> figures)
        {
            var ids = new List<string>(figures.Count);
            var seen = new HashSet<string>();
            foreach (var f in figures)
            {
                if (seen.Add(f.FigureTypeId))
                    ids.Add(f.FigureTypeId);
            }
            return ids;
        }
    }
}
