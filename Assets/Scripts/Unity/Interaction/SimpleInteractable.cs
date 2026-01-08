using UnityEngine;
using UnityEngine.Events;

namespace Project.Unity.Interaction
{
    public class SimpleInteractable : InteractableBase
    {
        [Header("Простое взаимодействие")]
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private bool _singleUse = false;
        
        private bool _used;
        
        protected override bool CanInteractInternal()
        {
            return !_singleUse || !_used;
        }
        
        protected override void OnInteractInternal()
        {
            _onInteract?.Invoke();
            
            if (_singleUse)
            {
                _used = true;
            }
        }
        
        public void ResetUsage()
        {
            _used = false;
        }
    }
}


