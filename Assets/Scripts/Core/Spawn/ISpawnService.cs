using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Spawn
{
    public interface ISpawnService
    {
        UniTask<GameObject> SpawnPlayerAsync(Vector3 position, Quaternion rotation);
        UniTask<GameObject> SpawnPlayerAtPointAsync(string spawnPointId);
        UniTask<GameObject> SpawnPlayerFromModelAsync();
        void DespawnPlayer();
        void RegisterSpawnPoint(ISpawnPoint spawnPoint);
        void UnregisterSpawnPoint(string id);
        void FindAndRegisterAllSpawnPoints();
        GameObject CurrentPlayer { get; }
        bool HasPlayer { get; }
    }
    
    public interface ISpawnPoint
    {
        string Id { get; }
        Vector3 Position { get; }
        Quaternion Rotation { get; }
    }
}


