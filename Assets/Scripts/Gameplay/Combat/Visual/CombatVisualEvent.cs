using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Loot;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    /// <summary>
    /// Порядок визуала: PreHit → Hit → Death → Loot → Cleanup.
    /// Внутри Stage команды сортируются по OrderInStage.
    /// </summary>
    public enum CombatVisualStage
    {
        PreHit = 0,  // melee: Attack; ranged: FlyProjectile
        Hit = 1,     // melee: Damage; ranged: Impact(0), Cleanup(1), ProjectileHitApply(2) [+ при смерти Death/Loot из очереди]
        Death = 2,   // DeathCommand (HideHpBar → PlayDeath → RemoveFigure)
        Loot = 3,    // LootCommand
        Cleanup = 4  // не используется для projectile (Cleanup в Hit)
    }

    /// <summary>
    /// Domain-side visual event. Planner сортирует по Stage, затем OrderInStage.
    /// Melee: PreHit(Attack) → Hit(Damage) → [Death, Loot при убийстве].
    /// Ranged: PreHit(Fly) → Hit(Impact=0, Cleanup=1, ProjectileHitApply=2) → при убийстве дописываются Death, Loot.
    /// </summary>
    public interface ICombatVisualEvent
    {
        CombatVisualStage Stage { get; }
        int OrderInStage { get; }
    }

    public sealed class DamageVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 0;
        public int TargetId { get; }
        public float Amount { get; }
        public bool IsCritical { get; }
        public string DamageType { get; }

        public DamageVisualEvent(int targetId, float amount, bool isCritical = false, string damageType = null)
        {
            TargetId = targetId;
            Amount = amount;
            IsCritical = isCritical;
            DamageType = damageType;
        }
    }

    public sealed class AttackVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.PreHit;
        public int OrderInStage => 0;
        public int AttackerId { get; }
        public GridPosition TargetPosition { get; }
        public string AttackType { get; }

        public AttackVisualEvent(int attackerId, GridPosition targetPosition, string attackType = null)
        {
            AttackerId = attackerId;
            TargetPosition = targetPosition;
            AttackType = attackType;
        }
    }

    public sealed class ProjectileVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.PreHit;
        public int OrderInStage => 0;
        public int AttackerId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public int TargetId { get; }
        public string ProjectileConfigId { get; }
        public string AttackType { get; }
        public float Damage { get; }
        public bool IsCritical { get; }
        public string ImpactFxId { get; }

        public ProjectileVisualEvent(
            int attackerId,
            GridPosition from,
            GridPosition to,
            int targetId,
            string projectileConfigId,
            float damage,
            bool isCritical,
            string impactFxId = null,
            string attackType = null)
        {
            AttackerId = attackerId;
            From = from;
            To = to;
            TargetId = targetId;
            ProjectileConfigId = projectileConfigId;
            AttackType = attackType;
            Damage = damage;
            IsCritical = isCritical;
            ImpactFxId = impactFxId;
        }
    }

    public sealed class ProjectileImpactEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 0;
        public GridPosition Position { get; }
        public string ImpactFxId { get; }

        public ProjectileImpactEvent(GridPosition position, string impactFxId = null)
        {
            Position = position;
            ImpactFxId = impactFxId;
        }
    }

    public sealed class CleanupProjectileEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 1;
    }

    /// <summary>
    /// Применить урон снаряда после Impact+Cleanup (чтобы HP и лут обновлялись когда пуля уже долетела).
    /// </summary>
    public sealed class ProjectileHitApplyEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 2;
        public int AttackerId { get; }
        public int TargetId { get; }
        public GridPosition TargetPosition { get; }
        public float Damage { get; }
        public bool IsCritical { get; }
        public bool IsDodged { get; }
        public bool IsCancelled { get; }
        public string AttackId { get; }

        public ProjectileHitApplyEvent(int attackerId, int targetId, GridPosition targetPosition,
            float damage, bool isCritical, bool isDodged, bool isCancelled, string attackId)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            TargetPosition = targetPosition;
            Damage = damage;
            IsCritical = isCritical;
            IsDodged = isDodged;
            IsCancelled = isCancelled;
            AttackId = attackId;
        }
    }

    public sealed class BeamVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.PreHit;
        public int OrderInStage => 0;
        public int AttackerId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public int TargetId { get; }
        public string AttackType { get; }

        public BeamVisualEvent(int attackerId, GridPosition from, GridPosition to, int targetId, string attackType = null)
        {
            AttackerId = attackerId;
            From = from;
            To = to;
            TargetId = targetId;
            AttackType = attackType;
        }
    }

    public sealed class WaveVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.PreHit;
        public int OrderInStage => 0;
        public int AttackerId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public int TargetId { get; }
        public string AttackType { get; }

        public WaveVisualEvent(int attackerId, GridPosition from, GridPosition to, int targetId, string attackType = null)
        {
            AttackerId = attackerId;
            From = from;
            To = to;
            TargetId = targetId;
            AttackType = attackType;
        }
    }

    public sealed class HealVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 0;
        public int TargetId { get; }
        public float Amount { get; }

        public HealVisualEvent(int targetId, float amount)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }

    public sealed class PushVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 0;
        public int TargetId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public PushVisualEvent(int targetId, GridPosition from, GridPosition to)
        {
            TargetId = targetId;
            From = from;
            To = to;
        }
    }

    public sealed class MoveVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Hit;
        public int OrderInStage => 0;
        public int FigureId { get; }
        public GridPosition To { get; }

        public MoveVisualEvent(int figureId, GridPosition to)
        {
            FigureId = figureId;
            To = to;
        }
    }

    public sealed class DeathVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Death;
        public int OrderInStage => 0;
        public int FigureId { get; }
        public string Reason { get; }

        public DeathVisualEvent(int figureId, string reason = null)
        {
            FigureId = figureId;
            Reason = reason;
        }
    }

    /// <summary>
    /// Loot dropped at a grid position (e.g. on enemy death). Mapped to LootCommand → LootPresenter.
    /// </summary>
    public sealed class LootVisualEvent : ICombatVisualEvent
    {
        public CombatVisualStage Stage => CombatVisualStage.Loot;
        public int OrderInStage => 0;
        public GridPosition DropPosition { get; }
        public LootResult Loot { get; }

        public LootVisualEvent(GridPosition dropPosition, LootResult loot)
        {
            DropPosition = dropPosition;
            Loot = loot;
        }
    }
}
