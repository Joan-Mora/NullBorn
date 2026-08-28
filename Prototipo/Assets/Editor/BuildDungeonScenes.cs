using NullBorn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.EditorTools
{
    public static class BuildDungeonScenes
    {
        [MenuItem("NullBorn/Build Dungeon Scenes")]
        public static void BuildFromMenu()
        {
            BuildOne("Assets/Scenes/Dungeons/Dungeon_HeapCave.unity", "Dungeon_HeapCave");
            BuildOne("Assets/Scenes/Dungeons/Dungeon_StackChasm.unity", "Dungeon_StackChasm");
            EditorUtility.DisplayDialog("NullBorn", "Dungeons Heap Cave y Stack Chasm generados.", "OK");
        }

        public static void BuildBatch()
        {
            BuildOne("Assets/Scenes/Dungeons/Dungeon_HeapCave.unity", "Dungeon_HeapCave");
            BuildOne("Assets/Scenes/Dungeons/Dungeon_StackChasm.unity", "Dungeon_StackChasm");
        }

        private static void BuildOne(string path, string sceneName)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/Dungeons")) AssetDatabase.CreateFolder("Assets/Scenes", "Dungeons");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            DungeonLayoutBuilder.Build(sceneName);
            EditorSceneManager.SaveScene(scene, path);
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!list.Exists(s => s.path == path))
            {
                list.Add(new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = list.ToArray();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
