using Project.Core.Core.Grid;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Gains +damage per cell moved closer to enemy this turn.
    /// Resets each turn.
    /// </summary>
    public sealed class MomentumPassive : IPassive, IOnMove
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Additive;
        public TriggerPhase Phase => TriggerPhase.AfterMove;

        private readonly float _damagePerCell;

        public MomentumPassive(string id, float damagePerCell)
        {
            Id = id;
            _damagePerCell = damagePerCell;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnMove)
            {
                return false;
            }
            if (!context.TryGetData<MoveContext>(out MoveContext move))
            {
                return false;
            }
            return move.DidMove;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<MoveContext>(out MoveContext move))
            {
                return TriggerResult.Continue;
            }

            Figure actor = move.Actor;
            BoardGrid grid = move.Grid;

            GridPosition from = move.From;
            GridPosition to = move.To;
            GridPosition? enemyPos = grid.GetNearestEnemy(actor);

            if (!enemyPos.HasValue)
            {
                return TriggerResult.Continue;
            }

            int oldDist = Attack.AttackUtils.GetDistance(from, enemyPos.Value);
            int newDist = Attack.AttackUtils.GetDistance(to, enemyPos.Value);

            if (newDist >= oldDist)
            {
                return TriggerResult.Continue;
            }

            int moved = oldDist - newDist;
            float bonus = moved * _damagePerCell;
            FlatModifier<float> modifier = new($"{Id}", bonus, 0, 1,
                true, ModifierSourceContext.PreviewCalculation);
            actor.Stats.Attack.AddModifier(modifier);

            return TriggerResult.Continue;
        }
    }
}
