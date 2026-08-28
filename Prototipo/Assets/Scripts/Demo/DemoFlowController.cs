using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Demo
{
    /// <summary>
    /// Orquesta el mini-demo de 3 zonas y la victoria al matar al boss.
    /// </summary>
    public class DemoFlowController : MonoBehaviour
    {
        public static DemoFlowController Instance { get; private set; }

        [SerializeField] private DemoZoneId currentZone = DemoZoneId.BootSector;
        [SerializeField] private MemoryLeakEnemy bossLeak;

        public DemoZoneId CurrentZone => currentZone;
        public MemoryLeakEnemy BossLeak => bossLeak;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetBoss(MemoryLeakEnemy boss)
        {
            if (bossLeak != null)
            {
                bossLeak.OnDied -= HandleBossDied;
            }

            bossLeak = boss;
            if (bossLeak != null)
            {
                bossLeak.OnDied += HandleBossDied;
            }
        }

        public void NotifyZoneEntered(DemoZoneId zone)
        {
            currentZone = zone;
            Debug.Log($"Zona activa: {zone}");
        }

        private void HandleBossDied(MemoryLeakEnemy _)
        {
            TutorialDirector.Instance?.ShowRaw("LEAK BOSS CONTENIDO. Sistema estabilizado.", 5f);
            GameSession.Instance?.NotifyVictory();
        }
    }
}
