using System;

namespace Project.Core.Core.Grid
{
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public static readonly GridPosition Invalid = new(-1, -1);
        
        public readonly int Row;
        public readonly int Column;

        public bool IsValid => Row >= 0 && Column >= 0;

        public GridPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool Equals(GridPosition other) => Row == other.Row && Column == other.Column;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Row, Column);
        
        public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
        public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);
    }
}