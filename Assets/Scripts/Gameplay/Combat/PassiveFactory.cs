using Project.Core.Core.Configs.Passive;
using Project.Gameplay.Gameplay.Combat.Passives;

namespace Project.Gameplay.Gameplay.Combat
{
    public static class PassiveFactory
    {
        private const string LIFESTEAL = "lifesteal";
        private const string EXECUTE = "execute";
        private const string CRITICAL = "critical";
        private const string THORNS = "thorns";

        public static IPassive? Create(PassiveConfig config)
        {
            return config.Type switch
            {
                LIFESTEAL => new LifestealPassive(config.Id, config.GetFloat("percent", 0.5f)),
                EXECUTE => new ExecutePassive(config.Id, config.GetFloat("threshold", 0.3f), config.GetFloat("multiplier", 2f)),
                CRITICAL => new CriticalPassive(config.Id, config.GetFloat("chance", 0.2f), config.GetFloat("multiplier", 2f)),
                THORNS => new ThornsPassive(config.Id, config.GetFloat("percent", 0.25f)),
                _ => null
            };
        }
    }
}
