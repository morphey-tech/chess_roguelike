using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Board;
using UnityEngine;

namespace Project.Unity.Bootstrap
{
    /// <summary>
    /// Бутстрап игровой сцены.
    /// Просто добавь на сцену — автоматически заспавнит игрока.
    /// </summary>
    public class GameSceneBootstrap : MonoSceneBootstrap
    {
        private BoardSpawnService _boardSpawnService;
        
        protected override void OnConstruct()
        {
            _boardSpawnService = Resolve<BoardSpawnService>();
        }
        
        protected override async UniTask OnBootstrapAsync()
        {
            await InitializeGameServicesAsync();
        }
        
        private async UniTask InitializeGameServicesAsync()
        {
            Log.Debug("Initializing game services...");
            await _boardSpawnService.SpawnAsync("0");
            await UniTask.CompletedTask;
        }
    }
}
