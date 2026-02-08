using System.Linq;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackResolver : IAttackResolver
    {
        private readonly ITargetingService _targeting;
        private readonly IEngagementRuleService _engagement;

        public AttackResolver(ITargetingService targeting, IEngagementRuleService engagement)
        {
            _targeting = targeting;
            _engagement = engagement;
        }

        public AttackProfile Resolve(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (attacker?.Stats?.Attacks == null || attacker.Stats.Attacks.Count == 0)
                return null;

            bool engaged = _engagement.IsEngaged(attacker, grid);

            var candidates = attacker.Stats.Attacks
                .Where(a => !engaged || a.Type == AttackType.Melee)
                .Where(a => _targeting.CanTarget(from, to, a, grid, attacker.Team))
                .ToList();

            if (candidates.Count == 0)
                return null;

            return candidates.OrderByDescending(a => a.Damage).First();
        }
    }
}
