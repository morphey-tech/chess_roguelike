using Project.Core.Spawn;
using UnityEngine;
using VContainer;

namespace Project.Unity.Spawn
{
    /// <summary>
    /// Точка спавна на сцене.
    /// Автоматически регистрируется в SpawnService при включении.
    /// </summary>
    public class SpawnPoint : MonoBehaviour, ISpawnPoint
    {
        [SerializeField] private string _id = "player_default";
        
        private ISpawnService? _spawnService;
        private bool _isRegistered;
        
        public string Id => _id;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        [Inject]
        public void Construct(ISpawnService spawnService)
        {
            _spawnService = spawnService;
            
            // Если объект уже активен — регистрируемся сразу
            if (isActiveAndEnabled && !_isRegistered)
            {
                Register();
            }
        }
        
        private void OnEnable()
        {
            // Регистрируемся только если уже получили инъекцию
            if (_spawnService != null && !_isRegistered)
            {
                Register();
            }
        }
        
        private void OnDisable()
        {
            Unregister();
        }
        
        private void Register()
        {
            _spawnService?.RegisterSpawnPoint(this);
            _isRegistered = true;
        }
        
        private void Unregister()
        {
            if (_isRegistered)
            {
                _spawnService?.UnregisterSpawnPoint(Id);
                _isRegistered = false;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.7f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
        
        private void Reset()
        {
            _id = gameObject.name;
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = gameObject.name;
            }
        }
    }
}


