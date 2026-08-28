using System.Collections;
using NullBorn.Art;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Puente Mixamo (Speed / Attack / Dash) + fallback procedural si no hay Animator clips.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerAnimBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string attackParam = "Attack";
        [SerializeField] private string dashParam = "Dash";

        private CharacterController controller;
        private Transform visual;
        private Transform armL;
        private Transform armR;
        private SimpleBobAnim bob;
        private Vector3 visualBaseScale;
        private Quaternion armLBase;
        private Quaternion armRBase;
        private Coroutine punchRoutine;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            CacheVisual();
        }

        private void CacheVisual()
        {
            visual = transform.Find("Visual_ThreadAvatar");
            if (visual == null) return;
            visualBaseScale = visual.localScale;
            bob = visual.GetComponent<NullBorn.Art.SimpleBobAnim>();
            armL = visual.Find("Arm_L");
            armR = visual.Find("Arm_R");
            if (armL != null) armLBase = armL.localRotation;
            if (armR != null) armRBase = armR.localRotation;
        }

        private void Update()
        {
            if (visual == null) CacheVisual();
            float speed = 0f;
            if (controller != null)
            {
                Vector3 flat = controller.velocity;
                flat.y = 0f;
                speed = flat.magnitude;
            }

            if (animator != null)
            {
                animator.SetFloat(speedParam, speed);
            }

            if (animator == null && visual != null)
            {
                float sway = Mathf.Sin(Time.time * (4f + speed)) * Mathf.Clamp01(speed / 6f) * 8f;
                visual.localRotation = Quaternion.Euler(0f, 0f, sway);
            }
        }

        public void PlayAttack(int index)
        {
            if (animator != null)
            {
                animator.SetInteger(attackParam, index + 1);
                animator.SetTrigger("AttackTrig");
            }

            StartPunch(index);
        }

        public void PlayDash()
        {
            if (animator != null) animator.SetTrigger(dashParam);
            StartPunch(-1);
        }

        public void PlayPurge()
        {
            StartPunch(9);
        }

        public void PlayRestore()
        {
            StartPunch(8);
        }

        public void PlayHit()
        {
            StartPunch(7);
        }

        private void StartPunch(int index)
        {
            if (visual == null) CacheVisual();
            if (visual == null) return;
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchCo(index));
        }

        private IEnumerator PunchCo(int index)
        {
            if (bob != null) bob.enabled = false;
            float t = 0f;
            float dur = index == -1 ? 0.22f : 0.28f;
            float yaw = index == 2 ? 35f : index >= 8 ? 0f : 18f;
            Vector3 punchScale = index == 2 ? new Vector3(1.15f, 0.9f, 1.2f) : new Vector3(1.08f, 0.95f, 1.12f);
            if (index == -1) punchScale = new Vector3(0.85f, 1.15f, 0.85f);

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = 1f - Mathf.Abs((t / dur) * 2f - 1f);
                visual.localScale = Vector3.Lerp(visualBaseScale, Vector3.Scale(visualBaseScale, punchScale), p);
                visual.localRotation = Quaternion.Euler(0f, yaw * p * (index % 2 == 0 ? 1f : -1f), 0f);

                if (armR != null)
                {
                    armR.localRotation = armRBase * Quaternion.Euler(-80f * p, 0f, 0f);
                }

                if (armL != null && index != 2)
                {
                    armL.localRotation = armLBase * Quaternion.Euler(-40f * p, 0f, 0f);
                }

                yield return null;
            }

            visual.localScale = visualBaseScale;
            visual.localRotation = Quaternion.identity;
            if (armL != null) armL.localRotation = armLBase;
            if (armR != null) armR.localRotation = armRBase;
            if (bob != null) bob.enabled = true;
            punchRoutine = null;
        }
    }
}
