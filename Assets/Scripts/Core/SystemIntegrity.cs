using UnityEngine;

namespace NullBorn.Core
{
    /// <summary>
    /// ISOMORFISMO: HP del jugador = Integridad del Sistema.
    /// Si llega a 0, el "sistema" colapsa (Game Over).
    /// </summary>
    public class SystemIntegrity : MonoBehaviour
    {
        [Header("Integridad del Sistema")]
        [SerializeField] private float maxIntegrity = 100f;
        [SerializeField] private float currentIntegrity = 100f;

        public float MaxIntegrity => maxIntegrity;
        public float CurrentIntegrity => currentIntegrity;
        public float Normalized => maxIntegrity <= 0f ? 0f : currentIntegrity / maxIntegrity;
        public bool IsCollapsed => currentIntegrity <= 0f;

        public void SetMaxIntegrity(float max, bool refill = false)
        {
            float ratio = Normalized;
            maxIntegrity = Mathf.Max(1f, max);
            currentIntegrity = refill ? maxIntegrity : Mathf.Clamp(maxIntegrity * ratio, 0f, maxIntegrity);
        }

        public void Damage(float amount)
        {
            if (amount <= 0f || IsCollapsed) return;
            currentIntegrity = Mathf.Max(0f, currentIntegrity - amount);
        }

        public void Restore(float amount)
        {
            if (amount <= 0f || IsCollapsed) return;
            currentIntegrity = Mathf.Min(maxIntegrity, currentIntegrity + amount);
        }

        public void ResetIntegrity()
        {
            currentIntegrity = maxIntegrity;
        }

        public void SetCurrentIntegrity(float value)
        {
            currentIntegrity = Mathf.Clamp(value, 0f, maxIntegrity);
        }
    }
}
