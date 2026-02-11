using System.Collections.Generic;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Save.Adapter
{
    /// <summary>
    /// Bridges EconomyService state to/from SaveSnapshot.
    /// Handles both per-run (resources + items) and meta (resources) data.
    /// </summary>
    public sealed class EconomySaveAdapter : ISaveDataProvider, ISaveDataApplier
    {
        private readonly EconomyService _economyService;

        public EconomySaveAdapter(EconomyService economyService)
        {
            _economyService = economyService;
        }

        public void Populate(SaveSnapshot snapshot)
        {
            // Run resources
            Dictionary<string, int> runRes = _economyService.RunResources.GetAll();
            snapshot.RunResources = runRes.Count > 0 ? runRes : null;

            // Run items
            IReadOnlyList<Item> items = _economyService.RunInventory.Items;
            if (items.Count > 0)
            {
                List<ItemState> itemStates = new(items.Count);
                foreach (Item item in items)
                {
                    itemStates.Add(new ItemState(item.Id, item.ConfigId, item.StackCount));
                }
                snapshot.RunItems = itemStates;
            }
            else
            {
                snapshot.RunItems = null;
            }

            // Meta resources
            Dictionary<string, int> metaRes = _economyService.MetaResources.GetAll();
            snapshot.MetaResources = metaRes.Count > 0 ? metaRes : null;
        }

        public void Apply(SaveSnapshot snapshot)
        {
            // Run resources
            _economyService.RunResources.Load(snapshot.RunResources);

            // Run items — recreate from config IDs
            _economyService.RunInventory.Clear();
            if (snapshot.RunItems != null)
            {
                foreach (ItemState state in snapshot.RunItems)
                {
                    try
                    {
                        Item item = _economyService.ItemFactory.CreateSync(state.ConfigId);
                        // Restore the saved instance ID and stack count
                        // We create a new item via factory (to get passives), then fix up state
                        Item restored = new Item(
                            id: state.Id,
                            configId: item.ConfigId,
                            categoryId: item.CategoryId,
                            lifetime: item.Lifetime,
                            name: item.Name,
                            maxStack: item.MaxStack,
                            passives: item.Passives,
                            @params: item.Params);
                        restored.StackCount = state.StackCount;
                        _economyService.RunInventory.Add(restored);
                    }
                    catch
                    {
                        // Skip items whose config no longer exists
                    }
                }
            }

            // Meta resources
            _economyService.MetaResources.Load(snapshot.MetaResources);
        }
    }
}
