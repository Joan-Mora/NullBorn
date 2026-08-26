using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NullBorn.EditorTools
{
    /// <summary>
    /// Build Windows del mini-demo.
    /// Menú: NullBorn → Build Windows (.exe)
    /// </summary>
    public static class BuildWindowsPlayer
    {
        private const string ScenePath = "Assets/Scenes/Demo_10min.unity";
        private const string OutputPath = "Builds/Windows/NullBorn.exe";

        [MenuItem("NullBorn/Build Windows (.exe)")]
        public static void BuildFromMenu()
        {
            Build(false);
        }

        public static void BuildBatch()
        {
            Build(true);
        }

        private static void Build(bool batch)
        {
            if (!System.IO.File.Exists(ScenePath) && !System.IO.File.Exists(
                    System.IO.Path.Combine(Application.dataPath, "../", ScenePath)))
            {
                // AssetDatabase check
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                {
                    Debug.LogError("No existe Demo_10min.unity. Ejecuta NullBorn → Generar Demo 10min primero.");
                    if (batch) EditorApplication.Exit(1);
                    return;
                }
            }

            string dir = System.IO.Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build OK: {OutputPath} ({summary.totalSize} bytes)");
                if (!batch)
                {
                    EditorUtility.DisplayDialog("NullBorn", $"Build listo:\n{OutputPath}", "OK");
                }
                if (batch) EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build falló: {summary.result}");
                if (batch) EditorApplication.Exit(1);
            }
        }
    }
}
