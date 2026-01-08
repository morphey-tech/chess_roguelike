using Project.Core.Interaction;

namespace Project.Core.Messages
{
    public readonly struct InteractionTargetChangedMessage
    {
        public IInteractable Previous { get; }
        public IInteractable Current { get; }
        
        public InteractionTargetChangedMessage(IInteractable previous, IInteractable current)
        {
            Previous = previous;
            Current = current;
        }
    }
    
    public readonly struct InteractionPerformedMessage
    {
        public IInteractable Target { get; }
        
        public InteractionPerformedMessage(IInteractable target)
        {
            Target = target;
        }
    }
}


