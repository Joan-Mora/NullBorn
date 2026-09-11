using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace NullBorn.Game.Combat
{
    /// <summary>
    /// Cap 0 — Corte de Traza (1 hit).
    ///
    /// Clip: UAL1 <c>UAL1_Standard.fbx</c> take <c>Armature|Sword_Attack</c>
    ///       → <c>Assets/Art/Animation/Cap0_SwordAttack.anim</c>, event <see cref="Hit"/> @ 35%.
    /// Animator layer <c>Attack</c> + upper-body AvatarMask (spine/chest/arms/head).
    /// Legs stay on loco layer 0. Motor is never frozen.
    ///
    /// Playtest:
    /// - Run: one sheathed sword, no grave hood clip, hair stays visible.
    /// - R/Q: draw/sheathe the same mesh. Never two swords.
    /// - Click/F Drawn: slash while walking/running (legs keep moving).
    /// - Click/F Sheathed: Draw + attack after 0.2s.
    /// </summary>
    public class CombatMeleeAttack : MonoBehaviour
    {
        const string AttackLayerName = "Attack";
        const string LegacyMeleeLayerName = "Melee";
        const string AttackStateName = "Attack";
        const float HitEventNormalizedTime = 0.35f;
        const float AttackFade = 0.12f;
        const float DrawAttackBuffer = 0.2f;

        [SerializeField] Animator animator;
        [SerializeField] WeaponController weapon;
        [SerializeField] float hitstopSeconds = 0.05f;
        [SerializeField] float damage = 1f;
        [SerializeField] Vector3 hitboxHalfExtents = new Vector3(0.55f, 0.7f, 0.65f);
        [SerializeField] float hitboxForward = 1.05f;
        [SerializeField] float hitboxHeight = 1.0f;
        [SerializeField] AudioClip hitSfx;

        static readonly Collider[] OverlapHits = new Collider[16];

        SimpleThirdPersonMotor motor;
        int attackLayer = -1;
        int attackHash;
        int emptyHash;
        bool attacking;
        bool hitstopRunning;
        bool hitConsumed;
        float pendingAttackAt = -1f;
        Coroutine attackWatch;

        public bool IsAttacking => attacking;

        void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            motor = GetComponentInParent<SimpleThirdPersonMotor>();
            if (weapon == null)
                weapon = GetComponentInParent<WeaponController>();
            attackHash = Animator.StringToHash(AttackStateName);
            emptyHash = Animator.StringToHash("Empty");
            ResolveAttackLayer();
        }

        void ResolveAttackLayer()
        {
            attackLayer = -1;
            if (animator == null) return;
            attackLayer = animator.GetLayerIndex(AttackLayerName);
            if (attackLayer < 0)
                attackLayer = animator.GetLayerIndex(LegacyMeleeLayerName);
        }

        void Update()
        {
            if (PauseMenuController.IsPaused) return;

            if (pendingAttackAt > 0f)
            {
                if (weapon != null && !weapon.IsDrawn)
                    pendingAttackAt = -1f;
                else if (Time.time >= pendingAttackAt)
                {
                    pendingAttackAt = -1f;
                    BeginAttack();
                }
            }

            if (!WantsAttack()) return;
            TryAttack();
        }

        bool WantsAttack()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame)
                return true;

            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return false;

            if (Cursor.lockState != CursorLockMode.Locked
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject())
                return false;

            return true;
        }

        void TryAttack()
        {
            if (attacking) return;
            if (motor != null && motor.IsRolling) return;

            if (weapon != null && !weapon.IsDrawn)
            {
                weapon.Draw();
                pendingAttackAt = Time.time + DrawAttackBuffer;
                return;
            }

            BeginAttack();
        }

        void BeginAttack()
        {
            if (attacking) return;
            if (motor != null && motor.IsRolling) return;
            if (weapon != null && !weapon.IsDrawn)
                weapon.Draw();

            if (animator == null)
            {
                Debug.LogWarning("[NullBorn][Combat] No Animator for melee.");
                return;
            }

            if (attackLayer < 0)
                ResolveAttackLayer();
            if (attackLayer < 0 || !animator.HasState(attackLayer, attackHash))
            {
                Debug.LogWarning(
                    "[NullBorn][Combat] Attack layer/state missing. Rebuild Art Test Scene "
                    + "(UAL1 Armature|Sword_Attack → Cap0_SwordAttack, layer Attack + upper mask, event Hit @ "
                    + HitEventNormalizedTime + ").");
                return;
            }

            attacking = true;
            hitConsumed = false;
            pendingAttackAt = -1f;

            animator.SetLayerWeight(attackLayer, 1f);
            animator.CrossFadeInFixedTime(attackHash, AttackFade, attackLayer, 0f);

            if (attackWatch != null)
                StopCoroutine(attackWatch);
            attackWatch = StartCoroutine(WatchAttackEnd());
        }

        IEnumerator WatchAttackEnd()
        {
            yield return null;
            var guard = 3f;
            while (attacking && animator != null && guard > 0f)
            {
                guard -= Time.unscaledDeltaTime;
                if (motor != null && motor.IsRolling)
                    break;

                var info = animator.GetCurrentAnimatorStateInfo(attackLayer);
                if (!hitConsumed && info.shortNameHash == attackHash && info.normalizedTime >= HitEventNormalizedTime)
                    Hit();

                if (info.shortNameHash == attackHash && info.normalizedTime >= 0.9f && !animator.IsInTransition(attackLayer))
                    break;

                yield return null;
            }

            EndAttack();
        }

        void EndAttack()
        {
            attacking = false;
            if (animator == null || attackLayer < 0)
            {
                attackWatch = null;
                return;
            }

            if (animator.HasState(attackLayer, emptyHash))
                animator.CrossFadeInFixedTime(emptyHash, AttackFade, attackLayer, 0f);
            attackWatch = StartCoroutine(FadeAttackLayerOut());
        }

        IEnumerator FadeAttackLayerOut()
        {
            var t = AttackFade;
            while (t > 0f && animator != null && attackLayer >= 0)
            {
                t -= Time.deltaTime;
                animator.SetLayerWeight(attackLayer, Mathf.Clamp01(t / AttackFade));
                yield return null;
            }

            if (animator != null && attackLayer >= 0)
                animator.SetLayerWeight(attackLayer, 0f);
            attackWatch = null;
        }

        /// <summary>
        /// Animation event on Cap0_SwordAttack (UAL1 Armature|Sword_Attack @ 35%).
        /// Hitbox only on this frame.
        /// </summary>
        public void Hit()
        {
            if (!attacking || hitConsumed) return;
            hitConsumed = true;

            var body = motor != null ? motor.transform : transform.root;
            var origin = body.position + Vector3.up * hitboxHeight + body.forward * hitboxForward;
            var count = Physics.OverlapBoxNonAlloc(
                origin,
                hitboxHalfExtents,
                OverlapHits,
                body.rotation,
                ~0,
                QueryTriggerInteraction.Collide);

            var connected = false;
            for (var i = 0; i < count; i++)
            {
                var col = OverlapHits[i];
                if (col == null) continue;
                if (col.transform.root == body.root) continue;

                var dummy = col.GetComponentInParent<CombatDummy>();
                if (dummy == null) continue;
                if (!dummy.ReceiveHit(damage)) continue;
                connected = true;
            }

            if (connected)
            {
                if (hitSfx != null)
                    AudioSource.PlayClipAtPoint(hitSfx, origin);
                if (!hitstopRunning && hitstopSeconds > 0f)
                    StartCoroutine(Hitstop());
            }
        }

        IEnumerator Hitstop()
        {
            hitstopRunning = true;
            var prev = Time.timeScale;
            Time.timeScale = 0.12f;
            yield return new WaitForSecondsRealtime(Mathf.Clamp(hitstopSeconds, 0.04f, 0.06f));
            Time.timeScale = prev > 0.01f ? prev : 1f;
            hitstopRunning = false;
        }

        void OnDisable()
        {
            if (hitstopRunning)
            {
                Time.timeScale = 1f;
                hitstopRunning = false;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var body = motor != null ? motor.transform : transform.root;
            var origin = body.position + Vector3.up * hitboxHeight + body.forward * hitboxForward;
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.35f);
            Gizmos.matrix = Matrix4x4.TRS(origin, body.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitboxHalfExtents * 2f);
        }
#endif
    }
}
