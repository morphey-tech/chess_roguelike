using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    /// <summary>
    /// Domain-side visual event produced by combat effects.
    /// Converted to visual commands by CombatVisualPlanner.
    /// </summary>
    public interface ICombatVisualEvent
    {
    }

    public sealed class DamageVisualEvent : ICombatVisualEvent
    {
        public int TargetId { get; }
        public int Amount { get; }
        public bool IsCritical { get; }
        public string DamageType { get; }

        public DamageVisualEvent(int targetId, int amount, bool isCritical = false, string damageType = null)
        {
            TargetId = targetId;
            Amount = amount;
            IsCritical = isCritical;
            DamageType = damageType;
        }
    }

    public sealed class AttackVisualEvent : ICombatVisualEvent
    {
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
        public int AttackerId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public int TargetId { get; }
        public string ProjectileConfigId { get; }
        public string AttackType { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
        public string ImpactFxId { get; }
        public System.Guid HitId { get; }
        public float Timestamp { get; }

        public ProjectileVisualEvent(
            int attackerId,
            GridPosition from,
            GridPosition to,
            int targetId,
            string projectileConfigId,
            int damage,
            bool isCritical,
            string impactFxId = null,
            System.Guid hitId = default,
            float timestamp = 0,
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
            HitId = hitId;
            Timestamp = timestamp;
        }
    }

    public sealed class BeamVisualEvent : ICombatVisualEvent
    {
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
        public int TargetId { get; }
        public int Amount { get; }

        public HealVisualEvent(int targetId, int amount)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }

    public sealed class PushVisualEvent : ICombatVisualEvent
    {
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
        public int FigureId { get; }
        public string Reason { get; }

        public DeathVisualEvent(int figureId, string reason = null)
        {
            FigureId = figureId;
            Reason = reason;
        }
    }
}
