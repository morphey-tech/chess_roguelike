using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Universal context for all artifact triggers.
    /// </summary>
    public sealed class ArtifactTriggerContext
    {
        /// <summary>
        /// Type of trigger event.
        /// </summary>
        public ArtifactTriggerType Trigger { get; set; }

        /// <summary>
        /// Main actor (killer, attacker, moving figure, etc.).
        /// </summary>
        public Figure? Actor { get; set; }

        /// <summary>
        /// Target of the action (victim, defender, etc.).
        /// </summary>
        public Figure? Target { get; set; }

        /// <summary>
        /// Optional value (damage amount, heal amount, etc.).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Stack count of the triggering artifact (for stacked effects).
        /// </summary>
        public int StackCount { get; set; } = 1;

        /// <summary>
        /// Optional board reference for position-based effects.
        /// </summary>
        public object? Grid { get; set; }

        /// <summary>
        /// Create context for battle start/end events.
        /// </summary>
        public static ArtifactTriggerContext CreateBattle(int teamId = 0)
        {
            return new ArtifactTriggerContext
            {
                Value = teamId
            };
        }

        /// <summary>
        /// Create context for kill/death events.
        /// </summary>
        public static ArtifactTriggerContext CreateKill(Figure killer, Figure victim)
        {
            return new ArtifactTriggerContext
            {
                Actor = killer,
                Target = victim
            };
        }

        /// <summary>
        /// Create context for death events.
        /// </summary>
        public static ArtifactTriggerContext CreateDeath(Figure victim, Figure? killer = null)
        {
            return new ArtifactTriggerContext
            {
                Actor = victim,
                Target = killer
            };
        }

        /// <summary>
        /// Create context for damage events.
        /// </summary>
        public static ArtifactTriggerContext CreateDamage(Figure target, int amount, Figure? source = null)
        {
            return new ArtifactTriggerContext
            {
                Actor = source,
                Target = target,
                Value = amount
            };
        }
    }

    /// <summary>
    /// Types of artifact triggers.
    /// </summary>
    public enum ArtifactTriggerType
    {
        None = 0,
        OnBattleStart = 1,
        OnBattleEnd = 2,
        OnUnitDeath = 3,
        OnUnitKill = 4,
        OnAllyDeath = 5,
        OnDamageReceived = 6,
        OnReward = 7,
        OnMove = 8,
        OnAttack = 9,
        OnTurnStart = 10,
        OnTurnEnd = 11
    }
}
