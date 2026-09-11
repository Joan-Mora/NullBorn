using UnityEngine;
using UnityEngine.InputSystem;

namespace NullBorn.Game
{
    /// <summary>
    /// Soft locked OTS camera. Mouse (WithMouse) or A/D (KeyboardOnly).
    /// </summary>
    public class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Transform facingTransform;

        [Header("Framing (shoulder)")]
        [SerializeField] float distance = 2.9f;
        [SerializeField] float height = 1.6f;
        [SerializeField] float shoulderOffset = 0.45f;
        [SerializeField] float lookHeight = 1.4f;
        [SerializeField] float lookAhead = 1.8f;
        [SerializeField] float minPitch = -8f;
        [SerializeField] float maxPitch = 28f;

        [Header("Mouse aim")]
        [SerializeField] float degreesPerPixel = 0.05f;
        [SerializeField] float maxPixelDeltaPerFrame = 16f;
        [SerializeField] float maxDegreesPerSecondAtSens1 = 110f;
        [SerializeField] float absoluteMaxDegreesPerSecond = 140f;
        [SerializeField] float positionSmoothTime = 0.1f;
        [SerializeField] float facingAlignDegreesPerSecond = 400f;
        [SerializeField] bool lockCursorOnPlay = true;

        float yaw;
        float pitch = 12f;
        Vector3 positionVelocity;
        bool initialized;
        bool cursorLocked;
        Quaternion currentLook = Quaternion.identity;

        public float LookYaw => yaw;
        public bool IsCursorLocked => cursorLocked;

        void Awake()
        {
            if (distance > 4f)
                ApplyOverTheShoulderDefaults();
            SanitizeFacing();
        }

        void OnEnable()
        {
            if (lockCursorOnPlay && Application.isPlaying && !PauseMenuController.IsPaused && GameSettings.UsesMouse)
                SetCursorLocked(true);
        }

        void OnDisable()
        {
            if (cursorLocked)
                SetCursorLocked(false);
        }

        public void SetTarget(Transform t)
        {
            target = t;
            SanitizeFacing();
        }

        public void SetFacingTransform(Transform t)
        {
            facingTransform = t;
            SanitizeFacing();
        }

        void SanitizeFacing()
        {
            if (facingTransform != null && facingTransform.GetComponentInChildren<Animator>() != null
                && facingTransform.GetComponent<CharacterController>() == null)
            {
                var root = facingTransform.root;
                var cc = root.GetComponentInChildren<CharacterController>();
                if (cc != null)
                    facingTransform = cc.transform;
            }
        }

        public void SetCursorLocked(bool locked)
        {
            cursorLocked = locked && GameSettings.UsesMouse;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !cursorLocked;
        }

        public void ApplyOverTheShoulderDefaults()
        {
            distance = 2.9f;
            height = 1.6f;
            shoulderOffset = 0.45f;
            lookHeight = 1.4f;
            lookAhead = 1.8f;
            minPitch = -8f;
            maxPitch = 28f;
            degreesPerPixel = 0.05f;
            maxPixelDeltaPerFrame = 16f;
            maxDegreesPerSecondAtSens1 = 110f;
            absoluteMaxDegreesPerSecond = 140f;
            positionSmoothTime = 0.1f;
            facingAlignDegreesPerSecond = 400f;
            pitch = 12f;
        }

        public void AddLookYaw(float degrees)
        {
            if (PauseMenuController.IsPaused) return;
            var sens = GameSettings.UsesMouse ? 1f : GameSettings.MouseSensitivity;
            var maxStep = Mathf.Min(absoluteMaxDegreesPerSecond, maxDegreesPerSecondAtSens1 * Mathf.Max(0.35f, sens))
                          * Time.unscaledDeltaTime;
            yaw += Mathf.Clamp(degrees, -maxStep, maxStep);
        }

        void LateUpdate()
        {
            if (target == null) return;
            if (PauseMenuController.IsPaused) return;

            var usesMouse = GameSettings.UsesMouse;
            if (usesMouse)
            {
                if (!cursorLocked)
                {
                    var mouse = Mouse.current;
                    if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                        SetCursorLocked(true);
                }

                ReadMouseAim();
            }

            if (!initialized)
            {
                var seed = facingTransform != null ? facingTransform : target;
                yaw = seed.eulerAngles.y;
                initialized = true;
                positionVelocity = Vector3.zero;
                if (!usesMouse)
                    SnapFacingToYaw();
                transform.position = ComputePivot();
                currentLook = ComputeLookRotation();
                transform.rotation = currentLook;
                return;
            }

            if (!usesMouse)
                AlignFacingToYaw();

            transform.position = Vector3.SmoothDamp(
                transform.position,
                ComputePivot(),
                ref positionVelocity,
                positionSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            var desiredLook = ComputeLookRotation();
            currentLook = Quaternion.Slerp(currentLook, desiredLook, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            transform.rotation = currentLook;
        }

        void ReadMouseAim()
        {
            if (!cursorLocked) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            var delta = mouse.delta.ReadValue();
            if (delta.sqrMagnitude < 0.0001f) return;

            var dx = Mathf.Clamp(delta.x, -maxPixelDeltaPerFrame, maxPixelDeltaPerFrame);
            var dy = Mathf.Clamp(delta.y, -maxPixelDeltaPerFrame, maxPixelDeltaPerFrame);

            var sens = GameSettings.MouseSensitivity;
            var maxStep = Mathf.Min(absoluteMaxDegreesPerSecond, maxDegreesPerSecondAtSens1 * sens)
                          * Time.unscaledDeltaTime;

            yaw += Mathf.Clamp(dx * degreesPerPixel * sens, -maxStep, maxStep);
            pitch -= Mathf.Clamp(dy * degreesPerPixel * sens, -maxStep, maxStep);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        void AlignFacingToYaw()
        {
            if (facingTransform == null) return;
            var targetRot = Quaternion.Euler(0f, yaw, 0f);
            facingTransform.rotation = Quaternion.RotateTowards(
                facingTransform.rotation,
                targetRot,
                facingAlignDegreesPerSecond * Time.unscaledDeltaTime);
        }

        void SnapFacingToYaw()
        {
            if (facingTransform == null) return;
            facingTransform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        Vector3 ComputePivot()
        {
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            return target.position + Vector3.up * height + rot * new Vector3(shoulderOffset, 0f, -distance);
        }

        Quaternion ComputeLookRotation()
        {
            var ahead = Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward;
            var lookPoint = target.position + Vector3.up * lookHeight + ahead * lookAhead;
            var to = lookPoint - transform.position;
            if (to.sqrMagnitude < 0.0001f)
                return transform.rotation;
            return Quaternion.LookRotation(to, Vector3.up);
        }
    }
}
