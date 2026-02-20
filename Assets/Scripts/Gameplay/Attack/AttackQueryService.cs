using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Attack.Rules;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackQueryService : IAttackQueryService
    {
        private readonly AttackRuleService _ruleService;

        [Inject]
        public AttackQueryService(AttackRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        public IReadOnlyCollection<GridPosition> GetTargets(Figure? actor, GridPosition from, BoardGrid? grid)
        {
            if (actor == null || grid == null)
                return new List<GridPosition>();

            return _ruleService.GetValidTargets(actor, from, grid);
        }
    }
}
