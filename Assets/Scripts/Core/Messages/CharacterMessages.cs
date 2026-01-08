using Project.Core.Character;

namespace Project.Core.Messages
{
    public readonly struct CharacterStateChangedMessage
    {
        public CharacterState PreviousState { get; }
        public CharacterState NewState { get; }
        
        public CharacterStateChangedMessage(CharacterState previousState, CharacterState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
    
    public readonly struct CharacterLandedMessage
    {
        public float FallSpeed { get; }
        
        public CharacterLandedMessage(float fallSpeed)
        {
            FallSpeed = fallSpeed;
        }
    }
    
    public readonly struct LadderInteractionMessage
    {
        public bool IsOnLadder { get; }
        
        public LadderInteractionMessage(bool isOnLadder)
        {
            IsOnLadder = isOnLadder;
        }
    }
}


