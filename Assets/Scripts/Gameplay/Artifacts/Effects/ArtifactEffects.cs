using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// Старая Корона: Король начинает бой с щитом 5 HP.
    /// Tags: Defense, Battle
    /// </summary>
    public sealed class WornCrownArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly int _shieldValue;

        public WornCrownArtifact(ArtifactConfig config) : base(config)
        {
            _shieldValue = (int)config.Effect.Value;
        }

        public override int Priority => ArtifactPriorities.Low;

        public void OnBattleStart(ArtifactTriggerContext context)
        {
            // TODO: Apply shield to king figure
        }
    }

    /// <summary>
    /// Щит Новичка: Все фигуры +2 щит в начале боя.
    /// Tags: Defense, Battle
    /// </summary>
    public sealed class PawnsGuardArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly int _shieldValue;

        public PawnsGuardArtifact(ArtifactConfig config) : base(config)
        {
            _shieldValue = (int)config.Effect.Value;
        }

        public override int Priority => ArtifactPriorities.Low;

        public void OnBattleStart(ArtifactTriggerContext context)
        {
            // TODO: Apply shield to all team figures
        }
    }

    /// <summary>
    /// Кошелёк Наёмника: +1 крона за победу.
    /// Tags: Economy, Battle
    /// </summary>
    public sealed class MercenariesPouchArtifact : ArtifactBase, IOnBattleEnd
    {
        private readonly int _crownsValue;

        public MercenariesPouchArtifact(ArtifactConfig config) : base(config)
        {
            _crownsValue = (int)config.Effect.Value;
        }

        public override int Priority => ArtifactPriorities.Cleanup;

        public void OnBattleEnd(ArtifactTriggerContext context)
        {
            // TODO: Add crowns to run resources
        }
    }

    /// <summary>
    /// Игральная Кость: 10% шанс уклониться от атаки.
    /// Tags: Utility, Defense
    /// </summary>
    public sealed class GamblersDieArtifact : ArtifactBase, IOnDamageReceived
    {
        private readonly float _dodgeChance;
        private readonly IRandomService _randomService;

        public GamblersDieArtifact(ArtifactConfig config, IRandomService randomService) : base(config)
        {
            _dodgeChance = config.Effect.Value;
            _randomService = randomService;
        }

        public override int Priority => ArtifactPriorities.High;

        public void OnDamageReceived(ArtifactTriggerContext context)
        {
            if (_randomService.Chance(_dodgeChance))
            {
                // TODO: Cancel damage
            }
        }
    }

    /// <summary>
    /// Мина-Ловушка: Смерть фигуры: 3 урона соседям.
    /// Tags: Death, Control
    /// </summary>
    public sealed class AmbushChargeArtifact : ArtifactBase, IOnUnitDeath
    {
        private readonly int _damageValue;
        private readonly int _radius;

        public AmbushChargeArtifact(ArtifactConfig config) : base(config)
        {
            _damageValue = (int)config.Effect.Value;
            _radius = (int)config.Effect.Radius;
        }

        public override int Priority => ArtifactPriorities.Critical;

        public void OnUnitDeath(ArtifactTriggerContext context)
        {
            // TODO: Deal damage to all figures within radius
        }
    }

    /// <summary>
    /// Меч Гроссмейстера: Игнорировать 1 броню врага.
    /// Tags: Attack, Utility
    /// </summary>
    public sealed class GrandmasterBladeArtifact : ArtifactBase
    {
        private readonly int _armorPenetration;

        public GrandmasterBladeArtifact(ArtifactConfig config) : base(config)
        {
            _armorPenetration = (int)config.Effect.Value;
        }

        public override int Priority => ArtifactPriorities.High;

        public int GetArmorPenetration() => _armorPenetration;
    }

    /// <summary>
    /// Ветреные Сапоги: +1 к дальности хода всем фигурам.
    /// Tags: Movement, Utility
    /// </summary>
    public sealed class SwiftStriderArtifact : ArtifactBase
    {
        private readonly int _movementBonus;

        public SwiftStriderArtifact(ArtifactConfig config) : base(config)
        {
            _movementBonus = (int)config.Effect.Value;
        }

        public override int Priority => ArtifactPriorities.Normal;

        public int GetMovementBonus() => _movementBonus;
    }
}
