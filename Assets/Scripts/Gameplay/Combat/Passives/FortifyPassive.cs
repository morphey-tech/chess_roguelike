using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Танк: если не двигался в этом ходу - получает меньше урона.
    /// Only activates when the tank is being attacked (is the target).
    /// </summary>
    public sealed class FortifyPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 100;
        
        private readonly int _damageReduction;

        public FortifyPassive(string id, int damageReduction)
        {
            Id = id;
            _damageReduction = damageReduction;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            // Only reduce damage when THIS figure (owner) is being attacked
            if (owner != context.Target)
            {
                Debug.Log($"[FortifyPassive] Skipped: owner={owner} is not target={context.Target}");
                return;
            }

            if (!context.TargetMovedThisTurn)
            {
                context.BonusDamage -= _damageReduction;
                Debug.Log($"[FortifyPassive] {owner} reduced incoming damage by {_damageReduction}. BonusDamage now: {context.BonusDamage}");
            }
            else
            {
                Debug.Log($"[FortifyPassive] {owner} moved this turn, no reduction");
            }
        }
    }
}
