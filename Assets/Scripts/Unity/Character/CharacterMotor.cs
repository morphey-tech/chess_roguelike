using Project.Core.Character;
using Project.Core.Ladder;
using UnityEngine;

namespace Project.Unity.Character
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public abstract class CharacterMotor : MonoBehaviour
    {
        [Header("Настройки (если не через DI)")]
        [SerializeField] private CharacterMovementSettings _defaultSettings;

        protected Rigidbody Rb;
        protected CapsuleCollider Capsule;
        protected CharacterMovementSettings Settings;

        protected CharacterState CurrentState = CharacterState.Normal;
        protected Vector3 Velocity;
        protected bool IsGrounded;

        protected float CurrentHeight;
        protected float TargetHeight;

        protected ILadder CurrentLadder;
        protected float LadderProgress;

        private bool _wasGrounded;
        private float _lastFallSpeed;

        public CharacterState State => CurrentState;
        public bool Grounded => IsGrounded;

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Capsule = GetComponent<CapsuleCollider>();
            SetupRigidbody();
        }

        protected virtual void Start()
        {
            if (Settings == null)
            {
                Settings = _defaultSettings ?? new CharacterMovementSettings();
                Debug.LogWarning($"[{GetType().Name}] Using default/serialized settings");
            }

            CurrentHeight = Settings.StandingHeight;
            TargetHeight = Settings.StandingHeight;

            Capsule.height = CurrentHeight;
            Capsule.center = Vector3.up * (CurrentHeight / 2f);
        }

        private void SetupRigidbody()
        {
            Rb.useGravity = false;
            Rb.freezeRotation = true;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;
            Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        protected virtual void FixedUpdate()
        {
            if (Settings == null) return;

            CheckGround();

            switch (CurrentState)
            {
                case CharacterState.Normal:
                case CharacterState.Crouching:
                    ProcessGroundMovement();
                    break;
                case CharacterState.InAir:
                    ProcessAirMovement();
                    break;
                case CharacterState.OnLadder:
                    ProcessLadderMovement();
                    break;
            }

            ApplyVelocity();
            CheckLanding();
        }

        protected virtual void Update()
        {
            if (Settings == null) return;
            UpdateCrouch();
        }

        private void CheckGround()
        {
            if (CurrentState == CharacterState.OnLadder) return;

            Vector3 origin = transform.position + Vector3.up * Settings.GroundCheckRadius;
            IsGrounded = Physics.SphereCast(
                origin,
                Settings.GroundCheckRadius,
                Vector3.down,
                out _,
                Settings.GroundCheckDistance + Settings.GroundCheckRadius,
                Settings.GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (CurrentState == CharacterState.InAir && IsGrounded)
            {
                SetState(GetTargetCrouchState() ? CharacterState.Crouching : CharacterState.Normal);
            }
            else if (CurrentState != CharacterState.InAir && !IsGrounded && CurrentState != CharacterState.OnLadder)
            {
                SetState(CharacterState.InAir);
            }
        }

        private void CheckLanding()
        {
            if (!_wasGrounded && IsGrounded && _lastFallSpeed > 0.5f)
            {
                OnLanded(_lastFallSpeed);
            }

            _wasGrounded = IsGrounded;
            if (!IsGrounded && Velocity.y < 0)
            {
                _lastFallSpeed = Mathf.Abs(Velocity.y);
            }
        }

        protected virtual void OnLanded(float fallSpeed) { }

        protected abstract void ProcessGroundMovement();
        protected abstract void ProcessAirMovement();
        protected abstract void ProcessLadderMovement();
        protected abstract bool GetTargetCrouchState();
        protected abstract void StopMovement();

        protected void ApplyGroundMovement(Vector2 input, bool isSprinting)
        {
            Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
            float targetSpeed = GetTargetSpeed(isSprinting);
            Vector3 targetVelocity = moveDir * targetSpeed;

            float acceleration = input.sqrMagnitude > 0.01f
                ? Settings.GroundAcceleration
                : Settings.GroundDeceleration;

            Velocity.x = Mathf.MoveTowards(Velocity.x, targetVelocity.x, acceleration * Time.fixedDeltaTime);
            Velocity.z = Mathf.MoveTowards(Velocity.z, targetVelocity.z, acceleration * Time.fixedDeltaTime);
            Velocity.y = 0f;
        }

        protected void ApplyAirMovement(Vector2 input, bool isSprinting)
        {
            Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
            Vector3 horizontalVelocity = new Vector3(Velocity.x, 0, Velocity.z);
            Vector3 targetVelocity = moveDir * GetTargetSpeed(isSprinting);

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                Settings.AirControl * Settings.GroundAcceleration * Time.fixedDeltaTime
            );

            Velocity.x = horizontalVelocity.x;
            Velocity.z = horizontalVelocity.z;
            Velocity.y += Settings.Gravity * Time.fixedDeltaTime;
            Velocity.y = Mathf.Max(Velocity.y, -Settings.MaxFallSpeed);
        }

        protected float GetTargetSpeed(bool isSprinting)
        {
            return CurrentState switch
            {
                CharacterState.Crouching => Settings.CrouchSpeed,
                _ when isSprinting => Settings.SprintSpeed,
                _ => Settings.WalkSpeed
            };
        }

        private void ApplyVelocity()
        {
            Rb.linearVelocity = Velocity;
        }

        protected bool TryJump()
        {
            if (CurrentState == CharacterState.OnLadder) return false;
            if (!IsGrounded) return false;
            if (CurrentState == CharacterState.Crouching && !Settings.CanJumpWhileCrouching) return false;

            Velocity.y = Settings.JumpForce;
            IsGrounded = false;
            SetState(CharacterState.InAir);
            return true;
        }

        protected void SetCrouchTarget(bool crouch)
        {
            if (CurrentState == CharacterState.OnLadder) return;

            if (crouch)
            {
                TargetHeight = Settings.CrouchingHeight;
                if (IsGrounded)
                {
                    SetState(CharacterState.Crouching);
                }
            }
            else if (CanStandUp())
            {
                TargetHeight = Settings.StandingHeight;
                if (IsGrounded && CurrentState == CharacterState.Crouching)
                {
                    SetState(CharacterState.Normal);
                }
            }
        }

        private void UpdateCrouch()
        {
            if (Mathf.Approximately(CurrentHeight, TargetHeight)) return;

            CurrentHeight = Mathf.MoveTowards(
                CurrentHeight,
                TargetHeight,
                Settings.CrouchTransitionSpeed * Time.deltaTime
            );

            Capsule.height = CurrentHeight;
            Capsule.center = Vector3.up * (CurrentHeight / 2f);
        }

        private bool CanStandUp()
        {
            Vector3 origin = transform.position + Vector3.up * Settings.CrouchingHeight;
            float distance = Settings.StandingHeight - Settings.CrouchingHeight;

            return !Physics.SphereCast(
                origin,
                Settings.GroundCheckRadius * 0.9f,
                Vector3.up,
                out _,
                distance,
                Settings.GroundLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        public void EnterLadder(ILadder ladder)
        {
            if (CurrentState == CharacterState.OnLadder) return;

            CurrentLadder = ladder;
            CurrentLadder.GetClosestPoint(transform.position, out LadderProgress);

            SetState(CharacterState.OnLadder);
            OnLadderEntered();

            Vector3 lookDir = CurrentLadder.Forward;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(-lookDir);
        }

        public void ExitLadder()
        {
            if (CurrentState != CharacterState.OnLadder) return;

            CurrentLadder = null;
            Velocity = Vector3.zero;

            SetState(IsGrounded ? CharacterState.Normal : CharacterState.InAir);
            OnLadderExited();
        }

        protected void ProcessLadderClimb(float direction)
        {
            if (CurrentLadder == null)
            {
                ExitLadder();
                return;
            }

            float ladderLength = Vector3.Distance(CurrentLadder.BottomPoint, CurrentLadder.TopPoint);
            LadderProgress += direction * Settings.LadderClimbSpeed * Time.fixedDeltaTime / ladderLength;
            LadderProgress = Mathf.Clamp01(LadderProgress);

            if (LadderProgress >= 1f && CurrentLadder.CanExitTop(out Vector3 exitPos))
            {
                transform.position = exitPos;
                ExitLadder();
                return;
            }

            if (LadderProgress <= 0f && CurrentLadder.CanExitBottom(out exitPos))
            {
                transform.position = exitPos;
                ExitLadder();
                return;
            }

            Vector3 targetPos = CurrentLadder.GetPositionOnLadder(LadderProgress);
            targetPos -= CurrentLadder.Forward * Settings.LadderGrabDistance;
            Velocity = (targetPos - transform.position) / Time.fixedDeltaTime;
        }

        protected void JumpFromLadder()
        {
            if (CurrentLadder == null) return;

            Vector3 jumpDir = -CurrentLadder.Forward + Vector3.up;
            Velocity = jumpDir.normalized * Settings.JumpForce * 0.7f;
            ExitLadder();
            SetState(CharacterState.InAir);
        }

        protected virtual void OnLadderEntered() { }
        protected virtual void OnLadderExited() { }

        protected void SetState(CharacterState newState)
        {
            if (CurrentState == newState) return;

            CharacterState previousState = CurrentState;
            CurrentState = newState;
            OnStateChanged(previousState, newState);
        }

        protected virtual void OnStateChanged(CharacterState previousState, CharacterState newState) { }

        protected virtual void OnDrawGizmosSelected()
        {
            if (Settings == null) return;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 origin = transform.position + Vector3.up * Settings.GroundCheckRadius;
            Gizmos.DrawWireSphere(origin, Settings.GroundCheckRadius);
            Gizmos.DrawWireSphere(origin + Vector3.down * Settings.GroundCheckDistance, Settings.GroundCheckRadius);
        }
    }
}
