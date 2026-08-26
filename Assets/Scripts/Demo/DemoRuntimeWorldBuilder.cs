using NullBorn.Art;
using NullBorn.Bootstrap;
using NullBorn.Core;
using NullBorn.Demo;
using NullBorn.Enemies;
using NullBorn.Player;
using NullBorn.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.Demo
{
    /// <summary>
    /// Si la escena está vacía (solo este objeto), construye el mini-demo al entrar en Play.
    /// Preferible: NullBorn → Generar Demo 10min para previsualizar en Scene View.
    /// </summary>
    public class DemoRuntimeWorldBuilder : MonoBehaviour
    {
        [SerializeField] private bool buildOnAwake = true;

        private void Awake()
        {
            if (!buildOnAwake) return;
            if (GameObject.Find("Player_Thread") != null) return;
            Build();
        }

        [ContextMenu("Build Demo World Now")]
        public void Build()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.28f, 0.32f, 0.4f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.05f, 0.07f, 0.1f);

            Material matFloorBoot = MakeMat(new Color(0.10f, 0.14f, 0.18f), new Color(0.05f, 0.25f, 0.35f) * 0.4f);
            Material matFloorHeap = MakeMat(new Color(0.14f, 0.10f, 0.14f), new Color(0.35f, 0.05f, 0.25f) * 0.5f);
            Material matFloorKernel = MakeMat(new Color(0.08f, 0.08f, 0.12f), new Color(0.4f, 0.05f, 0.5f) * 0.55f);
            Material matWall = MakeMat(new Color(0.16f, 0.18f, 0.22f), Color.black);
            Material matAccent = MakeMat(new Color(0.15f, 0.35f, 0.4f), new Color(0.2f, 0.8f, 1f) * 1.5f);
            Material matCorrupt = MakeMat(new Color(0.3f, 0.1f, 0.25f), new Color(0.9f, 0.1f, 0.6f) * 1.2f);
            Material matPlayer = MakeMat(new Color(0.2f, 0.55f, 0.75f), new Color(0.1f, 0.4f, 0.6f) * 0.3f);
            Material matCore = MakeMat(new Color(0.25f, 0.9f, 1f), new Color(0.25f, 0.9f, 1f) * 2f);
            Material matLeak = MakeMat(new Color(0.85f, 0.12f, 0.35f), new Color(0.9f, 0.1f, 0.4f) * 1.4f);
            Material matLeakBoss = MakeMat(new Color(0.55f, 0.05f, 0.7f), new Color(0.7f, 0.05f, 0.9f) * 1.8f);
            Material matProp = MakeMat(new Color(0.2f, 0.22f, 0.18f), new Color(1f, 0.45f, 0.1f) * 0.8f);

            if (Object.FindAnyObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                lightGo.transform.rotation = Quaternion.Euler(40f, -25f, 0f);
            }

            GameObject systems = GameObject.Find("GameSystems");
            if (systems == null)
            {
                systems = new GameObject("GameSystems");
                systems.AddComponent<GameSession>();
                systems.AddComponent<CheckpointSystem>();
                systems.AddComponent<TutorialDirector>();
                systems.AddComponent<DemoFlowController>();
                systems.AddComponent<CanvasHUD>();
            }

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.AddComponent<ThirdPersonCamera>();
            }

            Floor("Floor_Boot", new Vector3(0, 0, 10), new Vector3(2.2f, 1, 3.2f), matFloorBoot);
            Walls(10f, 18f, 12f, matWall, matAccent);
            Box("Prop_RegisterPanel", new Vector3(-5, 1.2f, 2), new Vector3(0.3f, 2.4f, 3), matAccent);
            Box("Prop_StackTower", new Vector3(5, 1.5f, 8), new Vector3(1.2f, 3f, 1.2f), matProp);

            Checkpoint cpBoot = MakeCheckpoint("Checkpoint_Boot", "boot", new Vector3(0, 0.1f, -2));
            MakeZone("Zone_Boot", DemoZoneId.BootSector, "boot_welcome", new Vector3(0, 1, 0), new Vector3(16, 4, 6), cpBoot);
            MakeTut("Tut_CPU", "boot_cpu", new Vector3(0, 1, 6), new Vector3(10, 3, 2));
            MakeTut("Tut_GC", "boot_gc", new Vector3(0, 1, 14), new Vector3(10, 3, 2));
            MakeTut("Tut_Debug", "boot_debug", new Vector3(0, 1, 20), new Vector3(10, 3, 2));
            MakeLeak("Leak_Basic_Boot", new Vector3(0, 1.1f, 22), LeakVariant.Basic, 70, 2.2f, 2.8f, 5, false, matLeak, 1.3f);

            Floor("Floor_Heap", new Vector3(0, 0, 50), new Vector3(2.6f, 1, 3.6f), matFloorHeap);
            Walls(50f, 22f, 14f, matWall, matCorrupt);
            Box("Portal_BootToHeap", new Vector3(0, 1.5f, 28), new Vector3(8, 3, 0.4f), matAccent);
            Checkpoint cpHeap = MakeCheckpoint("Checkpoint_Heap", "heap", new Vector3(0, 0.1f, 32));
            MakeZone("Zone_Heap", DemoZoneId.HeapCorrupt, "heap_tradeoff", new Vector3(0, 1, 34), new Vector3(18, 4, 6), cpHeap);
            MakeLeak("Leak_Basic_Heap_A", new Vector3(-4, 1.1f, 42), LeakVariant.Basic, 90, 3.2f, 2f, 8, false, matLeak, 1.4f);
            MakeLeak("Leak_Spawner_Heap", new Vector3(3, 1.2f, 52), LeakVariant.Spawner, 110, 3.8f, 1.3f, 16, false, matLeak, 1.6f);
            MakeLeak("Leak_Basic_Heap_B", new Vector3(-2, 1.1f, 62), LeakVariant.Basic, 100, 3.5f, 1.8f, 10, false, matLeak, 1.45f);

            Floor("Floor_Kernel", new Vector3(0, 0, 92), new Vector3(3f, 1, 3.2f), matFloorKernel);
            Box("Portal_HeapToKernel", new Vector3(0, 1.5f, 70), new Vector3(8, 3, 0.4f), matCorrupt);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                Vector3 p = new Vector3(0, 0, 95) + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * 10f;
                Box($"ArenaPillar_{i}", p + Vector3.up * 1.5f, new Vector3(1.2f, 3f, 1.2f), i % 2 == 0 ? matWall : matLeakBoss);
            }
            Checkpoint cpKernel = MakeCheckpoint("Checkpoint_Kernel", "kernel", new Vector3(0, 0.1f, 76));
            MakeZone("Zone_Kernel", DemoZoneId.KernelCore, "kernel_boss", new Vector3(0, 1, 78), new Vector3(20, 4, 6), cpKernel);
            MakeLeak("Leak_Boss_Kernel", new Vector3(0, 1.6f, 95), LeakVariant.Boss, 280, 5.5f, 1.6f, 18, true, matLeakBoss, 2.4f);

            GameObject player = new GameObject("Player_Thread");
            player.tag = "Player";
            player.transform.position = new Vector3(0, 1, -4);
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            player.AddComponent<SystemIntegrity>();
            player.AddComponent<CpuCycles>();
            player.AddComponent<ThirdPersonController>();
            player.AddComponent<GarbageCollectionAbility>();
            player.AddComponent<DebugStrikeAbility>();
            player.AddComponent<PlayerAnimBridge>();
            ThreadAvatarBuilder.Build(player.transform, matPlayer, matCore);

            if (systems.GetComponent<DemoBootstrap>() == null)
            {
                systems.AddComponent<DemoBootstrap>();
            }

            Debug.Log("DemoRuntimeWorldBuilder: mundo del mini-demo construido.");
        }

        private static Material MakeMat(Color color, Color emission)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }
            return mat;
        }

        private static void Floor(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void Walls(float z, float length, float width, Material wall, Material accent)
        {
            float half = width * 0.5f;
            Box($"Wall_L_{z}", new Vector3(-half, 1.5f, z), new Vector3(0.4f, 3f, length), wall);
            Box($"Wall_R_{z}", new Vector3(half, 1.5f, z), new Vector3(0.4f, 3f, length), wall);
            Box($"Strip_L_{z}", new Vector3(-half + 0.3f, 0.2f, z), new Vector3(0.15f, 0.15f, length * 0.9f), accent);
            Box($"Strip_R_{z}", new Vector3(half - 0.3f, 0.2f, z), new Vector3(0.15f, 0.15f, length * 0.9f), accent);
        }

        private static void Box(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static Checkpoint MakeCheckpoint(string name, string id, Vector3 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            Checkpoint cp = go.AddComponent<Checkpoint>();
            // zoneId via reflection-friendly public setter added below
            cp.SetZoneId(id);
            return cp;
        }

        private static void MakeZone(string name, DemoZoneId zone, string tip, Vector3 pos, Vector3 size, Checkpoint cp)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            var zt = go.AddComponent<ZoneTrigger>();
            zt.Configure(zone, tip, cp);
        }

        private static void MakeTut(string name, string tip, Vector3 pos, Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            go.AddComponent<TutorialVolume>().tipId = tip;
        }

        private static void MakeLeak(string name, Vector3 pos, LeakVariant v, float hp, float drain, float interval, int maxFrag, bool boss, Material mat, float scale)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.7f * scale;
            var leak = go.AddComponent<MemoryLeakEnemy>();
            leak.Configure(v, hp, drain, interval, maxFrag, boss);
            LeakAvatarBuilder.Build(go.transform, mat, scale);
        }
    }
}
