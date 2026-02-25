using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Loot;
using Project.Core.Core.Logging;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Economy;
using VContainer;
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
        private readonly IRandomService _random;
        private readonly ILogger _logger;

        private LootTableRepository? _repo;

        /// <summary>Drop rate multiplier for meta-lifetime items (e.g. 0.2 = 20% chance to drop).</summary>
        public float MetaItemDropRate { get; set; } = 1f;

        [Inject]
        private LootService(ConfigProvider configProvider, EconomyService economy, ILogService logService, IRandomService random)
        {
            _configProvider = configProvider;
            _economy = economy;
            _random = random;
            _logger = logService.CreateLogger<LootService>();
        }

        /// <summary>
        /// Ensures loot table config is loaded. Call before combat that may roll loot (e.g. in AttackAction).
        /// </summary>
        public async UniTask EnsureLoadedAsync()
        {
            if (_repo != null)
            {
                return;
            }
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

            LootResult result = new();
            for (int i = 0; i < table.Rolls; i++)
            {
                LootEntryConfig? entry = RollEntry(table.Entries);
                if (entry != null)
                {
                    AddEntryToResult(entry, result);
                }
            }

            if (!result.IsEmpty)
            {
                List<string> parts = new List<string>();
                foreach (ResourceDrop? r in result.Resources)
                {
                    parts.Add($"{r.Id} x{r.Amount}");
                }
                foreach (ItemDrop? i in result.Items)
                {
                    parts.Add($"item:{i.ConfigId}");
                }
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
            if (result is { IsEmpty: false })
            {
                _economy.ApplyLootResult(result);
            }
        }

        private LootEntryConfig? RollEntry(LootEntryConfig[]? entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return null;
            }

            int sum = 0;
            foreach (LootEntryConfig e in entries)
            {
                sum += e.Weight;
            }

            if (sum <= 0)
            {
                return entries[0];
            }

            int roll = _random.Range(0, sum - 1);
            int acc = 0;

            foreach (LootEntryConfig e in entries)
            {
                acc += e.Weight;
                if (roll < acc)
                {
                    return e;
                }
            }

            return entries[0];
        }

        private void AddEntryToResult(LootEntryConfig entry, LootResult result)
        {
            string type = entry.Type?.ToLowerInvariant() ?? "nothing";

            switch (type)
            {
                case "resource":
                    int amount = entry.Min < entry.Max
                        ? _random.Range(entry.Min, entry.Max)
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
