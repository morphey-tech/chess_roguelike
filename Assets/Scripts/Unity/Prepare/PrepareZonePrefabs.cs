using System.Collections.Generic;
using UnityEngine;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Готовые префабы для prepare-зоны. Presenter получает их от провайдера, сам не грузит.
    /// </summary>
    public sealed class PrepareZonePrefabs
    {
        public GameObject CellPrefab { get; }
        public GameObject ControllerPrefab { get; }
        public IReadOnlyDictionary<string, GameObject> FigurePrefabsByTypeId { get; }

        public PrepareZonePrefabs(
            GameObject cellPrefab,
            GameObject controllerPrefab,
            IReadOnlyDictionary<string, GameObject> figurePrefabsByTypeId)
        {
            CellPrefab = cellPrefab;
            ControllerPrefab = controllerPrefab;
            FigurePrefabsByTypeId = figurePrefabsByTypeId ?? new Dictionary<string, GameObject>();
        }
    }
}
