using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NullBorn.Bootstrap;

namespace NullBorn.EditorTools
{
    /// <summary>
    /// Menú de Unity: NullBorn → Crear / Abrir escena del prototipo.
    /// Genera Assets/Scenes/Prototype.unity con el Bootstrap listo.
    /// </summary>
    public static class CreatePrototypeScene
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        [MenuItem("NullBorn/Crear escena Prototype")]
        public static void CreateSceneMenu()
        {
            CreateScene(showDialog: true);
        }

        /// <summary>
        /// También se puede llamar en batch:
        /// Unity.exe -batchmode -projectPath ... -executeMethod NullBorn.EditorTools.CreatePrototypeScene.CreateSceneBatch -quit
        /// </summary>
        public static void CreateSceneBatch()
        {
            CreateScene(showDialog: false);
        }

        public static void CreateScene(bool showDialog)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject bootstrap = new GameObject("NullBorn_Bootstrap");
            bootstrap.AddComponent<PrototypeBootstrap>();

            // Cámara placeholder (el bootstrap la reutiliza en Play).
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, 4f, -11f);

            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Escena Prototype creada en Assets/Scenes/Prototype.unity. Pulsa Play.");
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn",
                    "Escena Prototype lista.\n\n1) Ábrela si no está abierta\n2) Pulsa el botón Play ►\n3) Click en el Game view para capturar el mouse",
                    "Entendido");
            }
        }
    }
}
