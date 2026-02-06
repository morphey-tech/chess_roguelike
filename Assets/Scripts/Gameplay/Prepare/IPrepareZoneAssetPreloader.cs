using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Предзагрузка ассетов для зоны подготовки (слоты, фигуры).
    /// Вызывать в BoardSpawnPhase, чтобы к началу PreparePhase ассеты уже были в кэше — убирает задержку между доской и prepare-зоной.
    /// </summary>
    public interface IPrepareZoneAssetPreloader
    {
        UniTask PreloadAsync(PlayerRunStateModel runState);
    }
}
