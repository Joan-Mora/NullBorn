using System.Collections;
using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Corte de Traza — combo L-L-H con hitboxes en ventanas de coroutine.
    /// Reemplaza DebugStrikeAbility cuando está presente.
    /// </summary>
    public class TraceCutCombo : MonoBehaviour
    {
        [SerializeField] private float lightCost = 8f;
        [SerializeField] private float heavyCost = 14f;
        [SerializeField] private float lightDamage = 16f;
        [SerializeField] private float heavyDamage = 28f;
        [SerializeField] private float comboReset = 0.85f;

        private CpuCycles cpu;
        private GameSession session;
        private ThirdPersonController movement;
        private PlayerAnimBridge anim;
        private CombatFeedback feedback;
        private int chain;
        private float chainUntil;
        private bool attacking;
        private GameObject trail;

        public bool IsAttacking => attacking;

        public void Initialize(CpuCycles cpuCycles, GameSession gameSession)
        {
            cpu = cpuCycles;
            session = gameSession;
            movement = GetComponent<ThirdPersonController>();
            anim = GetComponent<PlayerAnimBridge>();
            feedback = GetComponent<CombatFeedback>();
            if (feedback == null) feedback = gameObject.AddComponent<CombatFeedback>();
        }

        private void Start()
        {
            if (cpu == null) cpu = GetComponent<CpuCycles>();
            if (session == null) session = GameSession.Instance;
            if (movement == null) movement = GetComponent<ThirdPersonController>();
            if (anim == null) anim = GetComponent<PlayerAnimBridge>();
            if (feedback == null) feedback = GetComponent<CombatFeedback>() ?? gameObject.AddComponent<CombatFeedback>();
        }

        private void Update()
        {
            if (session != null && session.CurrentState != GameSession.State.Playing) return;
            if (Time.timeScale <= 0.01f) return;

            if (Time.time > chainUntil) chain = 0;

            bool pressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F) ||
                           (movement != null && movement.ConsumeBufferedAttack());
            if (!pressed || attacking) return;

            TryNextHit();
        }

        private void TryNextHit()
        {
            bool heavy = chain >= 2;
            float cost = heavy ? heavyCost : lightCost;
            if (cpu != null && !cpu.TrySpend(cost))
            {
                CombatFeedback.NotifyDeny(transform.position);
                return;
            }

            StartCoroutine(HitWindow(chain, heavy));
        }

        private IEnumerator HitWindow(int index, bool heavy)
        {
            attacking = true;
            anim?.PlayAttack(index);

            float windup = heavy ? 0.22f : 0.1f;
            float active = heavy ? 0.16f : 0.11f;
            float recover = heavy ? 0.28f : 0.16f;

            yield return new WaitForSeconds(windup);
            ShowTrail(heavy);
            bool hit = ResolveHit(heavy);
            HideTrail();

            if (hit)
            {
                feedback?.HitStop(heavy ? 0.06f : 0.045f);
                feedback?.Shake(heavy ? 0.16f : 0.07f);
                CombatFeedback.PlayHitSfx(transform.position + transform.forward, heavy);
            }
            else
            {
                CombatFeedback.PlayMissSfx(transform.position);
            }

            yield return new WaitForSeconds(active + recover);

            chain = heavy ? 0 : index + 1;
            chainUntil = Time.time + comboReset;
            attacking = false;

            if (!heavy && movement != null && movement.ConsumeBufferedAttack())
            {
                TryNextHit();
            }
        }

        private bool ResolveHit(bool heavy)
        {
            float range = heavy ? 2.6f : 2.1f;
            Vector3 center = transform.position + transform.forward * (range * 0.55f) + Vector3.up * 0.9f;
            Vector3 half = new Vector3(0.7f, 0.7f, range * 0.5f);
            Collider[] hits = Physics.OverlapBox(center, half, transform.rotation);
            bool any = false;
            float damage = heavy ? heavyDamage : lightDamage;

            for (int i = 0; i < hits.Length; i++)
            {
                MemoryLeakEnemy leak = hits[i].GetComponentInParent<MemoryLeakEnemy>();
                if (leak == null || !leak.IsAlive) continue;
                leak.TakeDamage(damage);
                any = true;
                feedback?.DamagePopup(leak.transform.position, damage, heavy);
                if (heavy)
                {
                    Vector3 kb = (leak.transform.position - transform.position).normalized * 3.2f;
                    kb.y = 0.4f;
                    leak.ApplyKnockback(kb);
                }
            }

            return any;
        }

        private void ShowTrail(bool heavy)
        {
            if (trail != null) Destroy(trail);
            trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trail.name = "VFX_TraceArc";
            Destroy(trail.GetComponent<Collider>());
            trail.transform.SetParent(transform, false);
            trail.transform.localPosition = new Vector3(0f, 1f, 1.1f);
            trail.transform.localScale = heavy ? new Vector3(1.6f, 0.08f, 2.2f) : new Vector3(1.1f, 0.06f, 1.6f);
            trail.transform.localRotation = Quaternion.Euler(0f, 0f, heavy ? -25f : 20f);
            var rend = trail.GetComponent<Renderer>();
            rend.sharedMaterial = NullBorn.Art.RuntimeMaterials.Lit(
                new Color(0.3f, 0.95f, 1f, 0.7f),
                new Color(0.2f, 0.9f, 1f) * 2.4f);
        }

        private void HideTrail()
        {
            if (trail == null) return;
            Destroy(trail, 0.08f);
            trail = null;
        }
    }
}
