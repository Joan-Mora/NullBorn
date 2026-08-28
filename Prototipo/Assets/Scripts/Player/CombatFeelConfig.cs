using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Parámetros de game feel (hitstop, shake, dash). Defaults usables sin asset.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatFeelConfig", menuName = "NullBorn/Combat Feel Config")]
    public class CombatFeelConfig : ScriptableObject
    {
        [Header("Hitstop")]
        public float lightHitstop = 0.045f;
        public float heavyHitstop = 0.07f;

        [Header("Cámara")]
        public float lightShake = 0.08f;
        public float heavyShake = 0.2f;
        public float purgeShake = 0.28f;
        public float lockOnDistance = 4.5f;
        public float defaultDistance = 5.5f;

        [Header("Dash")]
        public float dashDuration = 0.22f;
        public float dashIFrameDuration = 0.25f;
        public float dashDistance = 5.5f;
        public float dashCpuCost = 16f;
        public float dashCooldown = 0.4f;
        public float inputBuffer = 0.18f;

        [Header("Sprint")]
        public float sprintMultiplier = 1.55f;
        public float sprintCpuPerSecond = 7f;

        public static CombatFeelConfig CreateRuntimeDefault()
        {
            return CreateInstance<CombatFeelConfig>();
        }
    }
}
