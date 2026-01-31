using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Брутал: толкает цель после удара. Если некуда толкнуть - +N урона.
    /// Only triggers when the owner attacks.
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

        public void OnAfterHit(Figure owner, AfterHitContext context)
        {
            // Only trigger when the owner is the attacker
            if (owner != context.Attacker)
            {
                Debug.Log($"[PushOnHit] Skipped: owner={owner} is not attacker={context.Attacker}");
                return;
            }

            if (context.TargetDied || context.Target == null || context.Grid == null)
            {
                Debug.Log($"[PushOnHit] Skipped: TargetDied={context.TargetDied}, Target={context.Target}, Grid={context.Grid}");
                return;
            }

            GridPosition attackerPos = context.AttackerPosition;
            GridPosition targetPos = context.TargetPosition;
            
            if (!attackerPos.IsValid || !targetPos.IsValid)
            {
                Debug.Log($"[PushOnHit] Skipped: invalid positions. Attacker=({attackerPos.Row},{attackerPos.Column}), Target=({targetPos.Row},{targetPos.Column})");
                return;
            }

            // Calculate push direction (away from attacker)
            int pushRow = targetPos.Row + (targetPos.Row - attackerPos.Row);
            int pushCol = targetPos.Column + (targetPos.Column - attackerPos.Column);
            GridPosition pushTo = new(pushRow, pushCol);

            Debug.Log($"[PushOnHit] Trying to push {context.Target} from ({targetPos.Row},{targetPos.Column}) to ({pushRow},{pushCol})");

            if (context.Grid.IsInside(pushTo))
            {
                BoardCell pushCell = context.Grid.GetBoardCell(pushTo);
                if (pushCell.IsFree)
                {
                    // Move in grid
                    BoardCell fromCell = context.Grid.GetBoardCell(targetPos);
                    fromCell.RemoveFigure();
                    pushCell.PlaceFigure(context.Target);
                    
                    // Signal for visual update
                    context.TargetPushedTo = pushTo;
                    Debug.Log($"[PushOnHit] {context.Target} pushed to ({pushTo.Row},{pushTo.Column})");
                    return;
                }
                else
                {
                    Debug.Log($"[PushOnHit] Push blocked: cell ({pushTo.Row},{pushTo.Column}) occupied by {pushCell.OccupiedBy}");
                }
            }
            else
            {
                Debug.Log($"[PushOnHit] Push blocked: position ({pushTo.Row},{pushTo.Column}) outside grid");
            }

            // Push blocked - deal bonus damage
            context.Target.Stats.TakeDamage(_bonusDamageIfBlocked);
            context.BonusDamageDealt += _bonusDamageIfBlocked;
            Debug.Log($"[PushOnHit] {context.Target} takes {_bonusDamageIfBlocked} bonus damage (push blocked). HP: {context.Target.Stats.CurrentHp}");
        }
    }
}
