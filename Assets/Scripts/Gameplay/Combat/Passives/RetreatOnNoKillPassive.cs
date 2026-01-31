using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
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
        public int Priority => 40;
        
        private readonly int _retreatDistance;

        public RetreatOnNoKillPassive(string id, int retreatDistance)
        {
            Id = id;
            _retreatDistance = retreatDistance;
        }

        public void OnAfterHit(Figure owner, AfterHitContext context)
        {
            if (owner != context.Attacker)
            {
                Debug.Log($"[RetreatOnNoKill] Skipped: owner={owner} is not attacker={context.Attacker}");
                return;
            }

            if (context.TargetDied)
            {
                Debug.Log($"[RetreatOnNoKill] {owner}: target died, no bonus move");
                return;
            }

            context.Effects.Add(new BonusMoveRequestEffect(context.Attacker, _retreatDistance));
            Debug.Log($"[RetreatOnNoKill] {owner}: target survived! Granting bonus move distance={_retreatDistance}");
        }
    }
}
