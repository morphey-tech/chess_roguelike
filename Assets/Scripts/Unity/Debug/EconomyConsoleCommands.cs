using System;
using System.Text;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Economy;
using VContainer.Unity;

namespace Project.Unity.Unity.Debug
{
    public sealed class EconomyConsoleCommands : IStartable, IDisposable
    {
        private readonly EconomyService _economyService;
        private readonly ILogger _logger;
        private bool _registered;

        public EconomyConsoleCommands(EconomyService economyService, ILogService logService)
        {
            _economyService = economyService;
            _logger = logService.CreateLogger<EconomyConsoleCommands>();
        }

        public void Start()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif

            DebugLogConsole.AddCommand<string, int>(
                "add_resource",
                "Add run resource: add_resource <id> <amount>",
                AddResource);

            DebugLogConsole.AddCommand<string, int>(
                "add_meta_resource",
                "Add meta resource: add_meta_resource <id> <amount>",
                AddMetaResource);

            DebugLogConsole.AddCommand<int>(
                "add_crowns",
                "Add crowns (run currency): add_crowns <amount>",
                AddCrowns);

            DebugLogConsole.AddCommand<int>(
                "add_scrolls",
                "Add scrolls (meta currency): add_scrolls <amount>",
                AddScrolls);

            DebugLogConsole.AddCommand<string>(
                "add_item",
                "Add item to run inventory: add_item <configId>",
                AddItem);

            DebugLogConsole.AddCommand(
                "list_items",
                "List all items in run inventory",
                ListItems);

            DebugLogConsole.AddCommand(
                "list_resources",
                "List all resources (run + meta)",
                ListResources);

            _registered = true;
            _logger.Info("Economy debug commands registered");
        }

        private void AddResource(string id, int amount)
        {
            _economyService.RunResources.Add(id, amount);
            _logger.Info($"Added run resource: {id} +{amount} (total: {_economyService.RunResources.Get(id)})");
        }

        private void AddMetaResource(string id, int amount)
        {
            _economyService.MetaResources.Add(id, amount);
            _logger.Info($"Added meta resource: {id} +{amount} (total: {_economyService.MetaResources.Get(id)})");
        }

        private void AddCrowns(int amount)
        {
            _economyService.AddCrowns(amount);
            _logger.Info($"Added crowns: +{amount} (total: {_economyService.GetCrowns()})");
        }

        private void AddScrolls(int amount)
        {
            _economyService.AddScrolls(amount);
            _logger.Info($"Added scrolls: +{amount} (total: {_economyService.GetScrolls()})");
        }

        private void AddItem(string configId)
        {
            _economyService.AddItemAsync(configId).Forget();
        }

        private void ListItems()
        {
            var items = _economyService.RunInventory.Items;
            if (items.Count == 0)
            {
                _logger.Info("Inventory is empty");
                return;
            }

            StringBuilder sb = new();
            sb.AppendLine($"Inventory ({items.Count} items):");
            foreach (Item item in items)
            {
                sb.AppendLine($"  [{item.CategoryId}] {item.Name} (id={item.Id}, config={item.ConfigId}, stack={item.StackCount}, lifetime={item.Lifetime}, passives={item.Passives.Count})");
            }
            _logger.Info(sb.ToString());
        }

        private void ListResources()
        {
            StringBuilder sb = new();

            var runRes = _economyService.RunResources.GetAll();
            sb.AppendLine($"Run resources ({runRes.Count}):");
            foreach (var kvp in runRes)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }

            var metaRes = _economyService.MetaResources.GetAll();
            sb.AppendLine($"Meta resources ({metaRes.Count}):");
            foreach (var kvp in metaRes)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }

            _logger.Info(sb.ToString());
        }

        public void Dispose()
        {
            if (!_registered) return;

            DebugLogConsole.RemoveCommand("add_resource");
            DebugLogConsole.RemoveCommand("add_meta_resource");
            DebugLogConsole.RemoveCommand("add_crowns");
            DebugLogConsole.RemoveCommand("add_scrolls");
            DebugLogConsole.RemoveCommand("add_item");
            DebugLogConsole.RemoveCommand("list_items");
            DebugLogConsole.RemoveCommand("list_resources");
            _registered = false;
        }
    }
}
