using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.Core
{
    /// <summary>
    /// ISOMORFISMO: presión de RAM del proceso. Fragmentos vivos + leaks suben el uso.
    /// RF-07 / RF-15 — barra y alerta en HUD del overworld.
    /// </summary>
    public class RamPressure : MonoBehaviour
    {
        public static RamPressure Instance { get; private set; }

        [SerializeField] private float maxRamUnits = 40f;
        [SerializeField] private float alertNormalized = 0.75f;

        public float Normalized { get; private set; }
        public bool IsCritical => Normalized >= alertNormalized;
        public float AlertThreshold => alertNormalized;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            float units = 0f;
            MemoryLeakEnemy[] leaks = FindObjectsByType<MemoryLeakEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < leaks.Length; i++)
            {
                if (leaks[i] == null || !leaks[i].IsAlive) continue;
                units += leaks[i].Variant == LeakVariant.Boss ? 8f : leaks[i].Variant == LeakVariant.Spawner ? 4f : 2f;
            }

            MemoryFragment[] frags = FindObjectsByType<MemoryFragment>(FindObjectsSortMode.None);
            units += frags.Length;

            Normalized = maxRamUnits <= 0f ? 0f : Mathf.Clamp01(units / maxRamUnits);
        }
    }
}
