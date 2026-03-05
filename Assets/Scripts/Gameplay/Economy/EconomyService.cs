using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Loot;
using UniRx;
using VContainer;

namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Top-level orchestrator for the economy system.
    /// Manages run-scoped resources/items (reset each run) and meta-scoped resources (persistent).
    /// Registered as Singleton at application scope (meta persists across scenes).
    /// </summary>
    public sealed class EconomyService
    {
        /// <summary>Per-run resources (gold, keys, tokens). Cleared on new run.</summary>
        public ResourceStorage RunResources { get; } = new();

        /// <summary>Persistent resources (crystals, premium currency). Survives across runs.</summary>
        public ResourceStorage MetaResources { get; } = new();

        /// <summary>Per-run item inventory (artifacts, consumables). Cleared on new run.</summary>
        public Inventory RunInventory { get; } = new();

        public ItemFactory ItemFactory { get; }

        private readonly ILogger _logger;

        [Inject]
        private EconomyService(ItemFactory itemFactory, ILogService logService)
        {
            ItemFactory = itemFactory;
            _logger = logService.CreateLogger<EconomyService>();
        }

        /// <summary>
        /// Resets all per-run data. Call when starting a new run.
        /// </summary>
        public void StartNewRun()
        {
            RunResources.Clear();
            RunInventory.Clear();
            ItemFactory.ClearCache();
            _logger.Info("Economy: new run started, run resources and inventory cleared");
        }

        /// <summary>
        /// Creates an item from config and adds it to the run inventory.
        /// </summary>
        public async UniTask<Item> AddItemAsync(string configId)
        {
            Item item = await ItemFactory.CreateAsync(configId);
            RunInventory.Add(item);
            _logger.Info($"Item added: {item}");
            return item;
        }

        /// <summary>
        /// Creates an item from config synchronously and adds it to the run inventory.
        /// Requires configs to be preloaded.
        /// </summary>
        public Item AddItemSync(string configId)
        {
            Item item = ItemFactory.CreateSync(configId);
            RunInventory.Add(item);
            _logger.Info($"Item added: {item}");
            return item;
        }

        /// <summary>
        /// Applies a loot result to run economy (resources + items).
        /// Called by LootPresenter after visual, or by apply-only presenter.
        /// </summary>
        public void ApplyLootResult(LootResult? result)
        {
            if (result == null || result.IsEmpty)
            {
                return;
            }

            int resCount = result.Resources.Count;
            int itemCount = result.Items.Count;
            _logger.Info($"Loot received: {resCount} resource(s), {itemCount} item(s)");

            foreach (ResourceDrop? r in result.Resources)
            {
                if (string.IsNullOrEmpty(r.Id) || r.Amount <= 0)
                {
                    continue;
                }

                RunResources.Add(r.Id, r.Amount);
                _logger.Debug($"Loot: +{r.Amount} {r.Id}");
            }

            foreach (ItemDrop? i in result.Items)
            {
                if (string.IsNullOrEmpty(i.ConfigId))
                {
                    continue;
                }

                AddItemSync(i.ConfigId);
            }
        }

        /// <summary>
        /// Collects all passives from items currently in the run inventory.
        /// Used by PassiveTriggerService for global item effects.
        /// </summary>
        public IReadOnlyList<IPassive> GetAllItemPassives()
        {
            return RunInventory.Items
                .SelectMany(item => item.Passives)
                .ToList();
        }

        /// <summary>
        /// Adds crowns to run resources.
        /// </summary>
        public void AddCrowns(int amount)
        {
            if (amount > 0)
            {
                RunResources.Add(ResourceIds.Crowns, amount);
                _logger.Debug($"Crowns added: +{amount}");
            }
        }

        /// <summary>
        /// Spends crowns from run resources. Returns true if successful.
        /// </summary>
        public bool TrySpendCrowns(int amount)
        {
            bool result = RunResources.TrySpend(ResourceIds.Crowns, amount);
            if (result)
            {
                _logger.Debug($"Crowns spent: -{amount}");
            }
            return result;
        }

        /// <summary>
        /// Gets current crowns amount.
        /// </summary>
        public int GetCrowns() => RunResources.Get(ResourceIds.Crowns);

        /// <summary>
        /// Gets reactive property for crowns (for UI binding).
        /// </summary>
        public IReadOnlyReactiveProperty<int> GetCrownsProperty() => 
            RunResources.GetProperty(ResourceIds.Crowns);

        /// <summary>
        /// Adds scrolls to meta resources (persistent).
        /// </summary>
        public void AddScrolls(int amount)
        {
            if (amount > 0)
            {
                MetaResources.Add(ResourceIds.Scrolls, amount);
                _logger.Debug($"Scrolls added: +{amount}");
            }
        }

        /// <summary>
        /// Spends scrolls from meta resources. Returns true if successful.
        /// </summary>
        public bool TrySpendScrolls(int amount)
        {
            bool result = MetaResources.TrySpend(ResourceIds.Scrolls, amount);
            if (result)
            {
                _logger.Debug($"Scrolls spent: -{amount}");
            }
            return result;
        }

        /// <summary>
        /// Gets current scrolls amount.
        /// </summary>
        public int GetScrolls() => MetaResources.Get(ResourceIds.Scrolls);

        /// <summary>
        /// Gets reactive property for scrolls (for UI binding).
        /// </summary>
        public IReadOnlyReactiveProperty<int> GetScrollsProperty() => 
            MetaResources.GetProperty(ResourceIds.Scrolls);
    }
}
