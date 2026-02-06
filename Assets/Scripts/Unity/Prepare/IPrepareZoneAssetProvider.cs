using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Отдаёт готовые префабы для prepare-зоны (грузит/кэширует внутри).
    /// Presenter только запрашивает — не знает про Addressables и конфиги.
    /// </summary>
    public interface IPrepareZoneAssetProvider
    {
        UniTask<PrepareZonePrefabs> GetPrefabsAsync(IReadOnlyList<string> figureTypeIds);
    }
}
