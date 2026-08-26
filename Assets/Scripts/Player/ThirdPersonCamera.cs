using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Cámara orbital en tercera persona.
    /// Mouse X/Y orbitan alrededor del jugador.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Seguimiento")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 55f;

        private float yaw;
        private float pitch = 18f;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + targetOffset;
            Vector3 desiredPosition = focusPoint - rotation * Vector3.forward * distance;

            transform.position = desiredPosition;
            transform.rotation = rotation;
        }
    }
}
