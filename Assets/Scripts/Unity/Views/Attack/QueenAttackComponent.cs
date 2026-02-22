using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Unity
{
  public class QueenAttackComponent : AttackComponent
  {
    private int _depth = 10;
    private List<Vector2Int> Offsets = new()
    {    
      new Vector2Int(1, 1),
      new Vector2Int(1, -1),
      new Vector2Int(-1, 1),
      new Vector2Int(-1, -1),
      
      new Vector2Int(1, 0),
      new Vector2Int(-1, 0),
      new Vector2Int(0, 1),
      new Vector2Int(0, -1),
    };

    public override List<BoardCell> GetPossibleAttackCells()
    {
      var position = Figure.Position;
      List<BoardCell> result = new List<BoardCell>();

      foreach (Vector2Int offset in Offsets)
      {
        for (int i = 1; i <= _depth; i++)
        {
          GridPosition possiblePosition = new GridPosition(position.Row + offset.x * i, position.Column + offset.y * i);
          if (!Grid.IsInside(possiblePosition))
            break;
                    
          var cell = Grid.GetCell(possiblePosition) as BoardCell;
          if (!cell.IsFree && cell.OccupiedBy2.Team != Figure.Team)
          {
            result.Add(cell);
            break;
          }
        }
      }
            
      return result;
    }
  }
}