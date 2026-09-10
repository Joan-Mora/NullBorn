#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NullBorn.Editor
{
    /// <summary>
    /// One-click fixes for third-party import warnings (demo cleanup, Alpaca mesh).
    /// </summary>
    public static class NullBornAssetFixes
    {
        const string WaterRoot = "Assets/_ThirdParty/Water";
        const string AlpacaFbxPath = "Assets/_ThirdParty/Animals/FBX/Alpaca.fbx";
        const string AlpacaMeshAssetPath = "Assets/_ThirdParty/Animals/Meshes/Alpaca_Mesh.asset";
        const string AlpacaAnimFolder = "Assets/_ThirdParty/Animals/Animations/Alpaca";

        [MenuItem("NullBorn/Fix Asset Warnings/Clean Water Demo Artifacts")]
        public static void CleanWaterDemoArtifactsMenu()
        {
            var removed = CleanWaterDemoArtifacts();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "NullBorn — Water cleanup",
                removed > 0
                    ? $"Removed {removed} orphan demo file(s).\nUnity will refresh the asset database."
                    : "Nothing to clean — demo artifacts already removed.",
                "OK");
        }

        [MenuItem("NullBorn/Fix Asset Warnings/Upgrade Water Shader Graph Nodes (Python)")]
        public static void UpgradeWaterShaderGraphsMenu()
        {
            if (RunPythonGraphUpgrade())
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "NullBorn — Shader Graph",
                    "Python upgrade finished. Check the Console for details.",
                    "OK");
            }
        }

        [MenuItem("NullBorn/Fix Asset Warnings/Fix Alpaca Mesh (stop reimport warnings)")]
        public static void FixAlpacaMeshMenu()
        {
            if (!FixAlpacaMesh())
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — Alpaca",
                    "Could not fix Alpaca.fbx. Check the Console for details.",
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "NullBorn — Alpaca",
                "Alpaca listo:\n" +
                "• Mesh → Animals/Meshes/Alpaca_Mesh.asset\n" +
                "• Animaciones → Animals/Animations/Alpaca/\n" +
                "• FBX original archivado en Assets/Original/Animals/\n\n" +
                "Ya no debería aparecer el warning al reimportar.",
                "OK");
        }

        [MenuItem("NullBorn/Fix Asset Warnings/Run All Fixes")]
        public static void RunAllFixesMenu()
        {
            var cleaned = CleanWaterDemoArtifacts();
            var pythonOk = RunPythonGraphUpgrade();
            var alpacaFixed = FixAlpacaMesh();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "NullBorn — All fixes",
                $"Water demo cleanup: {cleaned} file(s)\n" +
                $"Shader Graph (Python): {(pythonOk ? "OK" : "skipped/failed")}\n" +
                $"Alpaca mesh: {(alpacaFixed ? "OK" : "skipped/failed")}",
                "OK");
        }

        public static int CleanWaterDemoArtifacts()
        {
            var removed = 0;
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;

            // Folder meta without content (Water/URP was archived).
            removed += DeleteIfExists(Path.Combine(projectRoot, WaterRoot, "URP.meta"));
            removed += DeleteDirectoryIfEmpty(Path.Combine(projectRoot, WaterRoot, "URP"));

            // Stale BubbleShader / URP renderer metas if Unity still has ghosts on disk.
            var stalePaths = new[]
            {
                Path.Combine(projectRoot, WaterRoot, "Shaders", "VFX", "BubbleShader.shadergraph"),
                Path.Combine(projectRoot, WaterRoot, "Shaders", "VFX", "BubbleShader.shadergraph.meta"),
                Path.Combine(projectRoot, WaterRoot, "Materials", "VFX", "BubbleShader.mat"),
                Path.Combine(projectRoot, WaterRoot, "Materials", "VFX", "BubbleShader.mat.meta"),
                Path.Combine(projectRoot, WaterRoot, "URP", "URPAsset_Renderer.asset"),
                Path.Combine(projectRoot, WaterRoot, "URP", "URPAsset_Renderer.asset.meta"),
                Path.Combine(projectRoot, WaterRoot, "URP", "URPAsset.asset"),
                Path.Combine(projectRoot, WaterRoot, "URP", "URPAsset.asset.meta"),
                Path.Combine(projectRoot, WaterRoot, "URP", "PostProcessing.asset"),
                Path.Combine(projectRoot, WaterRoot, "URP", "PostProcessing.asset.meta"),
            };

            foreach (var path in stalePaths)
                removed += DeleteIfExists(path);

            return removed;
        }

        static int DeleteIfExists(string path)
        {
            if (!File.Exists(path))
                return 0;
            File.Delete(path);
            Debug.Log($"[NullBorn] Deleted {path}");
            return 1;
        }

        static int DeleteDirectoryIfEmpty(string path)
        {
            if (!Directory.Exists(path))
                return 0;
            if (Directory.EnumerateFileSystemEntries(path).Any())
                return 0;
            Directory.Delete(path);
            Debug.Log($"[NullBorn] Deleted empty folder {path}");
            return 1;
        }

        static bool RunPythonGraphUpgrade()
        {
            var repoRoot = Directory.GetParent(Application.dataPath)!.Parent!.FullName;
            var script = Path.Combine(repoRoot, "tools", "upgrade_shader_graph_nodes.py");
            var waterPath = Path.Combine(Application.dataPath, "_ThirdParty", "Water");

            if (!File.Exists(script))
            {
                Debug.LogWarning($"[NullBorn] Python script not found: {script}");
                return false;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{script}\" \"{waterPath}\"",
                    WorkingDirectory = repoRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(psi);
                if (process == null)
                    return false;

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdout))
                    Debug.Log($"[NullBorn] {stdout.Trim()}");
                if (!string.IsNullOrWhiteSpace(stderr))
                    Debug.LogWarning($"[NullBorn] {stderr.Trim()}");

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NullBorn] Failed to run Python upgrade: {ex.Message}");
                return false;
            }
        }

        public static bool FixAlpacaMesh()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AlpacaFbxPath) == null)
            {
                Debug.LogWarning("[NullBorn] Alpaca.fbx not found — already fixed?");
                return false;
            }

            var sourceAssets = AssetDatabase.LoadAllAssetsAtPath(AlpacaFbxPath);
            var mesh = sourceAssets.OfType<Mesh>().FirstOrDefault(m => m.name == "Alpaca")
                ?? sourceAssets.OfType<Mesh>().FirstOrDefault();
            if (mesh == null)
            {
                Debug.LogError("[NullBorn] No mesh found in Alpaca.fbx");
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(AlpacaMeshAssetPath)!);
            var baked = UnityEngine.Object.Instantiate(mesh);
            baked.name = "Alpaca_Mesh";

            var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(AlpacaMeshAssetPath);
            if (existingMesh != null)
                AssetDatabase.DeleteAsset(AlpacaMeshAssetPath);

            AssetDatabase.CreateAsset(baked, AlpacaMeshAssetPath);

            var animDir = Path.Combine(Application.dataPath, "_ThirdParty", "Animals", "Animations", "Alpaca");
            Directory.CreateDirectory(animDir);
            var usedNames = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var clip in sourceAssets.OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")))
            {
                var safeName = SanitizeAssetFileName(clip.name);
                var uniqueName = safeName;
                var suffix = 1;
                while (!usedNames.Add(uniqueName))
                    uniqueName = $"{safeName}_{suffix++}";

                var animPath = $"{AlpacaAnimFolder}/{uniqueName}.anim";
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath) != null)
                    AssetDatabase.DeleteAsset(animPath);

                var copy = UnityEngine.Object.Instantiate(clip);
                copy.name = uniqueName;
                AssetDatabase.CreateAsset(copy, animPath);
            }

            var absFbx = Path.GetFullPath(Path.Combine(Application.dataPath, "_ThirdParty", "Animals", "FBX", "Alpaca.fbx"));
            var repoRoot = Directory.GetParent(Application.dataPath)!.Parent!.FullName;
            var archiveDir = Path.Combine(repoRoot, "Assets", "Original", "Animals");
            Directory.CreateDirectory(archiveDir);
            var archivePath = Path.Combine(archiveDir, "Alpaca.fbx");

            if (File.Exists(absFbx))
                File.Copy(absFbx, archivePath, true);

            AssetDatabase.DeleteAsset(AlpacaFbxPath);
            AssetDatabase.Refresh();
            Debug.Log($"[NullBorn] Alpaca: mesh → {AlpacaMeshAssetPath}, anims → {AlpacaAnimFolder}/, source archived → {archivePath}");
            return true;
        }

        /// <summary>FBX clip names often contain '|' (e.g. AnimalArmature|Walk) — invalid in asset paths.</summary>
        static string SanitizeAssetFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (invalid.Contains(chars[i]) || chars[i] is '|' or ':')
                    chars[i] = '_';
            }
            var sanitized = new string(chars).Trim('_', ' ');
            return string.IsNullOrEmpty(sanitized) ? "Animation" : sanitized;
        }
    }
}
#endif
