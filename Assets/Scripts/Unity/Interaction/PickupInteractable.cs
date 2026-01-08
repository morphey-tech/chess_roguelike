using Project.Core.Logging;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Interaction
{
    public class PickupInteractable : InteractableBase
    {
        [Header("Предмет")]
        [SerializeField] private string _itemId;
        [SerializeField] private int _amount = 1;
        [SerializeField] private bool _destroyOnPickup = true;
        
        private ILogger _logger;
        
        [Inject]
        public void Construct(ILogService logService)
        {
            _logger = logService.CreateLogger<PickupInteractable>();
        }
        
        protected override void OnInteractInternal()
        {
            _logger?.Info($"Picked up: {_itemId} x{_amount}");
            
            if (_destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                SetCanInteract(false);
            }
        }
    }
}


