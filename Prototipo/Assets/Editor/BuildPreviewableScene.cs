using NullBorn.Bootstrap;
using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.Player;
using NullBorn.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NullBorn.EditorTools
{
    /// <summary>
    /// Coloca el escenario real en la escena para verlo SIN pulsar Play.
    /// Menú: NullBorn → Generar escenario previsualizable
    /// </summary>
    public static class BuildPreviewableScene
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";
        private const string MaterialsFolder = "Assets/Materials";

        [MenuItem("NullBorn/Generar escenario previsualizable")]
        public static void BuildFromMenu()
        {
            Build(showDialog: true);
        }

        public static void BuildBatch()
        {
            Build(showDialog: false);
        }

        public static void Build(bool showDialog)
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClearGeneratedContent();

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.4f, 0.5f);

            // Luz
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.9f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Suelo y pilares
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            AssignMaterial(ground, "Mat_Ground", new Color(0.12f, 0.14f, 0.18f));

            CreateBox("Pillar_A", new Vector3(-4f, 1f, 3f), new Vector3(1f, 2f, 1f), "Mat_Pillar", new Color(0.2f, 0.25f, 0.32f));
            CreateBox("Pillar_B", new Vector3(5f, 1.25f, -2f), new Vector3(1.5f, 2.5f, 1.5f), "Mat_Pillar", new Color(0.18f, 0.22f, 0.28f));
            CreateBox("Pillar_C", new Vector3(-1f, 0.75f, -5f), new Vector3(2f, 1.5f, 1f), "Mat_PillarB", new Color(0.22f, 0.2f, 0.3f));

            // Sistemas
            GameObject systems = new GameObject("GameSystems");
            systems.AddComponent<GameSession>();
            systems.AddComponent<PrototypeHUD>();

            // Jugador
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_Thread";
            player.transform.position = new Vector3(0f, 1f, -6f);
            AssignMaterial(player, "Mat_Player", new Color(0.25f, 0.75f, 1f));
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = Vector3.zero;

            player.AddComponent<SystemIntegrity>();
            player.AddComponent<CpuCycles>();
            player.AddComponent<ThirdPersonController>();
            player.AddComponent<GarbageCollectionAbility>();
            player.AddComponent<DebugStrikeAbility>();

            // Enemigo
            GameObject leakGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leakGo.name = "Enemy_MemoryLeak";
            leakGo.transform.position = new Vector3(0f, 1.2f, 4f);
            leakGo.transform.localScale = Vector3.one * 1.8f;
            AssignMaterial(leakGo, "Mat_MemoryLeak", new Color(0.9f, 0.15f, 0.35f));
            leakGo.GetComponent<SphereCollider>().radius = 0.55f;
            leakGo.AddComponent<MemoryLeakEnemy>();

            // Cámara
            Camera mainCam = Object.FindAnyObjectByType<Camera>();
            if (mainCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            mainCam.transform.position = new Vector3(0f, 4f, -11f);
            mainCam.transform.LookAt(new Vector3(0f, 1f, 0f));
            if (mainCam.GetComponent<ThirdPersonCamera>() == null)
            {
                mainCam.gameObject.AddComponent<ThirdPersonCamera>();
            }

            // Bootstrap (solo enlaza en Play + cursor/reinicio)
            GameObject bootstrap = GameObject.Find("NullBorn_Bootstrap");
            if (bootstrap == null)
            {
                bootstrap = new GameObject("NullBorn_Bootstrap");
            }

            if (bootstrap.GetComponent<PrototypeBootstrap>() == null)
            {
                bootstrap.AddComponent<PrototypeBootstrap>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Enfoca la Scene View al escenario
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(new Vector3(0f, 1f, 0f), Quaternion.Euler(25f, -20f, 0f), 12f);
            }

            Debug.Log("Escenario previsualizable generado. Ya puedes verlo en Scene View sin Play.");

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn",
                    "Escenario guardado en la escena.\n\n" +
                    "Mira la ventana Scene (no hace falta Play).\n" +
                    "Cuando quieras jugar, pulsa Play ►",
                    "Perfecto");
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!FileExists(ScenePath))
            {
                // Si no existe la escena, créala vacía primero.
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
        }

        private static bool FileExists(string assetPath)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath));
        }

        private static void ClearGeneratedContent()
        {
            string[] names =
            {
                "Directional Light", "Ground", "Pillar_A", "Pillar_B", "Pillar_C",
                "GameSystems", "Player_Thread", "Enemy_MemoryLeak"
            };

            foreach (string objectName in names)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, string matName, Color color)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            AssignMaterial(box, matName, color);
            return box;
        }

        private static void AssignMaterial(GameObject go, string materialName, Color color)
        {
            string path = $"{MaterialsFolder}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader) { name = materialName };
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                mat.color = color;
                EditorUtility.SetDirty(mat);
            }

            go.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
