using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Rules for placement during prepare phase.
    /// Different modes can have different rules.
    /// </summary>
    public interface IPreparePlacementRules
    {
        bool CanPlace(GridPosition position);
    }
}
