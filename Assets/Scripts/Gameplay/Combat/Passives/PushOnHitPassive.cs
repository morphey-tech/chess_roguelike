using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Брутал: толкает цель после удара. Если некуда толкнуть - +N урона.
    /// </summary>
    public sealed class PushOnHitPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 50;
        
        private readonly int _bonusDamageIfBlocked;

        public PushOnHitPassive(string id, int bonusDamageIfBlocked)
        {
            Id = id;
            _bonusDamageIfBlocked = bonusDamageIfBlocked;
        }

        public void OnAfterHit(AfterHitContext context)
        {
            if (context.TargetDied || context.Target == null || context.Grid == null)
                return;

            GridPosition attackerPos = context.AttackerPosition;
            GridPosition targetPos = context.TargetPosition;
            
            if (!attackerPos.IsValid || !targetPos.IsValid)
                return;

            int pushRow = targetPos.Row + (targetPos.Row - attackerPos.Row);
            int pushCol = targetPos.Column + (targetPos.Column - attackerPos.Column);
            GridPosition pushTo = new(pushRow, pushCol);

            if (context.Grid.IsInside(pushTo))
            {
                BoardCell pushCell = context.Grid.GetBoardCell(pushTo);
                if (pushCell.IsFree)
                {
                    BoardCell fromCell = context.Grid.GetBoardCell(targetPos);
                    fromCell.RemoveFigure();
                    pushCell.PlaceFigure(context.Target);
                    return;
                }
            }

            context.Target.Stats.TakeDamage(_bonusDamageIfBlocked);
        }
    }
}
