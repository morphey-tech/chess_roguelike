using System;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Save.Models
{
    [Serializable]
    public sealed class FigureLocation
    {
        public FigureLocationType Type { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public static FigureLocation InHand() => new() { Type = FigureLocationType.Hand };
        
        public static FigureLocation OnBoard(GridPosition pos) => new()
        {
            Type = FigureLocationType.Board,
            Row = pos.Row,
            Column = pos.Column
        };

        public static FigureLocation Dead() => new() { Type = FigureLocationType.Dead };

        public GridPosition ToGridPosition() => new(Row, Column);
    }
}