using UnityEngine;
using UnityEngine.InputSystem;

namespace NullBorn.Game
{
    public enum LocomotionPose
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Backpedal = 3
    }

    /// <summary>
    /// Clean third-person locomotion (rebuilt).
    /// WithMouse: mouse aims camera; A/D yaw body; W/S move along body forward.
    /// KeyboardOnly: A/D yaw camera; body follows camera; W/S along camera.
    /// Movement uses acceleration; animation pose is discrete (no BlendTree mixing).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimpleThirdPersonMotor : MonoBehaviour
    {
        [Header("Speeds")]
        [SerializeField] float walkSpeed = 3.8f;
        [SerializeField] float sprintSpeed = 6.5f;
        [SerializeField] float backpedalSpeed = 2.5f;
        [SerializeField] float rollSpeed = 7f;
        [SerializeField] float rollDuration = 0.85f;
        [SerializeField] float rollCooldown = 0.4f;
        [SerializeField] float gravity = -25f;

        [Header("Feel")]
        [SerializeField] float moveSmoothTime = 0.12f;
        [SerializeField] float turnDegreesPerSecond = 120f;
        [SerializeField] float turnCommitSeconds = 0.06f;

        [SerializeField] Transform cameraTransform;
        [SerializeField] SimpleFollowCamera followCamera;

        CharacterController controller;
        float verticalVelocity;
        float bodyYaw;
        bool bodyYawReady;
        float holdA;
        float holdD;
        float inputForward;
        Vector3 velocityPlanar;
        Vector3 velocitySmoothRef;
        Vector3 rollDirection;
        float rollTimer;
        float rollCooldownTimer;
        bool rollRequested;
        bool sprintHeld;

        public LocomotionPose Pose { get; private set; }
        public float LocomotionBlend => Pose switch
        {
            LocomotionPose.Walk => 0.5f,
            LocomotionPose.Run => 1f,
            LocomotionPose.Backpedal => -0.5f,
            _ => 0f
        };
        public float ForwardAmount { get; private set; }
        public float StrafeAmount { get; private set; }
        public float TurnAmount { get; private set; }
        public bool IsMoving => velocityPlanar.sqrMagnitude > 0.04f;
        public bool IsTurningInPlace { get; private set; }
        public bool IsSprinting => Pose == LocomotionPose.Run;
        public bool IsRolling { get; private set; }
        public bool RollRequested => rollRequested;
        public float TurnIntensity => 0f;
        public float RollDuration => rollDuration;
        public float BodyYaw => bodyYaw;

        public void ConsumeRollRequest() => rollRequested = false;

        public void SetRollDuration(float seconds)
        {
            if (seconds > 0.05f)
                rollDuration = Mathf.Clamp(seconds, 0.35f, 1.4f);
        }

        public void NotifyRollAnimationEnded()
        {
            if (!IsRolling) return;
            EndRoll();
        }

        void Awake() => controller = GetComponent<CharacterController>();

        public void SetCamera(Transform cam)
        {
            cameraTransform = cam;
            if (cam != null)
                followCamera = cam.GetComponent<SimpleFollowCamera>();
        }

        public void SetFacingTransform(Transform _) { }

        float AimYaw
        {
            get
            {
                if (followCamera != null) return followCamera.LookYaw;
                if (cameraTransform != null) return cameraTransform.eulerAngles.y;
                return transform.eulerAngles.y;
            }
        }

        void EnsureYaw()
        {
            if (bodyYawReady) return;
            bodyYaw = transform.eulerAngles.y;
            bodyYawReady = true;
        }

        void SetYaw(float yaw)
        {
            bodyYaw = yaw;
            transform.rotation = Quaternion.Euler(0f, bodyYaw, 0f);
        }

        void Update()
        {
            if (PauseMenuController.IsPaused) return;

            if (rollCooldownTimer > 0f)
                rollCooldownTimer -= Time.deltaTime;

            TurnAmount = 0f;
            IsTurningInPlace = false;

            if (IsRolling)
            {
                TickRoll();
                ApplyGravityOnly();
                return;
            }

            ReadInputAndSteer();
            TryRoll();
            if (IsRolling) return;

            IntegrateMove();
            UpdatePose();
        }

        void ReadInputAndSteer()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                holdA = holdD = 0f;
                inputForward = 0f;
                sprintHeld = false;
                return;
            }

            EnsureYaw();
            var dt = Time.unscaledDeltaTime;
            holdA = kb.aKey.isPressed ? holdA + dt : 0f;
            holdD = kb.dKey.isPressed ? holdD + dt : 0f;

            // Forward axis: immediate and continuous (fluid start/stop via velocity smooth).
            inputForward = Axis(kb.sKey.isPressed, kb.wKey.isPressed);
            sprintHeld = (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) && inputForward > 0.01f;

            // Turn: commit only after short hold; longer-held side wins.
            var turn = CommittedAxis(holdA, holdD, turnCommitSeconds);

            if (GameSettings.UsesMouse)
            {
                if (Mathf.Abs(turn) > 0.01f)
                {
                    SetYaw(bodyYaw + turn * turnDegreesPerSecond * Time.deltaTime);
                    TurnAmount = turn * 0.35f;
                    IsTurningInPlace = Mathf.Abs(inputForward) < 0.01f;
                }
            }
            else
            {
                if (Mathf.Abs(turn) > 0.01f && followCamera != null)
                {
                    followCamera.AddLookYaw(turn * turnDegreesPerSecond * Time.deltaTime);
                    TurnAmount = turn * 0.35f;
                    IsTurningInPlace = Mathf.Abs(inputForward) < 0.01f;
                }

                SetYaw(AimYaw);
            }
        }

        void IntegrateMove()
        {
            float yaw = GameSettings.UsesMouse ? bodyYaw : AimYaw;
            Vector3 desired;
            if (Mathf.Abs(inputForward) < 0.01f)
            {
                desired = Vector3.zero;
            }
            else
            {
                var speed = inputForward < 0f ? backpedalSpeed : (sprintHeld ? sprintSpeed : walkSpeed);
                desired = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, inputForward);
                desired.y = 0f;
                if (desired.sqrMagnitude > 0.001f)
                    desired = desired.normalized * speed;
                else
                    desired = Vector3.zero;
            }

            velocityPlanar = Vector3.SmoothDamp(
                velocityPlanar,
                desired,
                ref velocitySmoothRef,
                moveSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            if (velocityPlanar.sqrMagnitude < 0.0001f)
            {
                velocityPlanar = Vector3.zero;
                velocitySmoothRef = Vector3.zero;
            }

            ApplyGravityOnly();
            var motion = velocityPlanar;
            motion.y = verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            if (velocityPlanar.sqrMagnitude > 0.01f)
            {
                var local = Quaternion.Inverse(transform.rotation) * velocityPlanar.normalized;
                ForwardAmount = Mathf.Clamp(local.z, -1f, 1f);
                StrafeAmount = Mathf.Clamp(local.x, -1f, 1f);
            }
            else
            {
                ForwardAmount = 0f;
                StrafeAmount = 0f;
            }
        }

        void UpdatePose()
        {
            if (Mathf.Abs(inputForward) < 0.01f && velocityPlanar.sqrMagnitude < 0.15f)
            {
                Pose = LocomotionPose.Idle;
                return;
            }

            if (inputForward < -0.01f)
                Pose = LocomotionPose.Backpedal;
            else if (sprintHeld)
                Pose = LocomotionPose.Run;
            else
                Pose = LocomotionPose.Walk;
        }

        void TryRoll()
        {
            var kb = Keyboard.current;
            if (kb == null || rollCooldownTimer > 0f) return;
            if (!kb.leftCtrlKey.wasPressedThisFrame && !kb.cKey.wasPressedThisFrame) return;

            IsRolling = true;
            rollTimer = rollDuration;
            rollRequested = true;
            Pose = LocomotionPose.Idle;

            if (velocityPlanar.sqrMagnitude > 0.05f)
                rollDirection = velocityPlanar.normalized;
            else
            {
                EnsureYaw();
                rollDirection = Quaternion.Euler(0f, bodyYaw, 0f) * Vector3.forward;
                rollDirection.y = 0f;
                if (rollDirection.sqrMagnitude < 0.001f)
                    rollDirection = Vector3.forward;
                else
                    rollDirection.Normalize();
            }

            SetYaw(Quaternion.LookRotation(rollDirection, Vector3.up).eulerAngles.y);
            velocityPlanar = Vector3.zero;
            velocitySmoothRef = Vector3.zero;
        }

        void TickRoll()
        {
            rollTimer -= Time.deltaTime;
            var t = 1f - Mathf.Clamp01(rollTimer / Mathf.Max(0.01f, rollDuration));
            var scale = Mathf.Lerp(1f, 0.2f, t * t);
            ApplyGravityOnly();
            var v = rollDirection * (rollSpeed * scale);
            v.y = verticalVelocity;
            controller.Move(v * Time.deltaTime);
            if (rollTimer <= 0f)
                EndRoll();
        }

        void EndRoll()
        {
            IsRolling = false;
            rollCooldownTimer = rollCooldown;
            rollTimer = 0f;
            rollRequested = false;
            velocityPlanar = Vector3.zero;
            velocitySmoothRef = Vector3.zero;
            inputForward = 0f;
            Pose = LocomotionPose.Idle;
        }

        void ApplyGravityOnly()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += gravity * Time.deltaTime;
        }

        static float Axis(bool negative, bool positive)
        {
            if (negative == positive) return 0f;
            return positive ? 1f : -1f;
        }

        static float CommittedAxis(float holdNeg, float holdPos, float minHold)
        {
            var n = holdNeg >= minHold;
            var p = holdPos >= minHold;
            if (!n && !p) return 0f;
            if (n && !p) return -1f;
            if (p && !n) return 1f;
            if (holdPos > holdNeg) return 1f;
            if (holdNeg > holdPos) return -1f;
            return 0f;
        }
    }
}
