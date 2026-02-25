using Project.Core.Core.Grid;
using Project.Core.Core.Storm.Core;

namespace Project.Core.Core.Storm.Messages
{
    /// <summary>
    /// Сообщение о получении урона юнитом от зоны
    /// </summary>
    public readonly struct FigureTakeStormDamageMessage
    {
        public readonly IStormDamageTarget Target;
        public readonly int Damage;
        public readonly GridPosition Position;

        public FigureTakeStormDamageMessage(IStormDamageTarget target, int damage, GridPosition position)
        {
            Target = target;
            Damage = damage;
            Position = position;
        }
    }
}
