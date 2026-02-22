using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
  public interface IFigureView
  {
    public Team Team { get; }
    GridPosition Position { get; set; }
    void Init(Team spawnInfoTeam, GridPosition position, BoardGrid boardGrid);
    void LockSelection(bool isLock);
    void Select(bool isSelect);
    List<BoardCell> GetCellsForMove();
    void MoveToPosition(GridPosition gridPosition);
    
    List<BoardCell> GetCellsForAttack();
    void Attack(GridPosition gridPosition, float damage);
  }
}