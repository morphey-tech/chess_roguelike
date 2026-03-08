using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Combat.Passives;

namespace Project.Gameplay.Gameplay.Combat
{
    public class PassiveFactory
    {
        private readonly IRandomService _random;

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
        private const string DESPERATION = "desperation";
        private const string ROYAL_PRESENCE = "royal_presence";
        private const string ESCAPE = "escape";
        private const string IMPACT = "impact";
        private const string MELEE_OVERRIDE = "ranger_provocation";
        private const string SPLASH = "splash";
        private const string PIERCE = "pierce";

        public PassiveFactory(IRandomService random)
        {
            _random = random;
        }

        public IPassive? Create(PassiveConfig config)
        {
            return config.Type switch
            {
                SWARM or ATTACK_FOR_ALLY => new SwarmPassive(config.Id, config.GetInt("damage", 1), config.GetInt("duration", 1)),
                LIFE_STEAL => new LifestealPassive(config.Id, config.GetFloat("percent", 0.5f)),
                EXECUTE => new ExecutePassive(config.Id, config.GetFloat("threshold", 0.3f), config.GetFloat("multiplier", 2f)),
                CRITICAL => new CriticalPassive(config.Id, config.GetFloat("chance", 0.2f), config.GetFloat("multiplier", 2f), _random),
                THORNS => new ThornsPassive(config.Id, config.GetFloat("percent", 0.25f)),
                PUSH_ON_HIT => new PushOnHitPassive(config.Id, config.GetInt("bonus_damage", 1)),
                RETREAT_ON_NO_KILL => new RetreatOnNoKillPassive(config.Id, config.GetInt("distance", 1)),
                MOMENTUM => new MomentumPassive(config.Id, config.GetFloat("bonus_damage", 0.5f)),
                AGILE_DODGE => new AgileDodgePassive(config.Id, config.GetFloat("chance", 0.5f), _random),
                FIRST_SHOT => new FirstShotPassive(config.Id, config.GetFloat("bonus_damage", 2f)),
                FURY => new FuryPassive(config.Id, config.GetFloat("damage", 0.5f), config.GetInt("max_stacks", 5)),
                INSPIRATION => new InspirationPassive(
                    config.Id,
                    config.GetFloat("attack_bonus", 2f),
                    config.GetFloat("defence_bonus", 2f),
                    config.GetFloat("evasion_bonus", 0.1f),
                    _random, config.GetInt("duration", 2)),
                PROVOCATION => new ProvocationPassive(),
                DESPERATION => new DesperationPassive(config.Id),
                ROYAL_PRESENCE => new RoyalPresencePassive(config.Id, config.GetFloat("damage_bonus", 1f), config.GetInt("aura_radius", 2)),
                ESCAPE => new EscapePassive(config.Id),
                IMPACT => new ImpactPassive(config.Id, config.GetInt("bonus_damage", 2)),
                MELEE_OVERRIDE => new MeleeOverridePassive(config.Id, config.GetFloat("melee_damage_multiplier", 0.5f)),
                SPLASH => new SplashPassive(config.Id),
                PIERCE => new PiercePassive(config.Id),
                _ => null
            };
        }
    }
}
