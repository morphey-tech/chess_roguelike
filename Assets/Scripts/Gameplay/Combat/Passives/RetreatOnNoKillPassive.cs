using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Скользкий: если не убил цель - отступает на N клеток назад.
    /// </summary>
    public sealed class RetreatOnNoKillPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 40;
        
        private readonly int _retreatDistance;

        public RetreatOnNoKillPassive(string id, int retreatDistance)
        {
            Id = id;
            _retreatDistance = retreatDistance;
        }

        public void OnAfterHit(AfterHitContext context)
        {
            if (context.TargetDied || context.Attacker == null || context.Grid == null)
                return;

            GridPosition attackerPos = context.AttackerPosition;
            GridPosition targetPos = context.TargetPosition;
            
            if (!attackerPos.IsValid || !targetPos.IsValid)
                return;

            int dirRow = attackerPos.Row - targetPos.Row;
            int dirCol = attackerPos.Column - targetPos.Column;
            
            if (dirRow != 0) dirRow = dirRow > 0 ? 1 : -1;
            if (dirCol != 0) dirCol = dirCol > 0 ? 1 : -1;

            for (int i = _retreatDistance; i >= 1; i--)
            {
                GridPosition retreatTo = new(
                    attackerPos.Row + dirRow * i,
                    attackerPos.Column + dirCol * i);

                if (!context.Grid.IsInside(retreatTo))
                    continue;

                BoardCell retreatCell = context.Grid.GetBoardCell(retreatTo);
                if (retreatCell.IsFree)
                {
                    BoardCell fromCell = context.Grid.GetBoardCell(attackerPos);
                    fromCell.RemoveFigure();
                    retreatCell.PlaceFigure(context.Attacker);
                    return;
                }
            }
        }
    }
}
