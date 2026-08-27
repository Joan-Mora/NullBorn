using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Prioridad de Thread (XP / nivel). Sube máximos de Vitalidad y Energía.
    /// </summary>
    public class PlayerProgression : MonoBehaviour
    {
        public static PlayerProgression Instance { get; private set; }

        [SerializeField] private int level = 1;
        [SerializeField] private float xp;
        [SerializeField] private float xpToNext = 80f;

        private SystemIntegrity integrity;
        private CpuCycles cpu;

        public int Level => level;
        public float Xp => xp;
        public float XpNormalized => xpToNext <= 0f ? 1f : Mathf.Clamp01(xp / xpToNext);

        public void Initialize(SystemIntegrity systemIntegrity, CpuCycles cpuCycles)
        {
            integrity = systemIntegrity;
            cpu = cpuCycles;
        }

        private void Awake()
        {
            Instance = this;
            if (integrity == null) integrity = GetComponent<SystemIntegrity>();
            if (cpu == null) cpu = GetComponent<CpuCycles>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddXp(float amount, string source)
        {
            if (amount <= 0f) return;
            xp += amount;
            while (xp >= xpToNext)
            {
                xp -= xpToNext;
                LevelUp();
            }

            Debug.Log($"PT +{amount:0} ({source}). Nivel {level}");
        }

        public void Load(int savedLevel, float savedXp)
        {
            level = Mathf.Max(1, savedLevel);
            xp = Mathf.Max(0f, savedXp);
            ApplyStats(false);
        }

        private void LevelUp()
        {
            level++;
            xpToNext = 70f + level * 25f;
            ApplyStats(true);
            NullBorn.Demo.TutorialDirector.Instance?.ShowRaw($"Prioridad de Thread {level}. Vitalidad y Energía ampliadas.", 4f);
        }

        private void ApplyStats(bool refill)
        {
            if (integrity != null) integrity.SetMaxIntegrity(100f + (level - 1) * 12f, refill);
            if (cpu != null) cpu.SetMaxCycles(100f + (level - 1) * 8f, refill);
        }
    }
}
