using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Board
{
    /// <summary>
    /// Component on cell prefab that handles visual effects.
    /// Attach to cell prefabs.
    /// </summary>
    public interface IBoardCellView
    {
        UniTask PlayAppearAsync();
        UniTask PlayHitAsync();
        void SetMoveTarget(bool enabled);
        void SetAttackTarget(bool enabled);
    }
}
