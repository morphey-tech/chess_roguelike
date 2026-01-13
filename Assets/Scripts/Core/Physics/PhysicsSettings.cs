using UnityEngine;

namespace Project.Core.Core.Physics
{
    /// <summary>
    /// Centralized physics settings - layer masks, raycast distances, etc.
    /// Single source of truth for all physics-related constants.
    /// </summary>
    public static class PhysicsSettings
    {
        public const string CellLayerName = "Cell";
        public const string FigureLayerName = "Figure";
        
        private static int? _cellLayerMask;
        private static int? _figureLayerMask;
        
        public static int CellLayerMask => _cellLayerMask ??= LayerMask.GetMask(CellLayerName);
        public static int FigureLayerMask => _figureLayerMask ??= LayerMask.GetMask(FigureLayerName);
        
        public const float DefaultRaycastDistance = 100f;
        public const float CellRaycastHeight = 10f;
    }
}
