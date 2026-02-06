using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Кэш префабов prepare-зоны. Заполняется во время появления доски (WarmAsync),
    /// отдаёт префабы синхронно (TryGet) — без повторной загрузки и без await.
    /// Один контракт: «заполни набор» и «отдай по запросу».
    /// </summary>
    public interface IPrepareZonePrefabCache
    {
        /// <summary>
        /// Загружает и кэширует префабы для указанных типов фигур.
        /// Вызывать параллельно с появлением доски (BoardSpawnPhase).
        /// </summary>
        UniTask WarmAsync(IReadOnlyList<string> figureTypeIds);

        /// <summary>
        /// Отдаёт закэшированные префабы, если кэш заполнен для запрошенных typeIds.
        /// Синхронно, без await. Возвращает false, если кэш холодный или не подходит.
        /// </summary>
        bool TryGet(IReadOnlyList<string> figureTypeIds, out PrepareZonePrefabs prefabs);

        /// <summary>
        /// Очистить кэш (например при смене стейджа/руны).
        /// </summary>
        void Clear();
    }
}
