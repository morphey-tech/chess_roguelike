using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Unity.Bootstrap
{
    /// <summary>
    /// Бутстрап игровой сцены.
    /// Просто добавь на сцену — автоматически заспавнит игрока.
    /// </summary>
    public class GameSceneBootstrap : MonoSceneBootstrap
    {
        private const string DefaultSpawnPointId = "player_default";

        protected override void OnConstruct()
        {
        }
        
        protected override async UniTask OnBootstrapAsync()
        {
            await SpawnPlayerAsync();
            await InitializeGameServicesAsync();
        }
        
        private async UniTask SpawnPlayerAsync()
        {
            Log.Debug("Spawning player...");
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
