using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to create a board cell at the given position.
    /// </summary>
    public sealed class CreateCellCommand : IVisualCommand
    {
        private readonly Entity _entity;
        private readonly GridPosition _pos;
        private readonly string _skinId;

        public string DebugName => $"CreateCell(pos=[{_pos.Row},{_pos.Column}], skin={_skinId})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public CreateCellCommand(Entity entity, GridPosition pos, string skinId)
        {
            _entity = entity;
            _pos = pos;
            _skinId = skinId;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Board.CreateCell(_entity, _pos, _skinId);
        }
    }
}
