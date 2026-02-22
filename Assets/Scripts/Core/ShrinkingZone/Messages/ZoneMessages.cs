using Project.Core.Core.Grid;
using Project.Core.Core.ShrinkingZone.Core;

namespace Project.Core.Core.ShrinkingZone.Messages
{
    /// <summary>
    /// Сообщение об изменении состояния зоны
    /// </summary>
    public readonly struct ZoneStateChangedMessage
    {
        public readonly ZoneState NewState;

        public ZoneStateChangedMessage(ZoneState newState)
        {
            NewState = newState;
        }
    }

    /// <summary>
    /// Сообщение об обновлении клеток зоны
    /// </summary>
    public readonly struct ZoneCellsUpdatedMessage
    {
        public readonly GridPosition[] WarningCells;
        public readonly GridPosition[] DangerCells;

        public ZoneCellsUpdatedMessage(GridPosition[] warningCells, GridPosition[] dangerCells)
        {
            WarningCells = warningCells;
            DangerCells = dangerCells;
        }
    }

    /// <summary>
    /// Сообщение о получении урона юнитом от зоны
    /// </summary>
    public readonly struct FigureTakeZoneDamageMessage
    {
        public readonly IZoneDamageTarget Target;
        public readonly int Damage;
        public readonly GridPosition Position;

        public FigureTakeZoneDamageMessage(IZoneDamageTarget target, int damage, GridPosition position)
        {
            Target = target;
            Damage = damage;
            Position = position;
        }
    }
}
