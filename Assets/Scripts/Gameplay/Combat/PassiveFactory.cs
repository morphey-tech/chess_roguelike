using Project.Core.Core.Configs.Passive;
using Project.Gameplay.Gameplay.Combat.Passives;

namespace Project.Gameplay.Gameplay.Combat
{
    public static class PassiveFactory
    {
        private const string SWARM = "swarm";
        private const string ATTACK_FOR_ALLY = "attack_for_ally";
        private const string LIFE_STEAL = "lifesteal";
        private const string EXECUTE = "execute";
        private const string CRITICAL = "critical";
        private const string THORNS = "thorns";
        private const string PUSH_ON_HIT = "push_on_hit";
        private const string RETREAT_ON_NO_KILL = "retreat_on_no_kill";
        private const string MOMENTUM = "momentum";
        private const string AGILE_DODGE = "agile_dodge";
        private const string FIRST_SHOT = "first_shot";
        private const string FURY = "fury";
        private const string INSPIRATION = "inspiration";
        private const string PROVOCATION = "provocation";

        public static IPassive? Create(PassiveConfig config)
        {
            return config.Type switch
            {
                SWARM or ATTACK_FOR_ALLY => new SwarmPassive(config.Id, config.GetFloat("percent", 5f), config.GetInt("duration", 1)),
                LIFE_STEAL => new LifestealPassive(config.Id, config.GetFloat("percent", 0.5f)),
                EXECUTE => new ExecutePassive(config.Id, config.GetFloat("threshold", 0.3f), config.GetFloat("multiplier", 2f)),
                CRITICAL => new CriticalPassive(config.Id, config.GetFloat("chance", 0.2f), config.GetFloat("multiplier", 2f)),
                THORNS => new ThornsPassive(config.Id, config.GetFloat("percent", 0.25f)),
                PUSH_ON_HIT => new PushOnHitPassive(config.Id, config.GetInt("bonus_damage", 1)),
                RETREAT_ON_NO_KILL => new RetreatOnNoKillPassive(config.Id, config.GetInt("distance", 1)),
                MOMENTUM => new MomentumPassive(config.Id, config.GetFloat("bonus_damage", 0.5f)),
                AGILE_DODGE => new AgileDodgePassive(config.Id, config.GetFloat("chance", 0.5f)),
                FIRST_SHOT => new FirstShotPassive(config.Id, config.GetFloat("bonus_damage", 2f)),
                FURY => new FuryPassive(config.Id, config.GetFloat("damage", 0.5f), config.GetInt("max_stacks", 5)),
                INSPIRATION => new InspirationPassive(
                    config.Id,
                    config.GetFloat("attack_bonus", 2f),
                    config.GetFloat("defence_bonus", 2f),
                    config.GetFloat("evasion_bonus", 0.1f),
                    config.GetInt("duration", 2)),
                PROVOCATION => new ProvocationPassive(config.Id),
                _ => null
            };
        }
    }
}
