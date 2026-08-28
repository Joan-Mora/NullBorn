using System.Collections;
using NullBorn.Art;
using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Restaurar — canalización defensiva (evolución del GC curativo). Tecla V.
    /// Bonus si no hay enemigos en melee.
    /// </summary>
    public class RestoreAbility : MonoBehaviour
    {
        [SerializeField] private float cpuCost = 22f;
        [SerializeField] private float restoreAmount = 22f;
        [SerializeField] private float meleeBonus = 10f;
        [SerializeField] private float cooldown = 12f;
        [SerializeField] private float channelTime = 0.6f;
        [SerializeField] private float meleeRange = 3.2f;

        private CpuCycles cpu;
        private SystemIntegrity integrity;
        private GameSession session;
        private ThirdPersonController movement;
        private float readyAt;
        private bool channeling;

        public void Initialize(CpuCycles cpuCycles, SystemIntegrity systemIntegrity, GameSession gameSession)
        {
            cpu = cpuCycles;
            integrity = systemIntegrity;
            session = gameSession;
            movement = GetComponent<ThirdPersonController>();
        }

        private void Start()
        {
            if (cpu == null) cpu = GetComponent<CpuCycles>();
            if (integrity == null) integrity = GetComponent<SystemIntegrity>();
            if (session == null) session = GameSession.Instance;
            if (movement == null) movement = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            if (session != null && session.CurrentState != GameSession.State.Playing) return;
            if (channeling) return;
            if (!Input.GetKeyDown(KeyCode.V)) return;
            if (Time.time < readyAt)
            {
                CombatFeedback.NotifyDeny(transform.position);
                return;
            }

            StartCoroutine(Channel());
        }

        private IEnumerator Channel()
        {
            if (cpu != null && !cpu.TrySpend(cpuCost))
            {
                CombatFeedback.NotifyDeny(transform.position);
                yield break;
            }

            channeling = true;
            if (movement != null) movement.MovementLocked = true;
            GetComponent<PlayerAnimBridge>()?.PlayRestore();
            CombatVfx.PlayRestore(transform.position);

            float t = 0f;
            bool cancelled = false;
            while (t < channelTime)
            {
                if (movement != null && movement.IsDashing)
                {
                    cancelled = true;
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            if (!cancelled && integrity != null)
            {
                bool safe = !EnemyInMelee();
                integrity.Restore(restoreAmount + (safe ? meleeBonus : 0f));
                CombatFeedback.PlayTone(transform.position, 480f, 0.16f);
            }

            readyAt = Time.time + cooldown;
            if (movement != null) movement.MovementLocked = false;
            channeling = false;
        }

        private bool EnemyInMelee()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, meleeRange);
            for (int i = 0; i < hits.Length; i++)
            {
                MemoryLeakEnemy leak = hits[i].GetComponentInParent<MemoryLeakEnemy>();
                if (leak != null && leak.IsAlive) return true;
            }

            return false;
        }
    }
}
