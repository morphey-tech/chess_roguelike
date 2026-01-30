using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Танк: если не двигался в этом ходу - получает меньше урона.
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

        public void OnBeforeHit(BeforeHitContext context)
        {
            if (context.Target == null)
                return;

            if (!context.TargetMovedThisTurn)
            {
                context.BonusDamage -= _damageReduction;
            }
        }
    }
}
