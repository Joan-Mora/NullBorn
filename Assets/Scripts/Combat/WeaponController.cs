using UnityEngine;
using UnityEngine.InputSystem;

namespace NullBorn.Game.Combat
{
    /// <summary>
    /// Single sword instance. Sheathed on WeaponHolster, Drawn on hand_r.
    /// Never Instantiate a second mesh — only reparent this one.
    ///
    /// Playtest:
    /// 1. Run in circles — holster stays off the hood/cape (no grave clip).
    /// 2. R (or Q) draws / sheathes the SAME sword. Never two blades.
    /// 3. Click/F while Drawn: slash, legs keep walk/run (upper-body Attack mask).
    /// 4. Click/F while Sheathed: Draw immediately, attack after 0.2s buffer.
    /// 5. Hierarchy: exactly one Weapon_Sword. Attack must not spawn another.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        public enum State
        {
            Sheathed,
            Drawn
        }

        [SerializeField] Transform sword;
        [SerializeField] Transform holster;
        [SerializeField] Transform hand;
        [SerializeField] Animator animator;

        [SerializeField] Vector3 holsterLocalPosition;
        [SerializeField] Quaternion holsterLocalRotation = Quaternion.identity;
        [SerializeField] Vector3 holsterLocalScale = Vector3.one;
        [SerializeField] Vector3 gripLocalPosition;
        [SerializeField] Quaternion gripLocalRotation = Quaternion.identity;
        [SerializeField] Vector3 gripLocalScale = Vector3.one;

        CombatMeleeAttack melee;
        State state = State.Sheathed;

        public bool IsDrawn => state == State.Drawn;
        public Transform Sword => sword;

        public void Wire(
            Animator anim,
            Transform swordTransform,
            Transform holsterTransform,
            Transform handTransform,
            Vector3 holsterPos,
            Quaternion holsterRot,
            Vector3 holsterScale,
            Vector3 gripPos,
            Quaternion gripRot,
            Vector3 gripScale)
        {
            animator = anim;
            sword = swordTransform;
            holster = holsterTransform;
            hand = handTransform;
            holsterLocalPosition = holsterPos;
            holsterLocalRotation = holsterRot;
            holsterLocalScale = holsterScale.sqrMagnitude > 0.0001f ? holsterScale : Vector3.one;
            gripLocalPosition = gripPos;
            gripLocalRotation = gripRot;
            gripLocalScale = gripScale.sqrMagnitude > 0.0001f ? gripScale : Vector3.one;
            DestroyExtraSwords();
            Sheathe();
        }

        void Awake()
        {
            melee = GetComponentInChildren<CombatMeleeAttack>(true)
                    ?? GetComponentInParent<CombatMeleeAttack>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            DestroyExtraSwords();
        }

        void Start()
        {
            ResolveSockets();
            if (sword != null && state == State.Sheathed && holster != null && sword.parent != holster)
                Sheathe();
        }

        void Update()
        {
            if (PauseMenuController.IsPaused) return;

            var kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.rKey.wasPressedThisFrame && !kb.qKey.wasPressedThisFrame) return;

            Toggle();
        }

        public void Toggle()
        {
            if (IsDrawn) Sheathe();
            else Draw();
        }

        public void Draw()
        {
            ResolveSockets();
            if (sword == null || hand == null) return;
            if (melee != null && melee.IsAttacking && IsDrawn) return;

            DestroyExtraSwords();
            sword.SetParent(hand, false);
            sword.localPosition = gripLocalPosition;
            sword.localRotation = gripLocalRotation;
            sword.localScale = gripLocalScale;
            state = State.Drawn;
        }

        public void Sheathe()
        {
            if (melee != null && melee.IsAttacking) return;

            ResolveSockets();
            if (sword == null || holster == null) return;

            DestroyExtraSwords();
            sword.SetParent(holster, false);
            sword.localPosition = holsterLocalPosition;
            sword.localRotation = holsterLocalRotation;
            sword.localScale = holsterLocalScale;
            state = State.Sheathed;
        }

        void ResolveSockets()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (sword == null)
            {
                foreach (var t in GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Weapon_Sword")
                    {
                        sword = t;
                        break;
                    }
                }
            }

            if (holster == null)
            {
                foreach (var t in GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "WeaponHolster")
                    {
                        holster = t;
                        break;
                    }
                }
            }

            if (hand == null && animator != null && animator.isHuman)
                hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }

        void DestroyExtraSwords()
        {
            Transform keep = sword;
            var all = GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.name != "Weapon_Sword") continue;
                if (keep == null)
                {
                    keep = t;
                    sword = t;
                    continue;
                }

                if (t != keep)
                {
                    Debug.LogWarning("[NullBorn][Combat] Extra Weapon_Sword destroyed (one instance lock).");
                    if (Application.isPlaying)
                        Destroy(t.gameObject);
                    else
                        DestroyImmediate(t.gameObject);
                }
            }
        }
    }
}
