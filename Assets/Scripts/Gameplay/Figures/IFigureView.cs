using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Component on figure prefab that handles visual effects.
    /// Attach to figure prefabs.
    /// </summary>
    public interface IFigureView
    {
        UniTask PlayMoveAsync(Vector3 targetPosition);
        UniTask PlayAttackAsync(Vector3 targetPosition);
        UniTask PlayDeathAsync();
        UniTask PlayHitAsync();
        void SetHighlight(bool enabled);
    }
}
