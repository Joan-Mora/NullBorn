using NullBorn.Art;
using NullBorn.Bootstrap;
using NullBorn.Core;
using NullBorn.Demo;
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
    /// Genera Assets/Scenes/Demo_10min.unity: 3 zonas lineales, prefabs, materiales temáticos.
    /// Menú: NullBorn → Generar Demo 10min
    /// </summary>
    public static class BuildDemo10MinScene
    {
        private const string ScenePath = "Assets/Scenes/Demo_10min.unity";
        private const string MatFolder = "Assets/Art/Materials";
        private const string PrefabFolder = "Assets/Prefabs";

        [MenuItem("NullBorn/Generar Demo 10min")]
        public static void BuildFromMenu() => Build(true);

        public static void BuildBatch() => Build(false);

        public static void Build(bool showDialog)
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material matFloorBoot = GetOrCreateMat("Mat_Floor_Boot", new Color(0.10f, 0.14f, 0.18f), new Color(0.05f, 0.25f, 0.35f) * 0.4f);
            Material matFloorHeap = GetOrCreateMat("Mat_Floor_Heap", new Color(0.14f, 0.10f, 0.14f), new Color(0.35f, 0.05f, 0.25f) * 0.5f);
            Material matFloorKernel = GetOrCreateMat("Mat_Floor_Kernel", new Color(0.08f, 0.08f, 0.12f), new Color(0.4f, 0.05f, 0.5f) * 0.55f);
            Material matWall = GetOrCreateMat("Mat_Wall", new Color(0.16f, 0.18f, 0.22f), Color.black);
            Material matAccent = GetOrCreateMat("Mat_AccentCyan", new Color(0.15f, 0.35f, 0.4f), new Color(0.2f, 0.8f, 1f) * 1.5f);
            Material matCorrupt = GetOrCreateMat("Mat_AccentMagenta", new Color(0.3f, 0.1f, 0.25f), new Color(0.9f, 0.1f, 0.6f) * 1.2f);
            Material matPlayer = GetOrCreateMat("Mat_PlayerBody", new Color(0.2f, 0.55f, 0.75f), new Color(0.1f, 0.4f, 0.6f) * 0.3f);
            Material matCore = GetOrCreateMat("Mat_PlayerCore", new Color(0.25f, 0.9f, 1f), new Color(0.25f, 0.9f, 1f) * 2f);
            Material matLeak = GetOrCreateMat("Mat_Leak", new Color(0.85f, 0.12f, 0.35f), new Color(0.9f, 0.1f, 0.4f) * 1.4f);
            Material matLeakBoss = GetOrCreateMat("Mat_LeakBoss", new Color(0.55f, 0.05f, 0.7f), new Color(0.7f, 0.05f, 0.9f) * 1.8f);
            Material matProp = GetOrCreateMat("Mat_PropRam", new Color(0.2f, 0.22f, 0.18f), new Color(1f, 0.45f, 0.1f) * 0.8f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.36f, 0.44f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.009f;
            RenderSettings.fogColor = new Color(0.05f, 0.07f, 0.1f);

            // Light
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.85f, 0.92f, 1f);
            light.intensity = 1.15f;
            lightGo.transform.rotation = Quaternion.Euler(40f, -25f, 0f);

            // Systems
            GameObject systems = new GameObject("GameSystems");
            systems.AddComponent<GameSession>();
            systems.AddComponent<CheckpointSystem>();
            systems.AddComponent<TutorialDirector>();
            systems.AddComponent<DemoFlowController>();
            systems.AddComponent<CanvasHUD>();
            systems.AddComponent<DemoBootstrap>();

            // Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<ThirdPersonCamera>();
            camGo.transform.position = new Vector3(0f, 5f, -14f);

            // ---- ZONE 1 Boot Sector (z -5 .. 25) ----
            BuildFloor("Floor_Boot", new Vector3(0f, 0f, 10f), new Vector3(2.2f, 1f, 3.2f), matFloorBoot);
            BuildCorridorWalls(0f, 10f, 18f, 12f, matWall, matAccent);
            CreateHeroProp("Prop_RegisterPanel", new Vector3(-5f, 1.2f, 2f), new Vector3(0.3f, 2.4f, 3f), matAccent);
            CreateHeroProp("Prop_StackTower", new Vector3(5f, 1.5f, 8f), new Vector3(1.2f, 3f, 1.2f), matProp);
            CreateHeroProp("Prop_RamCell_A", new Vector3(-3f, 0.4f, 14f), new Vector3(1f, 0.8f, 0.5f), matProp);
            CreateZoneSign("BOOT SECTOR", "Tutorial · Integridad · CPU", new Vector3(-7f, 2.2f, 4f), new Color(0.2f, 0.85f, 1f));
            CreateZoneSign("BOOT SECTOR", "Inicio del Thread", new Vector3(7f, 2.2f, 4f), new Color(0.2f, 0.85f, 1f));

            GameObject cpBoot = CreateCheckpoint("Checkpoint_Boot", "boot", new Vector3(0f, 0.1f, -2f));
            CreateZoneTrigger("Zone_Boot", DemoZoneId.BootSector, "boot_welcome",
                new Vector3(0f, 1f, 0f), new Vector3(16f, 4f, 6f), cpBoot.GetComponent<Checkpoint>(), null);

            // Soft gate tip triggers
            CreateTutorialVolume("Tut_CPU", "boot_cpu", new Vector3(0f, 1f, 6f), new Vector3(10f, 3f, 2f));
            CreateTutorialVolume("Tut_GC", "boot_gc", new Vector3(0f, 1f, 14f), new Vector3(10f, 3f, 2f));
            CreateTutorialVolume("Tut_Debug", "boot_debug", new Vector3(0f, 1f, 20f), new Vector3(10f, 3f, 2f));

            GameObject leak1 = CreateLeakEnemy("Leak_Basic_Boot", new Vector3(0f, 1.1f, 22f), LeakVariant.Basic, 70f, 2.2f, 2.8f, 5, false, matLeak, 1.3f);
            SavePrefab(leak1, "Leak_Basic");

            // ---- ZONE 2 Heap (z 30 .. 70) ----
            BuildFloor("Floor_Heap", new Vector3(0f, 0f, 50f), new Vector3(2.6f, 1f, 3.6f), matFloorHeap);
            BuildCorridorWalls(0f, 50f, 22f, 14f, matWall, matCorrupt);
            CreateHeroProp("Prop_RamCell_B", new Vector3(-6f, 0.5f, 40f), new Vector3(1.4f, 1f, 0.6f), matProp);
            CreateHeroProp("Prop_CorruptBus", new Vector3(6f, 1f, 55f), new Vector3(0.4f, 2f, 6f), matCorrupt);
            CreateScatteredDebris(new Vector3(0f, 0.3f, 48f), 8, matProp);
            CreateZoneSign("HEAP CORRUPTO", "Tradeoff CPU vs GC", new Vector3(-8f, 2.4f, 44f), new Color(0.95f, 0.2f, 0.65f));
            CreateZoneSign("HEAP CORRUPTO", "Fugas múltiples", new Vector3(8f, 2.4f, 56f), new Color(0.95f, 0.2f, 0.65f));

            GameObject heapGroup = new GameObject("Spawns_Heap");
            heapGroup.transform.position = Vector3.zero;
            // Start disabled until zone enter — keep active for greybox reliability; zone enables extras
            GameObject leak2a = CreateLeakEnemy("Leak_Basic_Heap_A", new Vector3(-4f, 1.1f, 42f), LeakVariant.Basic, 90f, 3.2f, 2.0f, 8, false, matLeak, 1.4f);
            GameObject leak2b = CreateLeakEnemy("Leak_Spawner_Heap", new Vector3(3f, 1.2f, 52f), LeakVariant.Spawner, 110f, 3.8f, 1.3f, 16, false, matLeak, 1.6f);
            GameObject leak2c = CreateLeakEnemy("Leak_Basic_Heap_B", new Vector3(-2f, 1.1f, 62f), LeakVariant.Basic, 100f, 3.5f, 1.8f, 10, false, matLeak, 1.45f);
            leak2a.transform.SetParent(heapGroup.transform);
            leak2b.transform.SetParent(heapGroup.transform);
            leak2c.transform.SetParent(heapGroup.transform);
            SavePrefab(leak2b, "Leak_Spawner");

            GameObject cpHeap = CreateCheckpoint("Checkpoint_Heap", "heap", new Vector3(0f, 0.1f, 32f));
            CreateZoneTrigger("Zone_Heap", DemoZoneId.HeapCorrupt, "heap_tradeoff",
                new Vector3(0f, 1f, 34f), new Vector3(18f, 4f, 6f), cpHeap.GetComponent<Checkpoint>(), new[] { heapGroup });

            // Portal visual between zones
            CreateHeroProp("Portal_BootToHeap", new Vector3(0f, 1.5f, 28f), new Vector3(8f, 3f, 0.4f), matAccent);
            CreateHeroProp("Portal_HeapToKernel", new Vector3(0f, 1.5f, 70f), new Vector3(8f, 3f, 0.4f), matCorrupt);

            // ---- ZONE 3 Kernel (z 75 .. 110) ----
            BuildFloor("Floor_Kernel", new Vector3(0f, 0f, 92f), new Vector3(3f, 1f, 3.2f), matFloorKernel);
            BuildArenaRing(new Vector3(0f, 0f, 95f), 10f, matWall, matLeakBoss);
            CreateHeroProp("Prop_KernelConsole", new Vector3(0f, 1.2f, 105f), new Vector3(3f, 2.4f, 0.5f), matAccent);
            CreateHeroProp("Prop_BusAntenna", new Vector3(-8f, 2f, 95f), new Vector3(0.5f, 4f, 0.5f), matCorrupt);
            CreateZoneSign("KERNEL CORE", "Leak Boss · ENRAGE", new Vector3(0f, 3.2f, 82f), new Color(0.75f, 0.15f, 1f));

            GameObject boss = CreateLeakEnemy("Leak_Boss_Kernel", new Vector3(0f, 1.6f, 95f), LeakVariant.Boss, 280f, 5.5f, 1.6f, 18, true, matLeakBoss, 2.4f);
            SavePrefab(boss, "Leak_Boss");

            GameObject cpKernel = CreateCheckpoint("Checkpoint_Kernel", "kernel", new Vector3(0f, 0.1f, 76f));
            CreateZoneTrigger("Zone_Kernel", DemoZoneId.KernelCore, "kernel_boss",
                new Vector3(0f, 1f, 78f), new Vector3(20f, 4f, 6f), cpKernel.GetComponent<Checkpoint>(), null);

            // Player
            GameObject player = CreatePlayer(new Vector3(0f, 1f, -4f), matPlayer, matCore);
            SavePrefab(player, "Player_Thread");

            // Ensure Player tag exists
            EnsureTag("Player");
            player.tag = "Player";

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/Prototype.unity", false)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(new Vector3(0f, 2f, 40f), Quaternion.Euler(25f, -15f, 0f), 45f);
            }

            Debug.Log("Demo_10min generado. Abre Assets/Scenes/Demo_10min y pulsa Play.");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("NullBorn",
                    "Escena Demo_10min lista.\n\n3 zonas lineales + tutorial + boss.\nAbre la escena y pulsa Play.",
                    "OK");
            }
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "Scenes");
            CreateFolder("Assets", "Art");
            CreateFolder("Assets/Art", "Materials");
            CreateFolder("Assets/Art", "Characters");
            CreateFolder("Assets/Art", "Environment");
            CreateFolder("Assets/Art", "VFX");
            CreateFolder("Assets", "Prefabs");
            CreateFolder("Assets/Prefabs", "Characters");
            CreateFolder("Assets/Prefabs", "Enemies");
        }

        private static void CreateFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }

        private static Material GetOrCreateMat(string name, Color color, Color emission)
        {
            string path = $"{MatFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void BuildFloor(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = name;
            floor.transform.position = pos;
            floor.transform.localScale = scale;
            floor.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void BuildCorridorWalls(float x, float zCenter, float length, float width, Material wall, Material accent)
        {
            float halfW = width * 0.5f;
            CreateBox($"Wall_L_{zCenter}", new Vector3(x - halfW, 1.5f, zCenter), new Vector3(0.4f, 3f, length), wall);
            CreateBox($"Wall_R_{zCenter}", new Vector3(x + halfW, 1.5f, zCenter), new Vector3(0.4f, 3f, length), wall);
            // Accent strips
            CreateBox($"Strip_L_{zCenter}", new Vector3(x - halfW + 0.3f, 0.2f, zCenter), new Vector3(0.15f, 0.15f, length * 0.9f), accent);
            CreateBox($"Strip_R_{zCenter}", new Vector3(x + halfW - 0.3f, 0.2f, zCenter), new Vector3(0.15f, 0.15f, length * 0.9f), accent);
        }

        private static void BuildArenaRing(Vector3 center, float radius, Material wall, Material accent)
        {
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                Vector3 p = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
                CreateBox($"ArenaPillar_{i}", p + Vector3.up * 1.5f, new Vector3(1.2f, 3f, 1.2f), i % 2 == 0 ? wall : accent);
            }
        }

        private static void CreateScatteredDebris(Vector3 center, int count, Material mat)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 p = center + new Vector3(Random.Range(-6f, 6f), 0.25f, Random.Range(-8f, 8f));
                CreateBox($"Debris_{i}", p, Vector3.one * Random.Range(0.3f, 0.7f), mat);
            }
        }

        private static GameObject CreateBox(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = pos;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = mat;
            return box;
        }

        private static void CreateHeroProp(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            CreateBox(name, pos, scale, mat);
        }

        private static void CreateZoneSign(string title, string subtitle, Vector3 pos, Color accent)
        {
            WorldZoneSign.Create(title, subtitle, pos, accent);
        }

        private static GameObject CreateCheckpoint(string name, string zoneId, Vector3 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            Checkpoint cp = go.AddComponent<Checkpoint>();
            SerializedObject so = new SerializedObject(cp);
            so.FindProperty("zoneId").stringValue = zoneId;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static void CreateZoneTrigger(string name, DemoZoneId zone, string tutorialId, Vector3 pos, Vector3 size, Checkpoint cp, GameObject[] enable)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            ZoneTrigger zt = go.AddComponent<ZoneTrigger>();
            SerializedObject so = new SerializedObject(zt);
            so.FindProperty("zoneId").intValue = (int)zone;
            so.FindProperty("tutorialId").stringValue = tutorialId;
            so.FindProperty("checkpoint").objectReferenceValue = cp;
            if (enable != null)
            {
                SerializedProperty arr = so.FindProperty("enableOnEnter");
                arr.arraySize = enable.Length;
                for (int i = 0; i < enable.Length; i++)
                {
                    arr.GetArrayElementAtIndex(i).objectReferenceValue = enable[i];
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateTutorialVolume(string name, string tipId, Vector3 pos, Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            go.AddComponent<TutorialVolume>().tipId = tipId;
        }

        private static GameObject CreatePlayer(Vector3 pos, Material body, Material core)
        {
            GameObject player = new GameObject("Player_Thread");
            player.transform.position = pos;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0f, 0f);

            player.AddComponent<SystemIntegrity>();
            player.AddComponent<CpuCycles>();
            player.AddComponent<ThirdPersonController>();
            player.AddComponent<GarbageCollectionAbility>();
            player.AddComponent<DebugStrikeAbility>();

            ThreadAvatarBuilder.Build(player.transform, body, core);
            return player;
        }

        private static GameObject CreateLeakEnemy(string name, Vector3 pos, LeakVariant variant, float hp, float drain, float fragInterval, int maxFrag, bool bossVictory, Material mat, float visualScale)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            SphereCollider col = go.AddComponent<SphereCollider>();
            col.radius = 0.7f * visualScale;
            col.isTrigger = false;

            MemoryLeakEnemy leak = go.AddComponent<MemoryLeakEnemy>();
            leak.Configure(variant, hp, drain, fragInterval, maxFrag, bossVictory);

            LeakAvatarBuilder.Build(go.transform, mat, visualScale);
            return go;
        }

        private static void SavePrefab(GameObject go, string prefabName)
        {
            string folder = prefabName.StartsWith("Player") ? $"{PrefabFolder}/Characters" : $"{PrefabFolder}/Enemies";
            string path = $"{folder}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
        }

        private static void EnsureTag(string tag)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;
            SerializedObject so = new SerializedObject(assets[0]);
            SerializedProperty tags = so.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            }
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
        }
    }
}
