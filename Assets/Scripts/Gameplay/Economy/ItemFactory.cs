using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Economy;
using Project.Core.Core.Configs.Passive;
using static Project.Core.Core.Configs.Economy.ItemCategories;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Configs;

namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Creates runtime Item instances from ItemConfig.
    /// Loads passive configs and creates IPassive instances via PassiveFactory.
    /// </summary>
    public sealed class ItemFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly ILogger _logger;

        private ItemConfigRepository? _itemConfigCache;
        private PassiveConfigRepository? _passiveConfigCache;

        public ItemFactory(ConfigProvider configProvider, ILogService logService)
        {
            _configProvider = configProvider;
            _logger = logService.CreateLogger<ItemFactory>();
        }

        public async UniTask<Item> CreateAsync(string configId)
        {
            _itemConfigCache ??= await _configProvider.Get<ItemConfigRepository>("items_conf");
            _passiveConfigCache ??= await _configProvider.Get<PassiveConfigRepository>("passives_conf");

            ItemConfig? config = _itemConfigCache.Get(configId);
            if (config == null)
            {
                throw new ArgumentException($"Item config '{configId}' not found");
            }

            return CreateFromConfig(config);
        }

        public Item CreateSync(string configId)
        {
            _itemConfigCache ??= _configProvider.GetSync<ItemConfigRepository>("items_conf");
            _passiveConfigCache ??= _configProvider.GetSync<PassiveConfigRepository>("passives_conf");

            ItemConfig? config = _itemConfigCache.Get(configId);
            if (config == null)
            {
                throw new ArgumentException($"Item config '{configId}' not found");
            }

            return CreateFromConfig(config);
        }

        private Item CreateFromConfig(ItemConfig config)
        {
            List<IPassive> passives = new();

            if (config.Passives != null && _passiveConfigCache != null)
            {
                foreach (string passiveId in config.Passives)
                {
                    PassiveConfig? passiveConfig = _passiveConfigCache.Get(passiveId);
                    if (passiveConfig == null)
                    {
                        _logger.Warning($"Passive config '{passiveId}' not found for item '{config.Id}'");
                        continue;
                    }

                    IPassive? passive = PassiveFactory.Create(passiveConfig);
                    if (passive != null)
                    {
                        passives.Add(passive);
                    }
                }
            }

            string instanceId = Guid.NewGuid().ToString("N")[..8];
            ItemParams itemParams = ItemParams.FromDict(config.Params);

            return new Item(
                id: instanceId,
                configId: config.Id,
                categoryId: config.Category ?? Artifact,
                lifetime: config.ParseLifetime(),
                name: config.Name ?? config.Id,
                maxStack: config.MaxStack,
                passives: passives,
                @params: itemParams);
        }

        public void ClearCache()
        {
            _itemConfigCache = null;
            _passiveConfigCache = null;
        }
    }
}
