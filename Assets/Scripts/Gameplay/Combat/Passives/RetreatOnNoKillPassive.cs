using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Скользкий: если не убил цель - получает бонусный ход на N клеток.
    /// Игрок сам выбирает куда отступить.
    /// Only triggers when the owner (slippery) is the attacker.
    /// </summary>
    public sealed class RetreatOnNoKillPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        private readonly int _retreatDistance;

        public RetreatOnNoKillPassive(string id, int retreatDistance)
        {
            Id = id;
            _retreatDistance = retreatDistance;
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

            if (afterHit.TargetDied)
            {
                Debug.Log($"[RetreatOnNoKill] {context.Actor}: target died, no bonus move");
                return TriggerResult.Continue;
            }

            afterHit.Effects.Add(new BonusMoveRequestEffect(afterHit.Attacker, _retreatDistance));
            Debug.Log($"[RetreatOnNoKill] {context.Actor}: target survived! Granting bonus move distance={_retreatDistance}");

            return TriggerResult.Continue;
        }
    }
}
