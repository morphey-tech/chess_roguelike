using System;

namespace Project.Core.Character
{
    public interface IMovementCommandDispatcher
    {
        IObservable<MovementCommand> OnMovement { get; }
        IObservable<JumpCommand> OnJump { get; }
        IObservable<InteractCommand> OnInteract { get; }
        IObservable<LookCommand> OnLook { get; }
        IObservable<bool> OnCrouchChanged { get; }
        IObservable<bool> OnEyeCloseChanged { get; }
    }
}


