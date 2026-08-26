using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Movimiento en tercera persona con CharacterController.
    /// WASD se interpreta relativo a la cámara (hacia donde miras).
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

        public void Initialize(Transform followCamera)
        {
            cameraTransform = followCamera;
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (Time.timeScale <= 0.01f) return;

            // Input clásico de Unity: -1..1 en ejes Horizontal/Vertical (A/D y W/S).
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(inputX, 0f, inputZ);
            input = Vector3.ClampMagnitude(input, 1f);

            Vector3 move = Vector3.zero;
            if (cameraTransform != null && input.sqrMagnitude > 0.001f)
            {
                // Dirección relativa a la cámara (solo plano XZ).
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                move = (camForward * input.z + camRight * input.x).normalized;

                Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Gravedad simple para que el CharacterController se pegue al suelo.
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = move * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
