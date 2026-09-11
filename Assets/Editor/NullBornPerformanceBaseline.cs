#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NullBorn.Editor
{
    /// <summary>
    /// Locks URP + QualitySettings to the NullBorn thesis baseline:
    /// stable ~60 FPS target on mid PC, with readable stylized quality.
    /// </summary>
    public static class NullBornPerformanceBaseline
    {
        const string UrpAssetPath = "Assets/Settings/URP_Pipeline.asset";

        [MenuItem("NullBorn/Performance/Apply Baseline (Quality + URP)")]
        public static void ApplyFromMenu()
        {
            Apply(showDialog: true);
        }

        [MenuItem("NullBorn/Performance/Log Current Stats Hint")]
        public static void LogStatsHint()
        {
            Debug.Log(
                "[NullBorn] En Play: Game view → Stats.\n" +
                "Meta tesis: ≥55 FPS estables, Batches < 150 en ArtTest, Shadow casters pocos.\n" +
                "Si GPU bound: baja MSAA/sombras. Si CPU bound: reduce draw calls / Animator.");
        }

        public static void Apply(bool showDialog)
        {
            ApplyUrpBaseline();
            ApplyQualityBaseline();
            AssetDatabase.SaveAssets();

            Debug.Log("[NullBorn] Performance baseline applied (URP + Quality Ultra retargeted).");

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — Performance Baseline",
                    "Aplicado:\n" +
                    "• Sombras: 40 m, 2 cascadas, soft medium, map 2048\n" +
                    "• MSAA 2× · HDR off · luces adicionales máx 2/objeto\n" +
                    "• Quality Ultra alineado a PERFORMANCE.md\n" +
                    "• VSync off en editor para medir FPS real\n\n" +
                    "Luego: NullBorn → Build Art Test Scene y mira Stats en Play.",
                    "OK");
            }
        }

        static void ApplyUrpBaseline()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (urp == null)
            {
                Debug.LogError("[NullBorn] Missing URP asset at " + UrpAssetPath);
                return;
            }

            var so = new SerializedObject(urp);

            Set(so, "m_MSAA", 2);                         // 2x — PERFORMANCE.md
            Set(so, "m_RenderScale", 1f);
            Set(so, "m_SupportsHDR", false);
            Set(so, "m_MainLightShadowmapResolution", 2048);
            Set(so, "m_ShadowDistance", 40f);
            Set(so, "m_ShadowCascadeCount", 2);
            Set(so, "m_Cascade2Split", 0.2f);
            Set(so, "m_SoftShadowsSupported", true);
            Set(so, "m_SoftShadowQuality", 2);              // Medium
            Set(so, "m_AdditionalLightsPerObjectLimit", 2);
            Set(so, "m_AdditionalLightShadowsSupported", false);
            Set(so, "m_SupportsDynamicBatching", true);
            Set(so, "m_UseSRPBatcher", true);
            Set(so, "m_ShadowDepthBias", 1f);
            Set(so, "m_ShadowNormalBias", 1f);
            Set(so, "m_PrefilterSoftShadows", false);
            Set(so, "m_PrefilterSoftShadowsQualityMedium", false);
            Set(so, "m_PrefilterSoftShadowsQualityLow", true);
            Set(so, "m_PrefilterSoftShadowsQualityHigh", true);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(urp);
        }

        static void ApplyQualityBaseline()
        {
            // Prefer "Ultra" if present; otherwise current level.
            var names = QualitySettings.names;
            var ultraIndex = -1;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == "Ultra")
                {
                    ultraIndex = i;
                    break;
                }
            }

            if (ultraIndex >= 0)
                QualitySettings.SetQualityLevel(ultraIndex, applyExpensiveChanges: true);

            // Align builtins that still matter beside URP.
            QualitySettings.shadowDistance = 40f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
            QualitySettings.antiAliasing = 2;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.lodBias = 1f;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.vSyncCount = 0;
            QualitySettings.softParticles = false;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.skinWeights = SkinWeights.TwoBones;
            QualitySettings.particleRaycastBudget = 64;

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (urp != null)
                QualitySettings.renderPipeline = urp;
        }

        static void Set(SerializedObject so, string prop, int value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.intValue = value;
            else Debug.LogWarning("[NullBorn] URP property missing: " + prop);
        }

        static void Set(SerializedObject so, string prop, float value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.floatValue = value;
            else Debug.LogWarning("[NullBorn] URP property missing: " + prop);
        }

        static void Set(SerializedObject so, string prop, bool value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.boolValue = value;
            else Debug.LogWarning("[NullBorn] URP property missing: " + prop);
        }
    }
}
#endif
