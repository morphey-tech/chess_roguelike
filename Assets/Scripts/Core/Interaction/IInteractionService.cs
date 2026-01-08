using System;

namespace Project.Core.Interaction
{
    public interface IInteractionService
    {
        IInteractable CurrentTarget { get; }
        IObservable<IInteractable> OnTargetChanged { get; }
        IObservable<IInteractable> OnInteracted { get; }
        
        void TryInteract();
        void SetEnabled(bool enabled);
    }
}


