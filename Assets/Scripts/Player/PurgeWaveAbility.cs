using System.Collections;
using NullBorn.Art;
using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Onda de Purga — AoE ofensivo (evolución del GC ofensivo). No cura.
    /// Tecla G. Cooldown 8–10 s. Carga 0,4 s vulnerable.
    /// </summary>
    public class PurgeWaveAbility : MonoBehaviour
    {
        [SerializeField] private float cpuCost = 32f;
        [SerializeField] private float radius = 6.5f;
        [SerializeField] private float leakDamage = 38f;
        [SerializeField] private float cooldown = 9f;
        [SerializeField] private float chargeTime = 0.4f;

        private CpuCycles cpu;
        private GameSession session;
        private ThirdPersonController movement;
        private float readyAt;
        private bool casting;

        public float CooldownNormalized => cooldown <= 0f ? 1f : Mathf.Clamp01(1f - (readyAt - Time.time) / cooldown);

        public void Initialize(CpuCycles cpuCycles, GameSession gameSession)
        {
            cpu = cpuCycles;
            session = gameSession;
            movement = GetComponent<ThirdPersonController>();
        }

        private void Start()
        {
            if (cpu == null) cpu = GetComponent<CpuCycles>();
            if (session == null) session = GameSession.Instance;
            if (movement == null) movement = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            if (session != null && session.CurrentState != GameSession.State.Playing) return;
            if (casting) return;
            if (!Input.GetKeyDown(KeyCode.G)) return;
            if (Time.time < readyAt)
            {
                CombatFeedback.NotifyDeny(transform.position);
                return;
            }

            StartCoroutine(Cast());
        }

        private IEnumerator Cast()
        {
            if (cpu != null && !cpu.TrySpend(cpuCost))
            {
                CombatFeedback.NotifyDeny(transform.position);
                yield break;
            }

            casting = true;
            if (movement != null) movement.MovementLocked = true;
            GetComponent<PlayerAnimBridge>()?.PlayPurge();

            yield return new WaitForSeconds(chargeTime);

            int cleaned = 0;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            for (int i = 0; i < hits.Length; i++)
            {
                MemoryFragment fragment = hits[i].GetComponent<MemoryFragment>();
                if (fragment != null)
                {
                    fragment.Collect();
                    cleaned++;
                    continue;
                }

                MemoryLeakEnemy leak = hits[i].GetComponentInParent<MemoryLeakEnemy>();
                leak?.ApplyGarbageCollection(leakDamage);
            }

            if (cleaned > 0)
            {
                ChainCleanFragments(4f);
            }

            CombatVfx.PlayPulse(transform.position, radius, new Color(0.95f, 0.25f, 0.85f), new Color(1f, 0.2f, 0.7f) * 2f);
            CombatFeedback fb = GetComponent<CombatFeedback>() ?? CombatFeedback.Instance;
            fb?.Shake(0.26f);
            fb?.HitStop(0.05f);
            CombatFeedback.PlayTone(transform.position, 160f, 0.18f);

            NullBorn.Api.INullBornApi api = NullBorn.Api.NullBornApi.Current;
            api?.RecordGcEvent("purge_wave", cleaned, leakDamage);

            readyAt = Time.time + cooldown;
            if (movement != null) movement.MovementLocked = false;
            casting = false;
        }

        private void ChainCleanFragments(float chainRadius)
        {
            MemoryFragment[] all = FindObjectsByType<MemoryFragment>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null) continue;
                if (Vector3.Distance(transform.position, all[i].transform.position) <= radius + chainRadius)
                {
                    all[i].Collect();
                }
            }
        }
    }
}
