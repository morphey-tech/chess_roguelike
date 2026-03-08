using Project.Core.Core.Grid;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
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
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        private readonly int _bonusDamageIfBlocked;

        public PushOnHitPassive(string id, int bonusDamageIfBlocked)
        {
            Id = id;
            _bonusDamageIfBlocked = bonusDamageIfBlocked;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnAfterHit)
            {
                return false;
            }
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return false;
            }
            return context.Actor == afterHit.Attacker;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return TriggerResult.Continue;
            }

            if (afterHit.TargetDied || afterHit.Target == null || afterHit.Grid == null)
            {
                Debug.Log($"[PushOnHit] Skipped: TargetDied={afterHit.TargetDied}, Target={afterHit.Target}, Grid={afterHit.Grid}");
                return TriggerResult.Continue;
            }

            GridPosition attackerPos = afterHit.AttackerPosition;
            GridPosition targetPos = afterHit.TargetPosition;

            if (!attackerPos.IsValid || !targetPos.IsValid)
            {
                Debug.Log($"[PushOnHit] Skipped: invalid positions");
                return TriggerResult.Continue;
            }

            // Calculate push direction (away from attacker)
            (int dirRow, int dirCol) = afterHit.GetAttackDirection();
            GridPosition pushTo = new(targetPos.Row + dirRow, targetPos.Column + dirCol);

            Debug.Log($"[PushOnHit] Trying to push {afterHit.Target} from ({targetPos.Row},{targetPos.Column}) to ({pushTo.Row},{pushTo.Column})");

            if (afterHit.Grid.IsInside(pushTo))
            {
                BoardCell pushCell = afterHit.Grid.GetBoardCell(pushTo);
                if (pushCell.IsFree)
                {
                    // Move in grid (state change)
                    afterHit.Grid.RemoveFigure(afterHit.Target);
                    afterHit.Grid.PlaceFigure(afterHit.Target, pushTo);

                    // Add effect for visual update
                    afterHit.Effects.Add(new PushEffect(afterHit.Attacker, afterHit.Target, targetPos, pushTo));
                    Debug.Log($"[PushOnHit] {afterHit.Target} pushed to ({pushTo.Row},{pushTo.Column})");
                    return TriggerResult.Continue;
                }

                Debug.Log($"[PushOnHit] Push blocked: cell occupied by {pushCell.OccupiedBy}");
            }
            else
            {
                Debug.Log($"[PushOnHit] Push blocked: position outside grid");
            }

            // Push blocked - deal bonus damage
            afterHit.Effects.Add(new BonusDamageEffect(afterHit.Attacker, afterHit.Target, _bonusDamageIfBlocked, "push blocked"));
            Debug.Log($"[PushOnHit] {afterHit.Target} takes {_bonusDamageIfBlocked} bonus damage (push blocked)");

            return TriggerResult.Continue;
        }
    }
}
