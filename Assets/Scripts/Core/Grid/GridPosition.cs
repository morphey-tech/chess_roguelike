namespace Project.Core.Core.Grid
{
    public readonly struct GridPosition
    {
        public readonly int Row;
        public readonly int Column;

        public GridPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}