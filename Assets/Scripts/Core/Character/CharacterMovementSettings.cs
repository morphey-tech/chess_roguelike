using System;
using UnityEngine;

namespace Project.Core.Character
{
    [Serializable]
    public class CharacterMovementSettings
    {
        [Header("Скорость")]
        public float WalkSpeed = 4f;
        public float SprintSpeed = 7f;
        public float CrouchSpeed = 2f;
        public float LadderClimbSpeed = 3f;
        
        [Header("Прыжок")]
        public float JumpForce = 5f;
        public bool CanJumpWhileCrouching = false;
        
        [Header("Физика")]
        public float GroundAcceleration = 10f;
        public float GroundDeceleration = 10f;
        [Range(0f, 1f)]
        public float AirControl = 0.3f;
        public float Gravity = -20f;
        public float MaxFallSpeed = 20f;
        
        [Header("Проверка земли")]
        public float GroundCheckDistance = 0.1f;
        public float GroundCheckRadius = 0.3f;
        public LayerMask GroundLayers = ~0;
        
        [Header("Приседание")]
        public float StandingHeight = 2f;
        public float CrouchingHeight = 1f;
        public float CrouchTransitionSpeed = 8f;
        
        [Header("Лестницы")]
        public float LadderGrabDistance = 0.5f;
    }
}


