using NullBorn.Art;
using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.NPC;
using NullBorn.Player;
using NullBorn.RPG.Dialogue;
using NullBorn.RPG.Economy;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.World
{
    /// <summary>
    /// Greybox procedural del overworld: hub + 3 biomas + Kernel Heights.
    /// Sin Asset Store; primitivas + materiales URP.
    /// </summary>
    public static class WorldLayoutBuilder
    {
        public static void Build()
        {
            ApplySky();

            Material matHub = RuntimeMaterials.Lit(new Color(0.12f, 0.16f, 0.2f), new Color(0.1f, 0.4f, 0.55f) * 0.35f);
            Material matHeap = RuntimeMaterials.Lit(new Color(0.16f, 0.1f, 0.14f), new Color(0.4f, 0.05f, 0.25f) * 0.4f);
            Material matStack = RuntimeMaterials.Lit(new Color(0.1f, 0.14f, 0.18f), new Color(0.15f, 0.35f, 0.5f) * 0.4f);
            Material matIo = RuntimeMaterials.Lit(new Color(0.18f, 0.15f, 0.1f), new Color(0.55f, 0.35f, 0.08f) * 0.35f);
            Material matKernel = RuntimeMaterials.Lit(new Color(0.08f, 0.08f, 0.12f), new Color(0.4f, 0.05f, 0.5f) * 0.5f);
            Material matWall = RuntimeMaterials.Lit(new Color(0.16f, 0.18f, 0.22f), Color.black);
            Material matCyan = RuntimeMaterials.Lit(new Color(0.15f, 0.35f, 0.4f), new Color(0.2f, 0.8f, 1f) * 1.4f);
            Material matMag = RuntimeMaterials.Lit(new Color(0.3f, 0.1f, 0.25f), new Color(0.9f, 0.1f, 0.6f) * 1.1f);
            Material matPlayer = RuntimeMaterials.Lit(new Color(0.2f, 0.55f, 0.75f), new Color(0.1f, 0.4f, 0.6f) * 0.3f);
            Material matCore = RuntimeMaterials.Lit(new Color(0.25f, 0.9f, 1f), new Color(0.25f, 0.9f, 1f) * 2f);
            Material matLeak = RuntimeMaterials.Lit(new Color(0.85f, 0.12f, 0.35f), new Color(0.9f, 0.1f, 0.4f) * 1.4f);
            Material matBoss = RuntimeMaterials.Lit(new Color(0.55f, 0.05f, 0.7f), new Color(0.7f, 0.05f, 0.9f) * 1.8f);
            Material matNpc = RuntimeMaterials.Lit(new Color(0.35f, 0.7f, 0.75f), new Color(0.2f, 0.6f, 0.7f) * 0.6f);
            Material matShop = RuntimeMaterials.Lit(new Color(0.75f, 0.55f, 0.2f), new Color(1f, 0.7f, 0.2f) * 0.8f);

            GameObject root = new GameObject("WorldRoot");

            EnsureLight();
            EnsureCamera();
            GameObject systems = EnsureSystems();

            // Hub ~ plaza 40m
            Floor("Floor_Plaza", new Vector3(0f, 0f, 0f), new Vector3(5.5f, 1f, 5.5f), matHub, root);
            Ring("PlazaRing", Vector3.zero, 18f, 8, matWall, matCyan, root);
            Box("Landmark_BootSpire", new Vector3(0f, 8f, -16f), new Vector3(2.2f, 16f, 2.2f), matCyan, root);
            Box("Stall_Market", new Vector3(8f, 1.2f, 4f), new Vector3(4f, 2.4f, 2.2f), matShop, root);
            Box("QuestBoard", new Vector3(-7f, 1.4f, 6f), new Vector3(0.3f, 2.8f, 3.2f), matCyan, root);
            WorldZoneSign.Create("RUNTIME PLAZA", "Hub · Boot Sector", new Vector3(-10f, 2.4f, -4f), new Color(0.2f, 0.85f, 1f));

            Checkpoint cpHub = MakeCheckpoint("Checkpoint_Plaza", "plaza", new Vector3(0f, 0.1f, -6f), root);
            RegionDefinition plaza = RegionDefinition.CreateRuntime(RegionId.RuntimePlaza, "Runtime Plaza",
                new Color(0.34f, 0.38f, 0.46f), new Color(0.06f, 0.08f, 0.12f), 0.006f, 0f);
            MakeRegion("Region_Plaza", plaza, new Vector3(0f, 2f, 0f), new Vector3(48f, 8f, 48f), cpHub, null, root);

            MakeTerminal("ft_hub", "Plaza", new Vector3(-4f, 0.6f, -8f), matCyan, root);
            MakeNpcs(matNpc, matShop, root);
            MakeShopKeeper(matShop, root);

            // Heap Wilds +Z
            Floor("Floor_Heap", new Vector3(0f, 0f, 70f), new Vector3(6f, 1f, 5.5f), matHeap, root);
            Scatter("HeapNodes", new Vector3(0f, 0.6f, 70f), 14, 16f, matMag, root);
            Box("Landmark_HeapTree", new Vector3(10f, 4f, 78f), new Vector3(1.4f, 8f, 1.4f), matMag, root);
            WorldZoneSign.Create("HEAP WILDS", "Fragmentación", new Vector3(-12f, 2.4f, 55f), new Color(0.95f, 0.2f, 0.65f));
            Checkpoint cpHeap = MakeCheckpoint("Checkpoint_Heap", "heap", new Vector3(0f, 0.1f, 48f), root);
            RegionDefinition heap = RegionDefinition.CreateRuntime(RegionId.HeapWilds, "Heap Wilds",
                new Color(0.4f, 0.22f, 0.32f), new Color(0.12f, 0.04f, 0.08f), 0.01f, 0.2f);
            MakeRegion("Region_Heap", heap, new Vector3(0f, 2f, 70f), new Vector3(52f, 8f, 50f), cpHeap, "poi_heap", root);
            MakeTerminal("ft_heap", "Heap", new Vector3(6f, 0.6f, 52f), matMag, root);
            SpawnLeak("Leak_Heap_A", new Vector3(-6f, 1.15f, 62f), LeakVariant.Basic, matLeak, root);
            SpawnLeak("Leak_Heap_B", new Vector3(5f, 1.15f, 74f), LeakVariant.Basic, matLeak, root);
            SpawnLeak("Leak_Heap_C", new Vector3(-2f, 1.2f, 84f), LeakVariant.Spawner, matLeak, root);
            MakePortal("Portal_HeapCave", "Dungeon_HeapCave", "Heap Cave", new Vector3(-8f, 1.5f, 90f), false, matMag, root);
            DungeonLayoutBuilder.BuildInline("Dungeon_HeapCave", new Vector3(-8f, 0f, 102f), root);

            // Stack Ridge -X
            Floor("Floor_Stack", new Vector3(-70f, 2f, 20f), new Vector3(5f, 1f, 5f), matStack, root);
            for (int i = 0; i < 6; i++)
            {
                Box($"StackTerrace_{i}", new Vector3(-55f - i * 4f, 0.5f + i * 0.45f, 18f + i * 2f), new Vector3(6f, 1f, 6f), matStack, root);
            }

            Box("Landmark_StackTower", new Vector3(-80f, 10f, 28f), new Vector3(3f, 20f, 3f), matCyan, root);
            WorldZoneSign.Create("STACK RIDGE", "Profundidad de llamada", new Vector3(-50f, 4f, 10f), new Color(0.4f, 0.8f, 1f));
            Checkpoint cpStack = MakeCheckpoint("Checkpoint_Stack", "stack", new Vector3(-48f, 2.1f, 16f), root);
            RegionDefinition stack = RegionDefinition.CreateRuntime(RegionId.StackRidge, "Stack Ridge",
                new Color(0.28f, 0.36f, 0.48f), new Color(0.05f, 0.08f, 0.14f), 0.008f, 0.1f);
            MakeRegion("Region_Stack", stack, new Vector3(-70f, 4f, 22f), new Vector3(48f, 12f, 46f), cpStack, null, root);
            MakeTerminal("ft_stack", "Stack", new Vector3(-52f, 2.6f, 14f), matCyan, root);
            SpawnLeak("Leak_Stack_A", new Vector3(-68f, 3.2f, 24f), LeakVariant.Spawner, matLeak, root);
            SpawnLeak("Leak_Stack_B", new Vector3(-76f, 3.2f, 32f), LeakVariant.NullRef, matLeak, root);
            MakePortal("Portal_StackChasm", "Dungeon_StackChasm", "Stack Chasm", new Vector3(-88f, 3.5f, 36f), false, matCyan, root);
            DungeonLayoutBuilder.BuildInline("Dungeon_StackChasm", new Vector3(-96f, 2f, 48f), root);

            // I/O Wastes +X
            Floor("Floor_IO", new Vector3(70f, 0f, 20f), new Vector3(5.5f, 1f, 5.5f), matIo, root);
            Scatter("IoDunes", new Vector3(70f, 0.4f, 20f), 10, 14f, matIo, root);
            Box("Landmark_IoTower", new Vector3(82f, 7f, 28f), new Vector3(1.6f, 14f, 1.6f), matShop, root);
            WorldZoneSign.Create("I/O WASTES", "Relay Tower", new Vector3(55f, 2.4f, 8f), new Color(1f, 0.7f, 0.25f));
            Checkpoint cpIo = MakeCheckpoint("Checkpoint_IO", "io", new Vector3(50f, 0.1f, 10f), root);
            RegionDefinition io = RegionDefinition.CreateRuntime(RegionId.IoWastes, "I/O Wastes",
                new Color(0.42f, 0.34f, 0.22f), new Color(0.14f, 0.1f, 0.05f), 0.007f, 0.05f);
            MakeRegion("Region_IO", io, new Vector3(70f, 2f, 20f), new Vector3(50f, 8f, 48f), cpIo, "poi_io_tower", root);
            MakeTerminal("ft_io", "I/O", new Vector3(58f, 0.6f, 12f), matShop, root);
            SpawnLeak("Leak_IO_A", new Vector3(74f, 1.15f, 26f), LeakVariant.Basic, matLeak, root);

            // Kernel Heights +Z far
            Floor("Floor_Kernel", new Vector3(0f, 4f, 150f), new Vector3(4.5f, 1f, 4.2f), matKernel, root);
            Box("Ramp_Kernel", new Vector3(0f, 2f, 122f), new Vector3(8f, 1f, 18f), matKernel, root);
            Ring("KernelRing", new Vector3(0f, 4f, 155f), 12f, 8, matWall, matBoss, root);
            WorldZoneSign.Create("KERNEL HEIGHTS", "Núcleo Corrupto", new Vector3(0f, 7f, 138f), new Color(0.75f, 0.15f, 1f));
            Checkpoint cpKernel = MakeCheckpoint("Checkpoint_Kernel", "kernel", new Vector3(0f, 4.1f, 136f), root);
            RegionDefinition kernel = RegionDefinition.CreateRuntime(RegionId.KernelHeights, "Kernel Heights",
                new Color(0.28f, 0.18f, 0.4f), new Color(0.08f, 0.02f, 0.12f), 0.012f, 0.3f);
            MakeRegion("Region_Kernel", kernel, new Vector3(0f, 6f, 150f), new Vector3(42f, 12f, 44f), cpKernel, null, root);
            MakePortal("Portal_KernelGate", "KernelGate", "Kernel Gate", new Vector3(0f, 5.5f, 142f), true, matBoss, root);
            SpawnLeak("Leak_Boss_Kernel", new Vector3(0f, 5.6f, 155f), LeakVariant.Boss, matBoss, root, true);

            GameObject player = PlayerKit.Create(new Vector3(0f, 1.1f, -4f), matPlayer, matCore, PlayerLoadout.WorldRPG);
            player.transform.SetParent(root.transform, true);

            AmbientSpawnDirector ambient = systems.GetComponent<AmbientSpawnDirector>();
            ambient?.SetMaterial(matLeak);

            if (systems.GetComponent<WorldBootstrap>() == null) systems.AddComponent<WorldBootstrap>();

            Debug.Log("WorldLayoutBuilder: Ciudad-Runtime greybox lista.");
        }

        public static GameObject SpawnLeak(string name, Vector3 pos, LeakVariant variant, Material mat, Transform parent = null, bool bossVictory = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            if (parent != null) go.transform.SetParent(parent, true);
            float scale = variant == LeakVariant.Boss ? 2.3f : variant == LeakVariant.Spawner ? 1.5f : 1.25f;
            SphereCollider col = go.AddComponent<SphereCollider>();
            col.radius = 0.7f * scale;
            MemoryLeakEnemy leak = go.AddComponent<MemoryLeakEnemy>();
            float hp = variant == LeakVariant.Boss ? 280f : variant == LeakVariant.Spawner ? 110f : variant == LeakVariant.NullRef ? 70f : 80f;
            leak.Configure(variant, hp, variant == LeakVariant.Boss ? 1.2f : 0.4f, 2.4f, 8, bossVictory);
            LeakAvatarBuilder.Build(go.transform, mat, scale);
            return go;
        }

        private static void ApplySky()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.46f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0065f;
            RenderSettings.fogColor = new Color(0.07f, 0.09f, 0.13f);
            Shader sky = Shader.Find("Skybox/Procedural");
            if (sky != null)
            {
                RenderSettings.skybox = new Material(sky);
            }
        }

        private static void EnsureLight()
        {
            if (Object.FindAnyObjectByType<Light>() != null) return;
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.85f, 0.92f, 1f);
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null) return;
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<ThirdPersonCamera>();
            camGo.transform.position = new Vector3(0f, 6f, -14f);
        }

        private static GameObject EnsureSystems()
        {
            GameObject systems = GameObject.Find("GameSystems");
            if (systems == null) systems = new GameObject("GameSystems");
            Require<GameSession>(systems);
            Require<CheckpointSystem>(systems);
            Require<NullBorn.Demo.TutorialDirector>(systems);
            Require<NullBorn.UI.CanvasHUD>(systems);
            Require<WorldMapController>(systems);
            Require<NullBorn.RPG.Quest.QuestManager>(systems);
            Require<NullBorn.RPG.Dialogue.DialogueRunner>(systems);
            Require<NullBorn.Persistence.SaveManager>(systems);
            Require<RamPressure>(systems);
            Require<AmbientSpawnDirector>(systems);
            Require<AdditiveDungeonLoader>(systems);
            return systems;
        }

        private static T Require<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static GameObject Floor(string name, Vector3 pos, Vector3 scale, Material mat, GameObject parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent.transform, true);
            return go;
        }

        private static void Box(string name, Vector3 pos, Vector3 scale, Material mat, GameObject parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent.transform, true);
        }

        private static void Ring(string name, Vector3 center, float radius, int count, Material a, Material b, GameObject parent)
        {
            for (int i = 0; i < count; i++)
            {
                float ang = i * (360f / count) * Mathf.Deg2Rad;
                Vector3 p = center + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * radius;
                Box($"{name}_{i}", p + Vector3.up * 2f, new Vector3(1.4f, 4f, 1.4f), i % 2 == 0 ? a : b, parent);
            }
        }

        private static void Scatter(string prefix, Vector3 center, int count, float radius, Material mat, GameObject parent)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 p = center + new Vector3(Random.Range(-radius, radius), 0.4f, Random.Range(-radius, radius));
                Box($"{prefix}_{i}", p, Vector3.one * Random.Range(0.5f, 1.4f), mat, parent);
            }
        }

        private static Checkpoint MakeCheckpoint(string name, string id, Vector3 pos, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.SetParent(parent.transform, true);
            Checkpoint cp = go.AddComponent<Checkpoint>();
            cp.SetZoneId(id);
            return cp;
        }

        private static void MakeRegion(string name, RegionDefinition def, Vector3 pos, Vector3 size, Checkpoint cp, string reachId, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.SetParent(parent.transform, true);
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            go.AddComponent<RegionTrigger>().Configure(def, cp, reachId);
        }

        private static void MakeTerminal(string id, string label, Vector3 pos, Material mat, GameObject parent)
        {
            Box($"TermMesh_{id}", pos + Vector3.up * 0.8f, new Vector3(0.8f, 1.6f, 0.8f), mat, parent);
            GameObject go = new GameObject($"Terminal_{id}");
            go.transform.position = pos;
            go.transform.SetParent(parent.transform, true);
            SphereCollider col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.6f;
            go.AddComponent<FastTravelTerminal>().Configure(id, label);
        }

        private static void MakePortal(string name, string scene, string label, Vector3 pos, bool boss, Material mat, GameObject parent)
        {
            Box(name + "_Arch", pos, new Vector3(4.5f, 3.2f, 0.5f), mat, parent);
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.SetParent(parent.transform, true);
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 3f, 2f);
            go.AddComponent<DungeonPortal>().Configure(scene, label, boss);
        }

        private static void MakeNpcs(Material matNpc, Material matShop, GameObject parent)
        {
            SpawnNpc("NPC_Daemon", "npc_daemon", "Daemon", new Vector3(-3f, 1f, 5f), matNpc, parent, DaemonLines(), false);
            SpawnNpc("NPC_Logger", "npc_logger", "Logger", new Vector3(-4f, 1f, 50f), matNpc, parent, LoggerLines(), false);
            GameObject sched = SpawnNpc("NPC_Scheduler", "npc_scheduler", "Scheduler", new Vector3(6f, 1f, 8f), matNpc, parent, null, false);
            NpcPatrol patrol = sched.AddComponent<NpcPatrol>();
            patrol.SetWaypoints(new[]
            {
                new Vector3(6f, 1f, 8f),
                new Vector3(12f, 1f, 2f),
                new Vector3(4f, 1f, -2f)
            });
        }

        private static void MakeShopKeeper(Material mat, GameObject parent)
        {
            GameObject go = SpawnNpc("NPC_Vendor", "npc_vendor", "Cache Broker", new Vector3(8f, 1f, 2.5f), mat, parent, null, true);
            go.AddComponent<Shop>();
        }

        private static GameObject SpawnNpc(string goName, string id, string display, Vector3 pos, Material mat, GameObject parent, DialogueNode[] lines, bool shop)
        {
            GameObject go = new GameObject(goName);
            go.transform.position = pos;
            go.transform.SetParent(parent.transform, true);
            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.height = 2f;
            col.radius = 0.6f;
            NpcController npc = go.AddComponent<NpcController>();
            npc.Configure(id, display, lines, shop, mat);
            return go;
        }

        private static DialogueNode[] DaemonLines()
        {
            return new[]
            {
                new DialogueNode
                {
                    id = "d0",
                    text = "Daemon: Guardián, el Runtime se desangra. Toma este ticket: contén los Goteos del Heap.",
                    questTalkId = "npc_daemon",
                    choices = new[]
                    {
                        new DialogueChoice { label = "Acepto el ticket.", nextId = "d1", questNotifyId = "npc_daemon" },
                        new DialogueChoice { label = "¿Qué es un Goteo?", nextId = "d2" }
                    }
                },
                new DialogueNode
                {
                    id = "d1",
                    text = "Daemon: Heap Wilds al norte. La plaza es segura… por ahora. Usa Q para fijar objetivo."
                },
                new DialogueNode
                {
                    id = "d2",
                    text = "Daemon: Un Goteo es una fuga de memoria con cuerpo. Telegrafía antes de golpear. Esquiva (Espacio) y corta.",
                    nextId = "d1"
                }
            };
        }

        private static DialogueNode[] LoggerLines()
        {
            return new[]
            {
                new DialogueNode
                {
                    id = "l0",
                    text = "Logger: Necesito fragmentos catalogados. Púrgalos con G y tráeme el registro.",
                    questTalkId = "npc_logger",
                    choices = new[]
                    {
                        new DialogueChoice { label = "Lo haré.", nextId = "l1", questNotifyId = "npc_logger" },
                        new DialogueChoice { label = "Luego.", nextId = null }
                    }
                },
                new DialogueNode { id = "l1", text = "Logger: Cada fragmento es RAM no liberada. La Onda de Purga los encadena." }
            };
        }
    }
}
