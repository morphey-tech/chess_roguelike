using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public sealed class ProjectileVisualContext
    {
        public int AttackerId { get; }
        public int TargetEntityId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public string ProjectileConfigId { get; }
        public string AttackType { get; }
        public float Damage { get; }
        public bool IsCritical { get; }
        public string ImpactFxId { get; }

        public ProjectileVisualContext(
            int attackerId,
            int targetEntityId,
            GridPosition from,
            GridPosition to,
            string projectileConfigId,
            float damage,
            bool isCritical,
            string impactFxId = null,
            string attackType = null)
        {
            AttackerId = attackerId;
            TargetEntityId = targetEntityId;
            From = from;
            To = to;
            ProjectileConfigId = projectileConfigId;
            Damage = damage;
            IsCritical = isCritical;
            ImpactFxId = impactFxId;
            AttackType = attackType;
        }
    }
}
