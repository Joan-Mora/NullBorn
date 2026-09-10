using UnityEngine;

namespace NullBorn.Game
{
    /// <summary>
    /// Discrete locomotion states with CrossFade. One clip at a time — no BlendTree dual-skinning.
    /// Requires PlayerLocomotion.controller with Idle / Walk / Run / Backpedal / Roll states.
    /// </summary>
    [RequireComponent(typeof(SimpleThirdPersonMotor))]
    public class SimpleLocomotionAnim : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] float fadeIn = 0.2f;
        [SerializeField] float fadeOut = 0.22f;
        [SerializeField] float idleHoldSeconds = 0.08f;

        SimpleThirdPersonMotor motor;
        int idleHash;
        int walkHash;
        int runHash;
        int backHash;
        int rollHash;
        int playingHash;
        bool rolling;
        float idleTimer;
        LocomotionPose latched = LocomotionPose.Idle;

        void Awake()
        {
            motor = GetComponent<SimpleThirdPersonMotor>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            idleHash = Animator.StringToHash("Idle");
            walkHash = Animator.StringToHash("Walk");
            runHash = Animator.StringToHash("Run");
            backHash = Animator.StringToHash("Backpedal");
            rollHash = Animator.StringToHash("Roll");
            playingHash = idleHash;
        }

        void Start()
        {
            if (animator == null) return;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.speed = 1f;

            // Prewarm Idle so the first transition isn't a cold clip hitch.
            if (Has(idleHash))
            {
                animator.Play(idleHash, 0, 0f);
                animator.Update(0f);
                playingHash = idleHash;
            }
            else
            {
                Debug.LogError(
                    "[NullBorn] Falta controller discreto. Menú: NullBorn → Rebuild Locomotion Controller");
            }
        }

        void Update()
        {
            if (animator == null || motor == null) return;

            if (motor.RollRequested)
            {
                motor.ConsumeRollRequest();
                rolling = true;
                Fade(rollHash, 0.1f);
            }

            if (rolling || motor.IsRolling)
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash == rollHash && info.normalizedTime >= 0.9f && !animator.IsInTransition(0))
                {
                    rolling = false;
                    motor.NotifyRollAnimationEnded();
                    latched = LocomotionPose.Idle;
                    idleTimer = 0f;
                    Fade(idleHash, fadeOut);
                }
                else if (!motor.IsRolling && info.shortNameHash != rollHash)
                {
                    rolling = false;
                }

                return;
            }

            var wanted = motor.Pose;

            // Stay in move poses until we've truly stopped for a beat (avoids Idle flicker).
            if (wanted == LocomotionPose.Idle)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer < idleHoldSeconds && latched != LocomotionPose.Idle)
                    wanted = latched;
            }
            else
            {
                idleTimer = 0f;
            }

            if (wanted == latched) return;
            latched = wanted;

            var fade = wanted == LocomotionPose.Idle ? fadeOut : fadeIn;
            Fade(HashFor(wanted), fade);
        }

        int HashFor(LocomotionPose pose)
        {
            switch (pose)
            {
                case LocomotionPose.Walk: return Has(walkHash) ? walkHash : idleHash;
                case LocomotionPose.Run: return Has(runHash) ? runHash : (Has(walkHash) ? walkHash : idleHash);
                case LocomotionPose.Backpedal: return Has(backHash) ? backHash : (Has(walkHash) ? walkHash : idleHash);
                default: return idleHash;
            }
        }

        void Fade(int stateHash, float seconds)
        {
            if (!Has(stateHash)) return;
            if (playingHash == stateHash && !animator.IsInTransition(0)) return;
            animator.CrossFadeInFixedTime(stateHash, Mathf.Max(0.05f, seconds), 0, 0f);
            playingHash = stateHash;
        }

        bool Has(int stateHash) => animator != null && animator.HasState(0, stateHash);
    }
}
