using UnityEngine;

namespace Project.Core.Character
{
    public readonly struct MovementCommand
    {
        public Vector2 Direction { get; }
        public bool IsSprinting { get; }
        public bool IsCrouching { get; }
        
        public MovementCommand(Vector2 direction, bool isSprinting, bool isCrouching)
        {
            Direction = direction;
            IsSprinting = isSprinting;
            IsCrouching = isCrouching;
        }
    }
    
    public readonly struct JumpCommand
    {
        public static JumpCommand Default => new();
    }
    
    public readonly struct InteractCommand
    {
        public static InteractCommand Default => new();
    }
    
    public readonly struct LookCommand
    {
        public Vector2 Delta { get; }
        
        public LookCommand(Vector2 delta)
        {
            Delta = delta;
        }
    }
}


