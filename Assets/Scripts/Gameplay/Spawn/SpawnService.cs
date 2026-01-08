using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Assets;
using Project.Core.Logging;
using Project.Core.Player;
using Project.Core.Spawn;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Spawn
{
    public class SpawnService : ISpawnService, IDisposable
    {
        private const string PlayerPrefabAddress = "PlayerController";
        
        private readonly IObjectResolver _resolver;
        private readonly IAssetService _assetService;
        private readonly PlayerModel _playerModel;
        private readonly Dictionary<string, ISpawnPoint> _spawnPoints = new();
        private readonly ILogger _logger;
        
        private GameObject _playerInstance;
        private bool _disposed;
        
        public GameObject CurrentPlayer => _playerInstance;
        public bool HasPlayer => _playerInstance != null;
        
        [Inject]
        public SpawnService(IObjectResolver resolver, IAssetService assetService,
            PlayerModel playerModel, ILogService logService)
        {
            _resolver = resolver;
            _assetService = assetService;
            _playerModel = playerModel;
            _logger = logService.CreateLogger<SpawnService>();
            
            _logger.Info("Initialized");
        }
        
        public void RegisterSpawnPoint(ISpawnPoint spawnPoint)
        {
            _spawnPoints[spawnPoint.Id] = spawnPoint;
            _logger.Debug($"Registered spawn point: {spawnPoint.Id}");
        }
        
        public void UnregisterSpawnPoint(string id)
        {
            _spawnPoints.Remove(id);
            _logger.Debug($"Unregistered spawn point: {id}");
        }
        
        /// <summary>
        /// Находит и регистрирует все SpawnPoint'ы на сцене.
        /// Вызывайте этот метод в бутстрапе сцены.
        /// </summary>
        public void FindAndRegisterAllSpawnPoints()
        {
            var spawnPoints = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                UnityEngine.FindObjectsSortMode.None);
            
            int count = 0;
            foreach (var mb in spawnPoints)
            {
                if (mb is ISpawnPoint spawnPoint)
                {
                    RegisterSpawnPoint(spawnPoint);
                    count++;
                }
            }
            
            _logger.Info($"Found and registered {count} spawn points");
        }
        
        public async UniTask<GameObject> SpawnPlayerAsync(Vector3 position, Quaternion rotation)
        {
            ThrowIfDisposed();
            
            if (_playerInstance != null)
            {
                DespawnPlayer();
            }
            
            _logger.Info($"Spawning player at {position}");
            _playerInstance = await _assetService.InstantiateAsync(PlayerPrefabAddress, position, rotation);
            
            if (_playerInstance == null)
            {
                _logger.Error($"Failed to spawn player from: {PlayerPrefabAddress}");
                return null;
            }
            
            _resolver.InjectGameObject(_playerInstance);
            _playerModel.SetPosition(position);
            _playerModel.SetRotation(rotation.eulerAngles.y);
            
            _logger.Info($"Player spawned on scene: {SceneManager.GetActiveScene().name}");
            return _playerInstance;
        }
        
        public async UniTask<GameObject> SpawnPlayerAtPointAsync(string spawnPointId)
        {
            if (!_spawnPoints.TryGetValue(spawnPointId, out ISpawnPoint spawnPoint))
            {
                _logger.Error($"Spawn point not found: {spawnPointId}");
                return null;
            }
            
            return await SpawnPlayerAsync(spawnPoint.Position, spawnPoint.Rotation);
        }
        
        public async UniTask<GameObject> SpawnPlayerFromModelAsync()
        {
            Vector3 position = _playerModel.Position.Value;
            Quaternion rotation = Quaternion.Euler(0f, _playerModel.RotationY.Value, 0f);
            
            _logger.Debug($"Spawning from model at {position}");
            return await SpawnPlayerAsync(position, rotation);
        }
        
        public void DespawnPlayer()
        {
            if (_playerInstance != null)
            {
                _logger.Info("Despawning player");
                _assetService.ReleaseInstance(_playerInstance);
                _playerInstance = null;
            }
        }
        
        public async UniTask PreloadPlayerAsync()
        {
            _logger.Debug("Preloading player prefab");
            await _assetService.PreloadAsync(PlayerPrefabAddress);
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SpawnService));
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            DespawnPlayer();
            _spawnPoints.Clear();
            _logger.Info("Disposed");
        }
    }
}


