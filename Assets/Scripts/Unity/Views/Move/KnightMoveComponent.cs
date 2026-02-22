using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Unity
{
  public class KnightMoveComponent : MoveComponent
  {
    private List<Vector2Int> Offsets = new()
    {
      new Vector2Int(2, 1),
      new Vector2Int(2, -1),
      new Vector2Int(-2, 1),
      new Vector2Int(-2, -1),
      new Vector2Int(1, 2),
      new Vector2Int(-1, 2),
      new Vector2Int(1, -2),
      new Vector2Int(-1, -2),
    };

    public override List<BoardCell> GetPossibleMoveCells()
    {
      var position = Figure.Position;
      List<BoardCell> result = new List<BoardCell>();

      foreach (Vector2Int offset in Offsets)
      {
        GridPosition possiblePosition = new GridPosition(position.Row + offset.x, position.Column + offset.y);
        if (!Grid.IsInside(possiblePosition))
          continue;

        var cell = Grid.GetCell(possiblePosition) as BoardCell;
        if (!cell.IsFree)
          continue;

        result.Add(cell);
      }

      return result;
    }
  }
}