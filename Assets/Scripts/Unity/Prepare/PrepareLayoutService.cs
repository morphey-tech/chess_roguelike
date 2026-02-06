using System.Collections.Generic;
using UnityEngine;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Чистая логика раскладки: только позиции слотов. Без Unity-API, без ассетов.
    /// </summary>
    public sealed class PrepareLayoutService
    {
        private const float CellSize = 1f;
        private const float SlotOffsetZ = -2f;
        private const int MaxSlots = 8;

        public IReadOnlyList<Vector3> BuildLayout(int count)
        {
            var result = new List<Vector3>(count);
            float start = (MaxSlots - count) / 2f;
            for (int i = 0; i < count; i++)
            {
                result.Add(new Vector3(
                    (start + i) * CellSize,
                    0f,
                    SlotOffsetZ));
            }
            return result;
        }
    }
}
