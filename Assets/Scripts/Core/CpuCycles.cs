using UnityEngine;

namespace NullBorn.Core
{
    /// <summary>
    /// ISOMORFISMO: Stamina / energía = Ciclos de CPU.
    /// Cada acción (atacar, Garbage Collection) consume CPU.
    /// La CPU se regenera con el tiempo (como un procesador que se enfría).
    /// </summary>
    public class CpuCycles : MonoBehaviour
    {
        [Header("Ciclos de CPU")]
        [SerializeField] private float maxCycles = 100f;
        [SerializeField] private float currentCycles = 100f;
        [SerializeField] private float regenPerSecond = 18f;

        public float MaxCycles => maxCycles;
        public float CurrentCycles => currentCycles;
        public float Normalized => maxCycles <= 0f ? 0f : currentCycles / maxCycles;
        public float RegenPerSecond => regenPerSecond;

        public bool CanAfford(float cost) => cost <= 0f || currentCycles >= cost;

        public void Restore(float amount)
        {
            if (amount <= 0f) return;
            currentCycles = Mathf.Min(maxCycles, currentCycles + amount);
        }

        public void SetMaxCycles(float max, bool refill = false)
        {
            float ratio = Normalized;
            maxCycles = Mathf.Max(1f, max);
            currentCycles = refill ? maxCycles : Mathf.Clamp(maxCycles * ratio, 0f, maxCycles);
        }

        public void SetRegen(float perSecond)
        {
            regenPerSecond = Mathf.Max(0f, perSecond);
        }

        private void Update()
        {
            if (currentCycles < maxCycles)
            {
                currentCycles = Mathf.Min(maxCycles, currentCycles + regenPerSecond * Time.deltaTime);
            }
        }

        /// <summary>
        /// Intenta gastar ciclos. Devuelve true si había CPU suficiente.
        /// </summary>
        public bool TrySpend(float cost)
        {
            if (cost <= 0f) return true;
            if (currentCycles < cost) return false;

            currentCycles -= cost;
            return true;
        }

        public void ResetCycles()
        {
            currentCycles = maxCycles;
        }

        public void SetCurrentCycles(float value)
        {
            currentCycles = Mathf.Clamp(value, 0f, maxCycles);
        }
    }
}
