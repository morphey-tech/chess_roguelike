using UnityEngine;

namespace Project.Core.Interaction
{
    public interface IInteractable
    {
        string InteractionId { get; }
        bool CanInteract { get; }
        InteractionDisplayData DisplayData { get; }
        
        void OnFocused();
        void OnUnfocused();
        void Interact();
    }
    
    [System.Serializable]
    public class InteractionDisplayData
    {
        public string PromptText = "Interact";
        public string ActionKey = "E";
        public Sprite Icon;
        public Vector3 UIOffset = Vector3.up;
        public bool ShowInWorldSpace = true;
    }
}


