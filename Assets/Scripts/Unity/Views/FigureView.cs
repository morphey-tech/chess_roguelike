using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Unity.Unity.Views.Presentations;
using UnityEngine;

namespace Project.Unity
{
    public class FigureView : MonoBehaviour, IFigureView
    {
        [SerializeField] private float _maxHp;
        
        private BoardGrid _board;
        public Team Team { get; private set; }
        public GridPosition Position { get; set; }
        public bool Selected { get; set; }
        
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        
        private float _currentHp;
        
        public MoveComponent MoveComponent { get; private set; }
        public AttackComponent AttackComponent { get; private set; }
        public void Init(Team spawnInfoTeam, GridPosition position, BoardGrid boardGrid)
        {
            Team  = spawnInfoTeam;
            _board = boardGrid;
            Position = position;
            MoveComponent = GetComponent<MoveComponent>();
            MoveComponent.Init(this, _board);

            AttackComponent = GetComponent<AttackComponent>();
            AttackComponent.Init(this, _board);
            
            _currentHp = _maxHp;
            GetComponent<FigureHealthPresenter>().Init2(this);
        }

        public void LockSelection(bool isLock)
        {
            
        }

        public void Select(bool isSelect)
        {
            Selected = isSelect;
        }

        public List<BoardCell> GetCellsForMove()
        {
            return MoveComponent.GetPossibleMoveCells();
        }
        
        public List<BoardCell> GetCellsForAttack()
        {
            return AttackComponent.GetPossibleAttackCells();
        }

        public void MoveToPosition(GridPosition gridPosition)
        {
           var prevCell = _board.GetBoardCell(Position);
           prevCell.RemoveFigure();
           
           var newCell = _board.GetBoardCell(gridPosition);
           newCell.PlaceFigure(this);

           Position = gridPosition;
           transform.position = BoardGrid.GetCellTopPosition(gridPosition);
        }
        
        public void Attack(GridPosition gridPosition, float damage)
        {
            var cell = _board.GetBoardCell(gridPosition);
            (cell.OccupiedBy2 as FigureView)?.TakeDamage(damage);
        }

        private void TakeDamage(float damage)
        {
            _currentHp -= damage;
            GetComponent<FigureHitPresenter>().PlayHitAsync().ContinueWith(() =>
            {
                if (_currentHp > 0) 
                    return;
                
                _board.GetBoardCell(Position).RemoveFigure();
                Destroy(gameObject);
            });
        }
    }
}
