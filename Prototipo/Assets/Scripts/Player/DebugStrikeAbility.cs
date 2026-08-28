using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Ataque básico del hilo de ejecución: "depurar" el leak cercano.
    /// Click izquierdo o F. Gasta CPU.
    /// </summary>
    public class DebugStrikeAbility : MonoBehaviour
    {
        [Header("Depuración (ataque)")]
        [SerializeField] private float cpuCost = 12f;
        [SerializeField] private float damage = 18f;
        [SerializeField] private float range = 3.2f;

        private CpuCycles cpu;
        private GameSession session;

        public void Initialize(CpuCycles cpuCycles, GameSession gameSession)
        {
            cpu = cpuCycles;
            session = gameSession;
        }

        private void Awake()
        {
            if (GetComponent<TraceCutCombo>() != null)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (GetComponent<TraceCutCombo>() != null)
            {
                enabled = false;
                return;
            }

            if (session != null && session.CurrentState != GameSession.State.Playing) return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
            {
                TryStrike();
            }
        }

        private void TryStrike()
        {
            if (cpu == null) return;
            if (!cpu.TrySpend(cpuCost))
            {
                Debug.Log("Ataque falló: ciclos de CPU insuficientes.");
                return;
            }

            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.2f, range * 0.55f);
            MemoryLeakEnemy best = null;
            float bestDist = float.MaxValue;

            foreach (Collider hit in hits)
            {
                MemoryLeakEnemy leak = hit.GetComponentInParent<MemoryLeakEnemy>();
                if (leak == null) continue;

                float dist = Vector3.Distance(transform.position, leak.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = leak;
                }
            }

            if (best != null)
            {
                best.TakeDamage(damage);
            }
        }
    }
}
