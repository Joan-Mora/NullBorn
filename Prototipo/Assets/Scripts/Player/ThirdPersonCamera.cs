using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Cámara orbital. Distancia se acorta en lock-on; shake vía CombatFeedback.
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
        private float shake;
        private Vector3 shakeOffset;
        private LockOnController lockOn;
        private float currentDistance;

        public float DefaultDistance
        {
            get => distance;
            set => distance = value;
        }

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            if (target != null)
            {
                yaw = target.eulerAngles.y;
                lockOn = target.GetComponent<LockOnController>();
            }

            currentDistance = distance;
        }

        public void AddShake(float amount)
        {
            shake = Mathf.Max(shake, amount);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Transform lockTarget = lockOn != null ? lockOn.CurrentTarget : null;
            float desiredDistance = lockTarget != null ? 4.5f : distance;
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, 6f * Time.deltaTime);

            if (lockTarget != null)
            {
                Vector3 toLock = lockTarget.position - target.position;
                if (toLock.sqrMagnitude > 0.01f)
                {
                    float targetYaw = Mathf.Atan2(toLock.x, toLock.z) * Mathf.Rad2Deg;
                    yaw = Mathf.LerpAngle(yaw, targetYaw, 3.5f * Time.deltaTime);
                }
            }

            shake = Mathf.MoveTowards(shake, 0f, Time.unscaledDeltaTime * 2.4f);
            shakeOffset = shake > 0.001f
                ? new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * shake
                : Vector3.zero;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + targetOffset + (lockTarget != null ? target.transform.right * 0.35f : Vector3.zero);
            Vector3 desiredPosition = focusPoint - rotation * Vector3.forward * currentDistance;

            transform.position = desiredPosition + transform.TransformDirection(shakeOffset);
            transform.rotation = rotation;
        }
    }
}
