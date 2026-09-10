using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.Game
{
    /// <summary>
    /// Applies cheap runtime graphics settings and keeps the 60 FPS cap enforced.
    /// </summary>
    public class RuntimePerformanceTweaks : MonoBehaviour
    {
        [SerializeField] float shadowDistance = 18f;
        [SerializeField] int targetFps = 60;
        [SerializeField] bool forceHardShadows = true;
        [SerializeField] bool disableSkinnedMotionVectors = true;
        [SerializeField] bool stripPropShadows = true;

        bool appliedOnce;

        void Awake() => Apply();

        void OnEnable() => Apply();

        void Update()
        {
            // Editor / other scripts sometimes reset this — keep the thesis cap.
            if (Application.targetFrameRate != targetFps)
                Application.targetFrameRate = targetFps;
            if (QualitySettings.vSyncCount != 0)
                QualitySettings.vSyncCount = 0;
        }

        void Apply()
        {
            Application.targetFrameRate = targetFps;
            QualitySettings.vSyncCount = 0;
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
    }
}
