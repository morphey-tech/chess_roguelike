using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Loot;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Economy;
using UnityEngine;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Loot
{
    /// <summary>
    /// Rolls loot tables. Returns LootResult (for visual pipeline) or applies via RollAsync (non-combat deaths).
    /// </summary>
    public sealed class LootService
    {
        private readonly ConfigProvider _configProvider;
        private readonly EconomyService _economy;
        private readonly ILogger _logger;

        private LootTableRepository? _repo;

        /// <summary>Drop rate multiplier for meta-lifetime items (e.g. 0.2 = 20% chance to drop).</summary>
        public float MetaItemDropRate { get; set; } = 1f;

        public LootService(ConfigProvider configProvider, EconomyService economy, ILogService logService)
        {
            _configProvider = configProvider;
            _economy = economy;
            _logger = logService.CreateLogger<LootService>();
        }

        /// <summary>
        /// Ensures loot table config is loaded. Call before combat that may roll loot (e.g. in AttackAction).
        /// </summary>
        public async UniTask EnsureLoadedAsync()
        {
            if (_repo != null) return;
            _repo = await _configProvider.Get<LootTableRepository>("loot_tables_conf");
        }

        /// <summary>
        /// Rolls the table synchronously and returns the result. Does not apply to economy.
        /// Requires EnsureLoadedAsync() to have been called (e.g. at start of attack step).
        /// </summary>
        public LootResult? Roll(string lootTableId)
        {
            if (string.IsNullOrEmpty(lootTableId))
            {
                _logger.Warning("Loot table id is empty");
                return null;
            }

            if (_repo == null)
            {
                _logger.Warning("Loot tables not loaded; call EnsureLoadedAsync first");
                return null;
            }

            LootTableConfig? table = _repo.Get(lootTableId);
            if (table == null)
            {
                _logger.Warning($"Loot table not found: {lootTableId}");
                return null;
            }

            var result = new LootResult();
            for (int i = 0; i < table.Rolls; i++)
            {
                LootEntryConfig? entry = RollEntry(table.Entries);
                if (entry != null)
                    AddEntryToResult(entry, result);
            }

            if (!result.IsEmpty)
            {
                var parts = new List<string>();
                foreach (var r in result.Resources)
                    parts.Add($"{r.Id} x{r.Amount}");
                foreach (var i in result.Items)
                    parts.Add($"item:{i.ConfigId}");
                _logger.Info($"Loot dropped [{lootTableId}]: {string.Join(", ", parts)}");
            }

            return result;
        }

        /// <summary>
        /// Rolls and applies to economy. Used for non-combat deaths (EnemyDeathLootHandler).
        /// </summary>
        public async UniTask RollAsync(string lootTableId)
        {
            await EnsureLoadedAsync();
            LootResult? result = Roll(lootTableId);
            if (result != null && !result.IsEmpty)
                _economy.ApplyLootResult(result);
        }

        private static LootEntryConfig? RollEntry(LootEntryConfig[] entries)
        {
            if (entries == null || entries.Length == 0) return null;

            int sum = 0;
            foreach (LootEntryConfig e in entries)
                sum += e.Weight;

            if (sum <= 0) return entries[0];

            int roll = UnityEngine.Random.Range(0, sum);
            int acc = 0;

            foreach (LootEntryConfig e in entries)
            {
                acc += e.Weight;
                if (roll < acc)
                    return e;
            }

            return entries[0];
        }

        private static void AddEntryToResult(LootEntryConfig entry, LootResult result)
        {
            string type = entry.Type?.ToLowerInvariant() ?? "nothing";

            switch (type)
            {
                case "resource":
                    int amount = entry.Min < entry.Max
                        ? UnityEngine.Random.Range(entry.Min, entry.Max + 1)
                        : entry.Min;
                    if (amount > 0 && !string.IsNullOrEmpty(entry.Id))
                        result.Resources.Add(new ResourceDrop(entry.Id, amount));
                    break;

                case "item":
                    if (!string.IsNullOrEmpty(entry.Id))
                        result.Items.Add(new ItemDrop(entry.Id));
                    break;

                case "nothing":
                default:
                    break;
            }
        }
    }
}
