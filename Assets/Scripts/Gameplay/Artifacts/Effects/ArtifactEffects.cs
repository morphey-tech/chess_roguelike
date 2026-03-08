using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Random;
using Project.Core.Core.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    #region Battle Start

    /// <summary>
    /// Старая Корона: Король начинает бой с щитом 5 HP.
    /// </summary>
    public sealed class WornCrownArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly int _shieldValue;

        public WornCrownArtifact(ArtifactConfig config) : base(config)
        {
            _shieldValue = (int)config.Effect.Value;
        }

        public override int Priority => TriggerPriorities.Low;

        public override bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnBattleStart;
        }

        public TriggerResult Execute(IBattleContext context)
        {
            return HandleBattleStart(context);
        }

        public TriggerResult HandleBattleStart(IBattleContext context)
        {
            // TODO: Apply shield to king figure
            // context.Actor is the king figure
            return TriggerResult.Continue;
        }
    }

    /// <summary>
    /// Щит Новичка: Все фигуры +2 щит в начале боя.
    /// </summary>
    public sealed class PawnsGuardArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly int _shieldValue;

        public PawnsGuardArtifact(ArtifactConfig config) : base(config)
        {
            _shieldValue = (int)config.Effect.Value;
        }

        public override int Priority => TriggerPriorities.Low;

        public override bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnBattleStart;
        }

        public TriggerResult Execute(IBattleContext context)
        {
            return HandleBattleStart(context);
        }

        public TriggerResult HandleBattleStart(IBattleContext context)
        {
            // TODO: Apply shield to all team figures
            return TriggerResult.Continue;
        }
    }

    #endregion

    #region Battle End

    /// <summary>
    /// Кошелёк Наёмника: +1 крона за победу.
    /// </summary>
    public sealed class MercenariesPouchArtifact : ArtifactBase, IOnBattleEnd
    {
        private readonly int _crownsValue;

        public MercenariesPouchArtifact(ArtifactConfig config) : base(config)
        {
            _crownsValue = (int)config.Effect.Value;
        }

        public override int Priority => TriggerPriorities.Cleanup;

        public override bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnBattleEnd;
        }

        public TriggerResult Execute(IBattleContext context)
        {
            return HandleBattleEnd(context);
        }

        public TriggerResult HandleBattleEnd(IBattleContext context)
        {
            // TODO: Add crowns to run resources
            return TriggerResult.Continue;
        }
    }

    #endregion

    #region Damage Received

    /// <summary>
    /// Игральная Кость: 10% шанс уклониться от атаки.
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

        public override int Priority => TriggerPriorities.High;

        public override bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnDamageReceived;
        }

        public TriggerResult Execute(IDamageContext context)
        {
            return HandleDamageReceived(context);
        }

        public TriggerResult HandleDamageReceived(IDamageContext context)
        {
            if (_randomService.Chance(_dodgeChance))
            {
                // TODO: Cancel damage via context
                // context.Value = damage amount
                // context.Target = figure taking damage
                return TriggerResult.Cancel; // Cancel the damage
            }
            return TriggerResult.Continue;
        }
    }

    #endregion

    #region Unit Death

    /// <summary>
    /// Мина-Ловушка: Смерть фигуры: 3 урона соседям.
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

        public override int Priority => TriggerPriorities.Critical;

        public override bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnUnitDeath;
        }

        public TriggerResult Execute(IKillContext context)
        {
            return HandleUnitDeath(context);
        }

        public TriggerResult HandleUnitDeath(IKillContext context)
        {
            // TODO: Deal damage to all figures within radius
            // context.Target = died figure
            return TriggerResult.Continue;
        }
    }

    #endregion

    #region Passive (No Trigger)

    /// <summary>
    /// Меч Гроссмейстера: Игнорировать 1 броню врага.
    /// </summary>
    public sealed class GrandmasterBladeArtifact : ArtifactBase
    {
        private readonly int _armorPenetration;

        public GrandmasterBladeArtifact(ArtifactConfig config) : base(config)
        {
            _armorPenetration = (int)config.Effect.Value;
        }

        public override int Priority => TriggerPriorities.High;

        public override bool Matches(TriggerContext context)
        {
            return false; // Passive effect - queried directly via GetArmorPenetration()
        }

        public override TriggerResult Execute(TriggerContext context)
        {
            // Passive effect - not triggered
            return TriggerResult.Continue;
        }

        public int GetArmorPenetration() => _armorPenetration;
    }

    /// <summary>
    /// Ветреные Сапоги: +1 к дальности хода всем фигурам.
    /// </summary>
    public sealed class SwiftStriderArtifact : ArtifactBase
    {
        private readonly int _movementBonus;

        public SwiftStriderArtifact(ArtifactConfig config) : base(config)
        {
            _movementBonus = (int)config.Effect.Value;
        }

        public override int Priority => TriggerPriorities.Normal;

        public override bool Matches(TriggerContext context)
        {
            return false; // Passive effect - queried directly via GetMovementBonus()
        }

        public override TriggerResult Execute(TriggerContext context)
        {
            // Passive effect - not triggered
            return TriggerResult.Continue;
        }

        public int GetMovementBonus() => _movementBonus;
    }

    #endregion
}
