using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Movimiento 3ª persona: WASD relativo a cámara, sprint (Shift), dash con i-frames (Espacio).
    /// Buffer de input ~180 ms para dash y ataques (consumido por TraceCutCombo).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private Transform cameraTransform;
        private float verticalVelocity;
        private Vector3 planarVelocity;
        private Vector3 impulse;
        private CombatFeelConfig feel;
        private LockOnController lockOn;
        private CpuCycles cpu;

        private float dashUntil;
        private float invulnUntil;
        private float dashReadyAt;
        private float bufferedDashUntil;
        private float bufferedAttackUntil;
        private Vector3 dashDir;
        private bool sprinting;

        public bool IsInvulnerable => Time.unscaledTime < invulnUntil;
        public bool IsDashing => Time.time < dashUntil;
        public bool IsSprinting => sprinting;
        public bool MovementLocked { get; set; }
        public float PlanarSpeed => planarVelocity.magnitude;
        public Vector3 PlanarVelocity => planarVelocity;
        public bool IsGrounded => controller != null && controller.isGrounded;
        public float InputBufferWindow => feel != null ? feel.inputBuffer : 0.18f;

        public void Initialize(Transform followCamera)
        {
            cameraTransform = followCamera;
            controller = GetComponent<CharacterController>();
            lockOn = GetComponent<LockOnController>();
            cpu = GetComponent<CpuCycles>();
            if (feel == null) feel = CombatFeelConfig.CreateRuntimeDefault();
        }

        public void SetFeel(CombatFeelConfig config)
        {
            if (config != null) feel = config;
        }

        public void BufferAttack()
        {
            bufferedAttackUntil = Time.unscaledTime + InputBufferWindow;
        }

        public bool ConsumeBufferedAttack()
        {
            if (Time.unscaledTime > bufferedAttackUntil) return false;
            bufferedAttackUntil = 0f;
            return true;
        }

        public void AddImpulse(Vector3 worldImpulse)
        {
            impulse += worldImpulse;
        }

        private void Update()
        {
            if (Time.timeScale <= 0.01f) return;
            if (controller == null) controller = GetComponent<CharacterController>();
            if (cpu == null) cpu = GetComponent<CpuCycles>();
            if (lockOn == null) lockOn = GetComponent<LockOnController>();
            if (feel == null) feel = CombatFeelConfig.CreateRuntimeDefault();

            ReadBuffers();
            TryStartDash();

            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(inputX, 0f, inputZ), 1f);

            Vector3 move = Vector3.zero;
            if (cameraTransform != null && input.sqrMagnitude > 0.001f)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
                move = (camForward * input.z + camRight * input.x).normalized;
            }

            bool wantSprint = Input.GetKey(KeyCode.LeftShift) && move.sqrMagnitude > 0.01f && !IsDashing && !MovementLocked;
            sprinting = wantSprint && cpu != null && cpu.CanAfford(feel.sprintCpuPerSecond * Time.deltaTime);
            if (sprinting)
            {
                cpu.TrySpend(feel.sprintCpuPerSecond * Time.deltaTime);
            }

            if (IsDashing)
            {
                move = dashDir;
            }
            else if (MovementLocked)
            {
                move = Vector3.zero;
                sprinting = false;
            }

            Face(move);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            else verticalVelocity += gravity * Time.deltaTime;

            float speed = moveSpeed * (sprinting ? feel.sprintMultiplier : 1f);
            if (IsDashing)
            {
                speed = feel.dashDistance / Mathf.Max(0.08f, feel.dashDuration);
            }

            planarVelocity = move * speed;
            Vector3 velocity = planarVelocity;
            velocity.y = verticalVelocity;
            velocity += impulse;
            impulse = Vector3.Lerp(impulse, Vector3.zero, 8f * Time.deltaTime);
            controller.Move(velocity * Time.deltaTime);
        }

        private void ReadBuffers()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                bufferedDashUntil = Time.unscaledTime + InputBufferWindow;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
            {
                BufferAttack();
            }
        }

        private void TryStartDash()
        {
            if (IsDashing) return;
            if (Time.unscaledTime > bufferedDashUntil && !Input.GetKeyDown(KeyCode.Space)) return;
            if (Time.time < dashReadyAt) return;
            if (cpu != null && !cpu.TrySpend(feel.dashCpuCost))
            {
                CombatFeedback.NotifyDeny(transform.position);
                bufferedDashUntil = 0f;
                return;
            }

            bufferedDashUntil = 0f;
            dashDir = planarVelocity.sqrMagnitude > 0.05f ? planarVelocity.normalized : transform.forward;
            dashDir.y = 0f;
            if (dashDir.sqrMagnitude < 0.01f) dashDir = transform.forward;
            dashDir.Normalize();

            dashUntil = Time.time + feel.dashDuration;
            invulnUntil = Time.unscaledTime + feel.dashIFrameDuration;
            dashReadyAt = Time.time + feel.dashCooldown;

            PlayerAnimBridge anim = GetComponent<PlayerAnimBridge>();
            anim?.PlayDash();
            CombatFeedback.PlayDodgeSfx(transform.position);
        }

        private void Face(Vector3 move)
        {
            Transform lockTarget = lockOn != null ? lockOn.CurrentTarget : null;
            Vector3 look = Vector3.zero;
            if (lockTarget != null)
            {
                look = lockTarget.position - transform.position;
                look.y = 0f;
            }
            else if (move.sqrMagnitude > 0.001f)
            {
                look = move;
            }

            if (look.sqrMagnitude < 0.001f) return;
            Quaternion targetRot = Quaternion.LookRotation(look.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
}
