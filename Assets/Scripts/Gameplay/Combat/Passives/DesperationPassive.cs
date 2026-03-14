using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class DesperationPassive : IPassive, IOnBeforeHit, IAttackFilter, IAttackRangeModifier
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.High;
        public TriggerGroup Group => TriggerGroup.First;
        public TriggerPhase Phase => TriggerPhase.BeforeCalculation;

        private readonly ILogger<DesperationPassive> _logger;

        public DesperationPassive(string id, ILogService logService)
        {
            Id = id;
            _logger = logService.CreateLogger<DesperationPassive>();
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (!context.TryGetData(out BeforeHitContext? beforeHit))
            {
                return false;
            }
            int allies = beforeHit.Grid.CountAlliesAround(beforeHit.Attacker);
            return allies == 0;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext damageContext)
            {
                return TriggerResult.Continue;
            }
            return HandleBeforeHit(damageContext);
        }

        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            if (context.TryGetData(out BeforeHitContext? beforeHit))
            {
                int allies = beforeHit.Grid.CountAlliesAround(beforeHit.Attacker);

                if (allies == 0)
                {
                    Figure owner = beforeHit.Attacker;
                    owner.Stats.Attack.RemoveModifiersById(Id);

                    float currentAttack = owner.Stats.Attack.Value;
                    float delta = 1f - currentAttack;

                    CombatFlatModifier modifier = new CombatFlatModifier(Id, delta, 0, 1, false);
                    owner.Stats.Attack.AddModifier(modifier);
                }
            }

            return TriggerResult.Continue;
        }

        public void FilterTargets(List<GridPosition> targets, AttackContext context)
        {
            if (!HasActiveDesperation(context.Attacker, context.Grid))
            {
                _logger.Debug($"DesperationPassive.FilterTargets: {context.Attacker.Id} has allies nearby, skipping");
                return;
            }

            int addedCount = 0;

            for (int row = -1; row <= 1; row++)
            {
                for (int col = -1; col <= 1; col++)
                {
                    if (row == 0 && col == 0)
                        continue;

                    GridPosition pos = new GridPosition(context.From.Row + row, context.From.Column + col);
                    if (context.Grid.IsInside(pos) && !targets.Contains(pos))
                    {
                        BoardCell cell = context.Grid.GetBoardCell(pos);
                        if (cell.OccupiedBy != null && cell.OccupiedBy.Team != context.Attacker.Team)
                        {
                            targets.Add(pos);
                            addedCount++;
                        }
                    }
                }
            }

            _logger.Debug($"DesperationPassive.FilterTargets: {context.Attacker.Id} added {addedCount} targets");
        }

        public bool CanAttackCell(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!HasActiveDesperation(attacker, grid))
                return false;

            int distance = AttackUtils.GetDistance(from, to);
            return distance == 1;
        }

        private static bool HasActiveDesperation(Figure figure, BoardGrid grid)
        {
            return grid.CountAlliesAround(figure) == 0;
        }
    }
}
