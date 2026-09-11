using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.Game
{
    /// <summary>
    /// Cheap runtime graphics. Frame cap: -1 = uncapped (default, F3 can read real FPS).
    /// Set frameCap to 60/120 for an explicit player cap. VSync stays off unless vsync is checked.
    /// </summary>
    public class RuntimePerformanceTweaks : MonoBehaviour
    {
        [SerializeField] float shadowDistance = 40f;
        [Tooltip("-1 = uncapped. 60 or 120 = optional cap. Never force 60 unless this is set.")]
        [SerializeField] int frameCap = -1;
        [SerializeField] bool vsync;
        [SerializeField] bool forceHardShadows = false;
        [SerializeField] bool disableSkinnedMotionVectors = true;
        [SerializeField] bool stripPropShadows = true;

        bool appliedOnce;

        void Awake() => Apply();

        void OnEnable() => Apply();

        void Update() => ApplyFrameCap();

        void Apply()
        {
            ApplyFrameCap();
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowCascades = 1;

            if (appliedOnce) return;
            appliedOnce = true;

            if (forceHardShadows)
            {
                foreach (var light in FindObjectsByType<Light>())
                {
                    if (light.type != LightType.Directional) continue;
                    if (light.shadows == LightShadows.None) continue;
                    light.shadows = LightShadows.Hard;
                }
            }

            if (disableSkinnedMotionVectors)
            {
                foreach (var smr in FindObjectsByType<SkinnedMeshRenderer>())
                {
                    smr.skinnedMotionVectors = false;
                    smr.updateWhenOffscreen = false;
                }
            }

            if (!stripPropShadows) return;

            foreach (var r in FindObjectsByType<MeshRenderer>())
            {
                if (r.GetComponentInParent<CharacterController>() != null) continue;
                if (r.shadowCastingMode == ShadowCastingMode.Off) continue;
                r.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        void ApplyFrameCap()
        {
            if (vsync)
            {
                if (QualitySettings.vSyncCount < 1)
                    QualitySettings.vSyncCount = 1;
                if (Application.targetFrameRate != -1)
                    Application.targetFrameRate = -1;
                return;
            }

            if (QualitySettings.vSyncCount != 0)
                QualitySettings.vSyncCount = 0;

            var cap = frameCap < 1 ? -1 : frameCap;
            if (Application.targetFrameRate != cap)
                Application.targetFrameRate = cap;
        }
    }
}
