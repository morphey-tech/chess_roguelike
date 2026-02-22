using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Assets;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.UI;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
  public sealed class InfinityPhase : IStagePhase
  {
    private enum TeamAction
    {
      MOVE,
      ATTACK
    }

    private readonly ILogger<InfinityPhase> _logger;
    private readonly IAssetService _assetService;
    private BoardGrid _boardGrid;

    private Dictionary<Team, List<IFigureView>> _figures = new Dictionary<Team, List<IFigureView>>();
    private IFigureView? _selectedFigure;
    private Team _stepTeam;
    
    private List<TeamAction> _actions = new List<TeamAction>() { TeamAction.MOVE, TeamAction.ATTACK };
    private List<TeamAction> _remainingActions = new List<TeamAction>();

    private struct SpawnInfo
    {
      public string FigureAsset;
      public Team Team;
      public GridPosition GridPosition;
    }

    private List<SpawnInfo> _spawnInfos = new()
    {
      // player
      new() { FigureAsset = "pawn_white_view", GridPosition = new GridPosition(2, 3), Team = Team.Player },
      new() { FigureAsset = "pawn_white_view", GridPosition = new GridPosition(2, 4), Team = Team.Player },
      new() { FigureAsset = "pawn_white_view", GridPosition = new GridPosition(2, 5), Team = Team.Player },
      new() { FigureAsset = "pawn_white_view", GridPosition = new GridPosition(2, 6), Team = Team.Player },

      new() { FigureAsset = "bishop_white_view", GridPosition = new GridPosition(1, 3), Team = Team.Player },
      new() { FigureAsset = "bishop_white_view", GridPosition = new GridPosition(1, 6), Team = Team.Player },

      new() { FigureAsset = "queen_white_view", GridPosition = new GridPosition(1, 4), Team = Team.Player },
      new() { FigureAsset = "queen_white_view", GridPosition = new GridPosition(1, 5), Team = Team.Player },

      new() { FigureAsset = "knight_white_view", GridPosition = new GridPosition(0, 4), Team = Team.Player },
      new() { FigureAsset = "knight_white_view", GridPosition = new GridPosition(0, 5), Team = Team.Player },
      new() { FigureAsset = "rook_white_view", GridPosition = new GridPosition(0, 3), Team = Team.Player },
      new() { FigureAsset = "rook_white_view", GridPosition = new GridPosition(0, 6), Team = Team.Player },

      // enemy
      new() { FigureAsset = "pawn_black_view", GridPosition = new GridPosition(7, 3), Team = Team.Enemy },
      new() { FigureAsset = "pawn_black_view", GridPosition = new GridPosition(7, 4), Team = Team.Enemy },
      new() { FigureAsset = "pawn_black_view", GridPosition = new GridPosition(7, 5), Team = Team.Enemy },
      new() { FigureAsset = "pawn_black_view", GridPosition = new GridPosition(7, 6), Team = Team.Enemy },

      new() { FigureAsset = "bishop_black_view", GridPosition = new GridPosition(8, 3), Team = Team.Enemy },
      new() { FigureAsset = "bishop_black_view", GridPosition = new GridPosition(8, 6), Team = Team.Enemy },

      new() { FigureAsset = "queen_black_view", GridPosition = new GridPosition(8, 4), Team = Team.Enemy },
      new() { FigureAsset = "queen_black_view", GridPosition = new GridPosition(8, 5), Team = Team.Enemy },

      new() { FigureAsset = "knight_black_view", GridPosition = new GridPosition(9, 4), Team = Team.Enemy },
      new() { FigureAsset = "knight_black_view", GridPosition = new GridPosition(9, 5), Team = Team.Enemy },
      new() { FigureAsset = "rook_black_view", GridPosition = new GridPosition(9, 3), Team = Team.Enemy },
      new() { FigureAsset = "rook_black_view", GridPosition = new GridPosition(9, 6), Team = Team.Enemy }
    };


    private readonly InteractionController _interractionController;

    public InfinityPhase(IAssetService assetService,
      BoardSpawnService boardSpawnService,
      InteractionController interactionController,
      ISubscriber<FigureSelectedMessage> selectionSubscriber,
      ISubscriber<FigureDeselectedMessage> figureDeselectedSubscriber,
      ILogService logService)
    {
      _assetService = assetService;
      _logger = logService.CreateLogger<InfinityPhase>();
      _interractionController = interactionController;

      DisposableBagBuilder bag = DisposableBag.CreateBuilder();
      selectionSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
      figureDeselectedSubscriber.Subscribe(OnFigureDeselected).AddTo(bag);
    }

    private void OnFigureSelected(FigureSelectedMessage message)
    {
      if (_selectedFigure != null)
        TryMakeActions(message.Position);
      else
        TrySelectFigure(message.Position);
    }

    private void TryMakeActions(GridPosition gridPosition)
    {
      for (var i = 0; i < _remainingActions.Count; i++)
      {
        var action = _remainingActions[i];
        if (TryMakeAction(gridPosition, action))
        {
          RemoveActionFromRemaining(action);
          return;
        }
      }

      if (_remainingActions.Count == _actions.Count)
      {
        if (!TrySelectFigure(gridPosition))
        {
          ClearBoardCellSelections();
          DeselectFigure(_selectedFigure);
        }
      }
      else
      {
        SelectFigure(_selectedFigure);
      }
    }

    private bool TryMakeAction(GridPosition gridPosition, TeamAction action)
    {
      if (action == TeamAction.MOVE)
        return TryMoveSelectedFigure(gridPosition);
      
      return TryAttackSelectedFigure(gridPosition);
    }
    
    private bool CanMakeAction(TeamAction action)
    {
      if (action == TeamAction.MOVE)
        return CanMakeMove();
      
      return CanMakeAttack();
    } 
    
    private bool CanMakeOtherActions()
    {
      foreach (TeamAction action in _remainingActions)
      {
        if (CanMakeAction(action))
          return true;
      }
      
      return false;
    }
    
    

    private bool TrySelectFigure(GridPosition gridPosition)
    {
      bool result = false;
      foreach (var value in _figures)
      {
        foreach (var figure in value.Value)
        {
          bool selected = gridPosition == figure.Position && figure.Team == _stepTeam;
          if (selected)
          {
            SelectFigure(figure);
            result = true;
          }
          else
          {
            figure.Select(selected);
          }
        }
      }
      
      return result;
    }

    private void RemoveActionFromRemaining(TeamAction action)
    {
      _remainingActions.Remove(action);
      if (CanMakeOtherActions())
      {
         SelectFigure(_selectedFigure);
      }
      else
      {
        ClearBoardCellSelections();
        ClearFigureSelections();
        _selectedFigure = null;
        _remainingActions.Clear();
      }
    }
    
    private bool CanMakeMove()
    {
      return _selectedFigure.GetCellsForMove().Count > 0;
    }
    
    private bool CanMakeAttack()
    {
      return _selectedFigure.GetCellsForAttack().Count > 0;
    }
    
    private bool TryMoveSelectedFigure(GridPosition gridPosition)
    {
      var possibleCells = _selectedFigure.GetCellsForMove();
      var cell = _boardGrid.GetBoardCell(gridPosition);
      if (!possibleCells.Contains(cell))
        return false;

      _selectedFigure.MoveToPosition(gridPosition);
      return true;
    }
    
    private bool TryAttackSelectedFigure(GridPosition gridPosition)
    {
      var possibleCells = _selectedFigure.GetCellsForAttack();
      var cell = _boardGrid.GetBoardCell(gridPosition);
      if (!possibleCells.Contains(cell))
        return false;

      _selectedFigure.Attack(gridPosition, 1f);
      return true;
    }
    
    private void SelectFigure(IFigureView figure)
    {
      _selectedFigure = figure;
      _selectedFigure.Select(true);

      ClearBoardCellSelections();

      if (_remainingActions.Contains(TeamAction.MOVE))
      {
        var cellsForMove = _selectedFigure.GetCellsForMove();
        foreach (BoardCell cell in cellsForMove)
          cell.EnsureComponent(new HighlightTag());
      }

      if (_remainingActions.Contains(TeamAction.ATTACK))
      {
        var cellsForAttack = _selectedFigure.GetCellsForAttack();
        foreach (BoardCell cell in cellsForAttack)
          cell.EnsureComponent(new AttackHighlightTag());
      }
    }
    
    private void DeselectFigure(IFigureView figure)
    {
      figure.Select(false);
      _selectedFigure = null;
      ClearBoardCellSelections();
    }

    private void ClearFigureSelections()
    {
      foreach (var pair in _figures)
      {
        foreach (var figure in pair.Value)
          figure.Select(false);
      }
    }

    private void ClearBoardCellSelections()
    {
      foreach (BoardCell cell in _boardGrid.AllCells())
      {
        cell.Del<HighlightTag>();
        cell.Del<AttackHighlightTag>();
      }
    }

    private void OnFigureDeselected(FigureDeselectedMessage message)
    {
    }


    public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
    {
      _boardGrid = context.Grid;
      _interractionController.Configure(_boardGrid); 
      await UIService.ShowAsync<WorldUIWindow>();
      await SpawnFigures();

      _stepTeam = Team.Player;
      while (true)
      {
        _remainingActions.AddRange(_actions);
        await StepTeam(_stepTeam);
        _stepTeam = _stepTeam == Team.Player ? Team.Enemy : Team.Player;
      }
    }

    private async UniTask SpawnFigures()
    {
      foreach (var spawnInfo in _spawnInfos)
      {
        Vector3 position = BoardGrid.GetCellTopPosition(spawnInfo.GridPosition);
        var figure = await _assetService.InstantiateAsync(spawnInfo.FigureAsset, position, Quaternion.identity, null);

        IFigureView figureView = figure.GetComponent<IFigureView>();
        _boardGrid.GetBoardCell(spawnInfo.GridPosition).PlaceFigure(figureView);
        figureView.Init(spawnInfo.Team, spawnInfo.GridPosition, _boardGrid);

        if (!_figures.ContainsKey(spawnInfo.Team))
          _figures.Add(spawnInfo.Team, new List<IFigureView>());

        _figures[spawnInfo.Team].Add(figure.GetComponent<IFigureView>());
      }
    }

    private async UniTask StepTeam(Team team)
    {
      await UniTask.WaitWhile(() => _remainingActions.Count > 0);
    }
  }
}