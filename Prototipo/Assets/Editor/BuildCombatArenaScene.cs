using NullBorn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.EditorTools
{
    public static class BuildCombatArenaScene
    {
        private const string ScenePath = "Assets/Scenes/Combat_Arena.unity";

        [MenuItem("NullBorn/Build Combat Arena")]
        public static void BuildFromMenu() => Build(true);

        public static void BuildBatch() => Build(false);

        public static void Build(bool showDialog)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CombatArenaBuilder.Build();
            EnsureFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuild(ScenePath, false);
            AssetDatabase.SaveAssets();
            Debug.Log("Combat_Arena generado.");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("NullBorn", "Escena Combat_Arena lista.\nAbre Assets/Scenes/Combat_Arena y pulsa Play.", "OK");
            }
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }

        private static void AddToBuild(string path, bool enabled)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == path)) return;
            list.Add(new EditorBuildSettingsScene(path, enabled));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
