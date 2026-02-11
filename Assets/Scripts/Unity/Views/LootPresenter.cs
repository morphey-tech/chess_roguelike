using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Core.Grid;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    /// <summary>
    /// Stub loot visual: spawns a cube above the cell, it falls and "evaporates", then applies economy.
    /// One visual per LootResult (one cube per drop event).
    /// </summary>
    public sealed class LootPresenter : ILootPresenter, IGameShutdownCleanup
    {
        private const float CellSize = 1f;
        private const float FallHeight = 1.4f;
        private const float LandHeight = 0.25f;
        private const float FallDuration = 0.35f;
        private const float EvaporateDuration = 0.25f;
        private const float DelayBetweenDrops = 0.06f;
        private const int MaxVisualCubes = 5;

        private readonly IWorldRoot _worldRoot;
        private readonly EconomyService _economy;

        [Inject]
        public LootPresenter(IWorldRoot worldRoot, EconomyService economy)
        {
            _worldRoot = worldRoot;
            _economy = economy;
        }

        public async UniTask PresentAsync(LootVisualContext context)
        {
            if (context?.Loot == null || context.Loot.IsEmpty)
                return;

            // One cube per drop event; if multiple resources/items we could spawn multiple cubes with delay
            int totalDrops = context.Loot.Resources.Count + context.Loot.Items.Count;
            if (totalDrops <= 0)
            {
                _economy.ApplyLootResult(context.Loot);
                return;
            }

            Vector3 baseWorldPos = GridToWorld(context.DropPosition);

            if (totalDrops == 1)
            {
                await PlayOneDropAsync(baseWorldPos, context.Loot);
                return;
            }

            // Burst: several cubes with small delay (cap to avoid clutter), then apply economy once at the end
            int visualCount = Mathf.Min(totalDrops, MaxVisualCubes);
            var tasks = new List<UniTask>();
            for (int i = 0; i < visualCount; i++)
            {
                float delay = i * DelayBetweenDrops;
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), 0f, UnityEngine.Random.Range(-0.15f, 0.15f));
                Vector3 pos = baseWorldPos + offset;
                tasks.Add(PlayOneDropDelayedAsync(pos, delay));
            }

            await UniTask.WhenAll(tasks);
            _economy.ApplyLootResult(context.Loot);
        }

        private async UniTask PlayOneDropAsync(Vector3 worldPos, LootResult loot)
        {
            await PlayCubeFallAndEvaporateAsync(worldPos);
            _economy.ApplyLootResult(loot);
        }

        private async UniTask PlayOneDropDelayedAsync(Vector3 worldPos, float delay)
        {
            if (delay > 0f)
                await UniTask.Delay((int)(delay * 1000));
            await PlayCubeFallAndEvaporateAsync(worldPos);
        }

        private async UniTask PlayCubeFallAndEvaporateAsync(Vector3 worldPos)
        {
            Transform root = _worldRoot.EffectsRoot != null ? _worldRoot.EffectsRoot : _worldRoot.BoardRoot;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "LootDrop_stub";
            cube.transform.SetParent(root, false);
            cube.transform.position = worldPos + Vector3.up * FallHeight;
            cube.transform.localScale = Vector3.one * 0.35f;

            // Fall
            Vector3 landPos = worldPos + Vector3.up * LandHeight;
            await cube.transform
                .DOMove(landPos, FallDuration)
                .SetEase(Ease.OutBounce)
                .AsyncWaitForCompletion();

            // Tiny squash on land
            Vector3 scale = cube.transform.localScale;
            await cube.transform
                .DOScale(new Vector3(scale.x * 1.15f, scale.y * 0.7f, scale.z * 1.15f), 0.06f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();
            await cube.transform
                .DOScale(scale, 0.08f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();

            // Evaporate
            await cube.transform
                .DOScale(Vector3.zero, EvaporateDuration)
                .SetEase(Ease.InBack)
                .AsyncWaitForCompletion();

            Object.Destroy(cube);
        }

        private static Vector3 GridToWorld(GridPosition pos)
        {
            return new Vector3(
                pos.Column * CellSize,
                0f,
                pos.Row * CellSize);
        }

        void IGameShutdownCleanup.Cleanup()
        {
            // Stub: nothing to clear; full impl could clear pooled loot views
        }
    }
}
