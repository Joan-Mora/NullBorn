using NullBorn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.EditorTools
{
    public static class BuildWorldMainScene
    {
        private const string ScenePath = "Assets/Scenes/World_Main.unity";

        [MenuItem("NullBorn/Build World Main")]
        public static void BuildFromMenu() => Build(true);

        public static void BuildBatch() => Build(false);

        public static void Build(bool showDialog)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            WorldLayoutBuilder.Build();
            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets/Scenes", "Dungeons");
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets/Data", "Quests");
            EnsureFolder("Assets/Data", "NPCs");
            EnsureFolder("Assets/Data", "Items");
            EnsureFolder("Assets/Data", "Regions");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuild(ScenePath, true);
            AssetDatabase.SaveAssets();
            Debug.Log("World_Main generado.");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("NullBorn",
                    "Escena World_Main regenerada limpia (sin WorldRoot duplicado; un solo terreno, sin discos de bioma).\nAbre Assets/Scenes/World_Main y pulsa Play.",
                    "OK");
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
