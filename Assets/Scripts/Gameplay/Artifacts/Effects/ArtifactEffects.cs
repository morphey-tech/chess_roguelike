using System.Collections.Generic;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Artifacts.Triggers;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// Старая Корона: Король начинает бой с щитом 5 HP.
    /// </summary>
    public sealed class OldCrownArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly int _shieldValue;

        public OldCrownArtifact(ArtifactConfig config) : base(config)
        {
            _shieldValue = (int)config.Effect.Value;
        }

        public void OnBattleStart(BattleContext context)
        {
            // TODO: Apply shield to king figure
            // king.Shield += _shieldValue;
        }
    }

    /// <summary>
    /// Щит Новичка: Все фигуры +2 щит в начале боя.
    /// </summary>
    public sealed class BeginnerShieldArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly int _shieldValue;

        public BeginnerShieldArtifact(ArtifactConfig config) : base(config)
        {
            _shieldValue = (int)config.Effect.Value;
        }

        public void OnBattleStart(BattleContext context)
        {
            // TODO: Apply shield to all team figures
            // foreach (var figure in team.Figures) figure.Shield += _shieldValue;
        }
    }

    /// <summary>
    /// Кошелёк Наёмника: +1 крона за победу.
    /// </summary>
    public sealed class MercenaryPouchArtifact : ArtifactBase, IOnBattleEnd
    {
        private readonly int _crownsValue;

        public MercenaryPouchArtifact(ArtifactConfig config) : base(config)
        {
            _crownsValue = (int)config.Effect.Value;
        }

        public void OnBattleEnd(BattleContext context)
        {
            // TODO: Add crowns to run resources
            // economyService.RunResources.Add(ResourceIds.Crowns, _crownsValue);
        }
    }

    /// <summary>
    /// Игральная Кость: 10% шанс уклониться от атаки.
    /// </summary>
    public sealed class DiceArtifact : ArtifactBase
    {
        private readonly float _dodgeChance;
        private readonly IRandomService _randomService;

        public DiceArtifact(ArtifactConfig config, IRandomService randomService) : base(config)
        {
            _dodgeChance = config.Effect.Value;
            _randomService = randomService;
        }

        // Passive - checked during combat resolution
        public bool TryDodge()
        {
            return _randomService.Chance(_dodgeChance);
        }
    }

    /// <summary>
    /// Мина-Ловушка: Смерть фигуры: 3 урона соседям.
    /// </summary>
    public sealed class TrapMineArtifact : ArtifactBase, IOnUnitDeath
    {
        private readonly int _damageValue;
        private readonly int _radius;

        public TrapMineArtifact(ArtifactConfig config) : base(config)
        {
            _damageValue = (int)config.Effect.Value;
            _radius = (int)config.Effect.Radius;
        }

        public void OnUnitDeath(DeathContext context)
        {
            // TODO: Deal damage to all figures within radius of death position
            // var neighbors = board.GetNeighbors(context.VictimId, _radius);
            // foreach (var neighbor in neighbors) neighbor.TakeDamage(_damageValue);
        }
    }

    /// <summary>
    /// Меч Гроссмейстера: Игнорировать 1 броню врага.
    /// </summary>
    public sealed class GrandmasterSwordArtifact : ArtifactBase
    {
        private readonly int _armorPenetration;

        public GrandmasterSwordArtifact(ArtifactConfig config) : base(config)
        {
            _armorPenetration = (int)config.Effect.Value;
        }

        // Passive - checked during damage calculation
        public int CalculateDamage(int baseDamage, int targetArmor)
        {
            int effectiveArmor = Mathf.Max(0, targetArmor - _armorPenetration);
            return baseDamage - effectiveArmor;
        }
    }

    /// <summary>
    /// Ветреные Сапоги: +1 к дальности хода всем фигурам.
    /// </summary>
    public sealed class WindBootsArtifact : ArtifactBase
    {
        private readonly int _movementBonus;

        public WindBootsArtifact(ArtifactConfig config) : base(config)
        {
            _movementBonus = (int)config.Effect.Value;
        }

        // Passive - checked during movement calculation
        public int GetMovementRange(int baseRange)
        {
            return baseRange + _movementBonus;
        }
    }
}
