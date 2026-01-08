using Project.Core.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Unity.Interaction
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Идентификация")]
        [SerializeField] private string _interactionId;
        
        [Header("Отображение")]
        [SerializeField] private InteractionDisplayData _displayData = new();
        
        [Header("Состояние")]
        [SerializeField] private bool _canInteract = true;
        
        [Header("События")]
        [SerializeField] private UnityEvent _onFocused;
        [SerializeField] private UnityEvent _onUnfocused;
        [SerializeField] private UnityEvent _onInteracted;
        
        public string InteractionId => _interactionId;
        public bool CanInteract => _canInteract && CanInteractInternal();
        public InteractionDisplayData DisplayData => _displayData;
        
        protected virtual bool CanInteractInternal() => true;
        
        public void OnFocused()
        {
            _onFocused?.Invoke();
            OnFocusedInternal();
        }
        
        public void OnUnfocused()
        {
            _onUnfocused?.Invoke();
            OnUnfocusedInternal();
        }
        
        public void Interact()
        {
            if (!CanInteract) return;
            
            _onInteracted?.Invoke();
            OnInteractInternal();
        }
        
        protected virtual void OnFocusedInternal() { }
        protected virtual void OnUnfocusedInternal() { }
        protected abstract void OnInteractInternal();
        
        public void SetCanInteract(bool canInteract)
        {
            _canInteract = canInteract;
        }
        
        protected virtual void Reset()
        {
            _interactionId = gameObject.name;
        }
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_interactionId))
            {
                _interactionId = gameObject.name;
            }
        }
    }
}


