using UnityEngine;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Marker component to identify hand figures for click detection.
    /// Attached to instantiated hand figure visuals.
    /// </summary>
    public sealed class HandFigureMarker : MonoBehaviour
    {
        public string FigureId { get; private set; }

        public void Initialize(string unitId)
        {
            FigureId = unitId;
        }
    }
}
