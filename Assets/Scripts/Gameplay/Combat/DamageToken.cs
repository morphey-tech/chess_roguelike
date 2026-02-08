using System;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Pending damage token for delayed hits (projectiles, beams, etc).
    /// </summary>
    public sealed class DamageToken
    {
        public Guid Id { get; }
        public int SourceEntityId { get; }
        public int TargetEntityId { get; }
        public GridPosition ExpectedPosition { get; }
        public int RawDamage { get; }
        public float CreatedAt { get; }
        public DeliveryType Delivery { get; }
        public bool IsCritical { get; }
        public string AttackId { get; }
        public bool Consumed { get; private set; }

        public DamageToken(
            Guid id,
            int sourceEntityId,
            int targetEntityId,
            GridPosition expectedPosition,
            int rawDamage,
            float createdAt,
            DeliveryType delivery,
            bool isCritical,
            string attackId)
        {
            Id = id;
            SourceEntityId = sourceEntityId;
            TargetEntityId = targetEntityId;
            ExpectedPosition = expectedPosition;
            RawDamage = rawDamage;
            CreatedAt = createdAt;
            Delivery = delivery;
            IsCritical = isCritical;
            AttackId = attackId;
        }

        public void Consume()
        {
            Consumed = true;
        }
    }
}
