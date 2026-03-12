using Project.Core.Core.Grid;
using Project.Core.Core.Storm.Core;

namespace Project.Core.Core.Storm.Messages
{
    /// <summary>
    /// Сообщения шторм-зоны (бой, ход, урон, состояние, клетки).
    /// </summary>
    public readonly struct StormMessage
    {
        public const string BATTLE_STARTED = "stormBattleStarted";
        public const string TURN_STARTED = "stormTurnStarted";
        public const string DAMAGE_DEALT = "stormDamageDealt";
        public const string FIGURE_TURN_ENDED = "stormFigureTurnEnded";
        public const string STATE_CHANGED = "stormStateChanged";
        public const string CELLS_UPDATED = "stormCellsUpdated";
        public const string FIGURE_DAMAGE = "stormFigureDamage";

        public readonly string Type;
        public readonly int Turn;
        public readonly IStormDamageTarget Target;
        public readonly int Row;
        public readonly int Col;
        public readonly StormState State;
        public readonly GridPosition[] WarningCells;
        public readonly GridPosition[] DangerCells;
        public readonly int Damage;
        public readonly GridPosition Position;

        private StormMessage(
            string type,
            int turn,
            IStormDamageTarget target,
            int row,
            int col,
            StormState state,
            GridPosition[] warningCells,
            GridPosition[] dangerCells,
            int damage,
            GridPosition position)
        {
            Type = type;
            Turn = turn;
            Target = target;
            Row = row;
            Col = col;
            State = state;
            WarningCells = warningCells;
            DangerCells = dangerCells;
            Damage = damage;
            Position = position;
        }

        public static StormMessage BattleStarted()
        {
            return new StormMessage(BATTLE_STARTED, 0, null, 0, 0, default, null, null, 0, default);
        }

        public static StormMessage TurnStarted(int turn)
        {
            return new StormMessage(TURN_STARTED, turn, null, 0, 0, default, null, null, 0, default);
        }

        public static StormMessage DamageDealt(int turn)
        {
            return new StormMessage(DAMAGE_DEALT, turn, null, 0, 0, default, null, null, 0, default);
        }

        public static StormMessage FigureTurnEnded(IStormDamageTarget target, int row, int col)
        {
            return new StormMessage(FIGURE_TURN_ENDED, 0, target, row, col, default, null, null, 0, default);
        }

        public static StormMessage StateChanged(StormState state)
        {
            return new StormMessage(STATE_CHANGED, 0, null, 0, 0, state, null, null, 0, default);
        }

        public static StormMessage CellsUpdated(GridPosition[] warningCells, GridPosition[] dangerCells)
        {
            return new StormMessage(CELLS_UPDATED, 0, null, 0, 0, default, warningCells, dangerCells, 0, default);
        }

        public static StormMessage FigureDamage(IStormDamageTarget target, int damage, GridPosition position)
        {
            return new StormMessage(FIGURE_DAMAGE, 0, target, 0, 0, default, null, null, damage, position);
        }
    }
}
