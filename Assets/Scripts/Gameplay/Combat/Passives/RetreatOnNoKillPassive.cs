using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Скользкий: если не убил цель - получает бонусный ход на N клеток.
    /// Игрок сам выбирает куда отступить.
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
            // Only trigger bonus move if target survived
            if (context.TargetDied)
                return;

            // Request bonus move - player will choose where to go
            context.BonusMoveDistance = _retreatDistance;
        }
    }
}
