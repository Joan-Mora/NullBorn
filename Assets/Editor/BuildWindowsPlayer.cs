#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NullBorn.Editor
{
    public static class BuildWindowsPlayer
    {
        const string ScenePath = "Assets/Scenes/ArtTest.unity";
        const string OutputPath = "Builds/Windows/NullBorn.exe";

        [MenuItem("NullBorn/Build Windows Player")]
        public static void BuildFromMenu()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "NullBorn",
                    "No está Assets/Scenes/ArtTest.unity.\nPrimero: NullBorn → Build Art Test Scene.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[NullBorn] Windows player → " + Path.GetFullPath(OutputPath));
                EditorUtility.RevealInFinder(OutputPath);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "NullBorn",
                    "Build falló: " + report.summary.result + "\nMira la Console.",
                    "OK");
            }
        }
    }
}
#endif
