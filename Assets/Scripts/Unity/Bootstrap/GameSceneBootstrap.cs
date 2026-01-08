using Cysharp.Threading.Tasks;
using Project.Core.Spawn;
using UnityEngine;

namespace Project.Unity.Bootstrap
{
    /// <summary>
    /// Бутстрап игровой сцены.
    /// Просто добавь на сцену — автоматически заспавнит игрока.
    /// </summary>
    public class GameSceneBootstrap : MonoSceneBootstrap
    {
        private ISpawnService _spawnService = null!;
        
        private const string DefaultSpawnPointId = "player_default";

        protected override void OnConstruct()
        {
            _spawnService = Resolve<ISpawnService>();
        }
        
        protected override async UniTask OnBootstrapAsync()
        {
            await SpawnPlayerAsync();
            await InitializeGameServicesAsync();
        }
        
        private async UniTask SpawnPlayerAsync()
        {
            Log.Debug("Spawning player...");
            
            string spawnPointId = !string.IsNullOrEmpty(TransitionData?.SpawnPointId) 
                ? TransitionData!.SpawnPointId 
                : DefaultSpawnPointId;
            GameObject? player = await _spawnService.SpawnPlayerAtPointAsync(spawnPointId);
            
            if (player == null && spawnPointId != DefaultSpawnPointId)
            {
                Log.Warning($"Spawn point '{spawnPointId}' not found, trying default");
                player = await _spawnService.SpawnPlayerAtPointAsync(DefaultSpawnPointId);
            }
            
            if (player == null)
            {
                Log.Warning("No spawn points found, spawning from model");
                player = await _spawnService.SpawnPlayerFromModelAsync();
            }
            
            if (player == null)
            {
                Log.Error("Failed to spawn player!");
                return;
            }
            
            Log.Info($"Player spawned: {player.name}");
        }
        
        private async UniTask InitializeGameServicesAsync()
        {
            Log.Debug("Initializing game services...");
            
            // TODO: Загрузка сохранения
            // TODO: Инициализация квестов
            
            await UniTask.CompletedTask;
        }
    }
}
