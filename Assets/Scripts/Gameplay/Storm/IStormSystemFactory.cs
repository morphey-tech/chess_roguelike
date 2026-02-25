using Cysharp.Threading.Tasks;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Фабрика для создания ZoneShrinkSystem под конкретный конфиг
    /// </summary>
    public interface IStormSystemFactory
    {
        UniTask<StormSystem?> Create(string zoneConfigId);
    }
}