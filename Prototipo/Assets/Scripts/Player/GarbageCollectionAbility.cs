using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// ISOMORFISMO: Curarse / limpiar = Garbage Collection (GC).
    /// Tecla E o G: gasta CPU, limpia basura cercana y restaura integridad.
    /// </summary>
    public class GarbageCollectionAbility : MonoBehaviour
    {
        [Header("Garbage Collection")]
        [SerializeField] private float cpuCost = 28f;
        [SerializeField] private float restoreIntegrity = 18f;
        [SerializeField] private float radius = 7f;
        [SerializeField] private float leakDamage = 25f;

        private CpuCycles cpu;
        private SystemIntegrity integrity;
        private GameSession session;

        public void Initialize(CpuCycles cpuCycles, SystemIntegrity systemIntegrity, GameSession gameSession)
        {
            cpu = cpuCycles;
            integrity = systemIntegrity;
            session = gameSession;
        }

        private void Awake()
        {
            if (GetComponent<PurgeWaveAbility>() != null) enabled = false;
        }

        private void Update()
        {
            if (session != null && session.CurrentState != GameSession.State.Playing) return;

            bool interactPresent = GetComponent<PlayerInteract>() != null;
            bool pressed = Input.GetKeyDown(KeyCode.G) || (!interactPresent && Input.GetKeyDown(KeyCode.E));
            if (!pressed) return;

            TryRunGarbageCollection();
        }

        private void TryRunGarbageCollection()
        {
            if (cpu == null || integrity == null) return;
            if (!cpu.TrySpend(cpuCost))
            {
                Debug.Log("GC falló: ciclos de CPU insuficientes.");
                return;
            }

            int cleaned = 0;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                MemoryFragment fragment = hit.GetComponent<MemoryFragment>();
                if (fragment != null)
                {
                    fragment.Collect();
                    cleaned++;
                    continue;
                }

                MemoryLeakEnemy leak = hit.GetComponentInParent<MemoryLeakEnemy>();
                if (leak != null)
                {
                    leak.ApplyGarbageCollection(leakDamage);
                }
            }

            integrity.Restore(restoreIntegrity + cleaned * 2f);
            NullBorn.Art.GcPulseVfx.Play(transform.position, radius);
            NullBorn.Api.NullBornApi.Current?.RecordGcEvent("garbage_collection", cleaned, leakDamage);
            Debug.Log($"GC ejecutado. Fragmentos liberados: {cleaned}");
        }
    }
}
