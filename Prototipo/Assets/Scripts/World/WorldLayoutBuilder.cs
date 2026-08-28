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
    /// Overworld con terreno continuo, vegetación, edificios, caminos y cielo.
    /// Kit procedural stylized (sin Asset Store) — Ciudad-Runtime.
    /// </summary>
    public static class WorldLayoutBuilder
    {
        public static void Build()
        {
            foreach (string n in new[] { "WorldRoot", "GameSystems", "Player_Thread" })
            {
                GameObject leftover;
                while ((leftover = GameObject.Find(n)) != null)
                    Object.DestroyImmediate(leftover);
            }

            ApplySky();

            // Textured biome grounds
            Texture2D texHub = ProceduralTextures.GridAlbedo(256, new Color(0.14f, 0.18f, 0.22f), new Color(0.22f, 0.45f, 0.55f), 16);
            Texture2D texGrass = ProceduralTextures.NoiseAlbedo(256, new Color(0.12f, 0.22f, 0.16f), new Color(0.2f, 0.35f, 0.22f), 0.08f, 11);
            Texture2D texRock = ProceduralTextures.NoiseAlbedo(256, new Color(0.22f, 0.24f, 0.28f), new Color(0.35f, 0.38f, 0.42f), 0.12f, 33);
            Texture2D texBark = ProceduralTextures.NoiseAlbedo(128, new Color(0.2f, 0.12f, 0.08f), new Color(0.35f, 0.22f, 0.12f), 0.2f, 55);
            Texture2D texLeaf = ProceduralTextures.NoiseAlbedo(128, new Color(0.1f, 0.28f, 0.18f), new Color(0.18f, 0.45f, 0.28f), 0.15f, 66);
            Texture2D texLeafBad = ProceduralTextures.NoiseAlbedo(128, new Color(0.25f, 0.08f, 0.18f), new Color(0.45f, 0.12f, 0.3f), 0.15f, 77);
            Texture2D texPath = ProceduralTextures.NoiseAlbedo(128, new Color(0.25f, 0.26f, 0.28f), new Color(0.4f, 0.42f, 0.45f), 0.2f, 88);

            Material matGround = RuntimeMaterials.Textured(texGrass, Color.white, 1f);
            Material matHubFloor = RuntimeMaterials.Textured(texHub, Color.white, 1f, new Color(0.05f, 0.2f, 0.28f) * 0.2f);
            Material matWall = RuntimeMaterials.Lit(new Color(0.28f, 0.32f, 0.38f), Color.black);
            Material matWallAccent = RuntimeMaterials.Lit(new Color(0.12f, 0.35f, 0.45f), new Color(0.15f, 0.7f, 0.95f) * 0.8f);
            Material matBark = RuntimeMaterials.Textured(texBark, Color.white, 2f);
            Material matLeaf = RuntimeMaterials.Textured(texLeaf, Color.white, 1f);
            Material matLeafBad = RuntimeMaterials.Textured(texLeafBad, Color.white, 1f, new Color(0.5f, 0.05f, 0.25f) * 0.4f);
            Material matPath = RuntimeMaterials.Textured(texPath, Color.white, 2f);
            Material matCyan = RuntimeMaterials.Lit(new Color(0.15f, 0.35f, 0.4f), new Color(0.2f, 0.8f, 1f) * 1.4f);
            Material matMag = RuntimeMaterials.Lit(new Color(0.3f, 0.1f, 0.25f), new Color(0.9f, 0.1f, 0.6f) * 1.1f);
            Material matPlayer = RuntimeMaterials.Lit(new Color(0.2f, 0.55f, 0.75f), new Color(0.1f, 0.4f, 0.6f) * 0.3f);
            Material matCore = RuntimeMaterials.Lit(new Color(0.25f, 0.9f, 1f), new Color(0.25f, 0.9f, 1f) * 2f);
            Material matLeak = RuntimeMaterials.Lit(new Color(0.85f, 0.12f, 0.35f), new Color(0.9f, 0.1f, 0.4f) * 1.4f);
            Material matBoss = RuntimeMaterials.Lit(new Color(0.55f, 0.05f, 0.7f), new Color(0.7f, 0.05f, 0.9f) * 1.8f);
            Material matNpc = RuntimeMaterials.Lit(new Color(0.35f, 0.7f, 0.75f), new Color(0.2f, 0.6f, 0.7f) * 0.6f);
            Material matShop = RuntimeMaterials.Lit(new Color(0.75f, 0.55f, 0.2f), new Color(1f, 0.7f, 0.2f) * 0.8f);
            Material matRock = RuntimeMaterials.Textured(texRock, new Color(0.85f, 0.85f, 0.9f), 1f);

            GameObject root = new GameObject("WorldRoot");
            Transform artRoot = new GameObject("EnvironmentArt").transform;
            artRoot.SetParent(root.transform, false);

            EnsureLight();
            EnsureCamera();
            GameObject systems = EnsureSystems();

            // Continuous living terrain (~220m) — low res / low amp to avoid clipping props
            EnvironmentKit.GroundMesh("Terrain_World", Vector3.zero, 220f, 32, 1.2f, 101, matGround, artRoot);

            // Hub plaza disc (readable floor; collider removed — terrain handles collision)
            GameObject plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Floor_Plaza";
            plaza.transform.SetParent(artRoot, true);
            plaza.transform.position = new Vector3(0f, 0.05f, 0f);
            plaza.transform.localScale = new Vector3(42f, 0.08f, 42f);
            plaza.GetComponent<Renderer>().sharedMaterial = matHubFloor;
            Collider plazaCol = plaza.GetComponent<Collider>();
            if (plazaCol != null)
            {
                if (Application.isPlaying) Object.Destroy(plazaCol);
                else Object.DestroyImmediate(plazaCol);
            }

            // Buildings around plaza
            EnvironmentKit.Building("Bld_North", new Vector3(-10f, 0f, 14f), new Vector3(6f, 1f, 5f), 4f, matWall, matWallAccent, artRoot);
            EnvironmentKit.Building("Bld_East", new Vector3(14f, 0f, 6f), new Vector3(5f, 1f, 7f), 5f, matWall, matCyan, artRoot);
            EnvironmentKit.Building("Bld_West", new Vector3(-16f, 0f, -4f), new Vector3(7f, 1f, 5f), 3f, matWall, matWallAccent, artRoot);
            EnvironmentKit.Building("Bld_Market", new Vector3(10f, 0f, -8f), new Vector3(8f, 1f, 4.5f), 2f, matShop, matCyan, artRoot);
            EnvironmentKit.Building("Landmark_BootSpire", new Vector3(0f, 0f, -18f), new Vector3(3.2f, 1f, 3.2f), 7f, matWallAccent, matCyan, artRoot);

            Box("QuestBoard", new Vector3(-7f, 1.4f, 6f), new Vector3(0.3f, 2.8f, 3.2f), matCyan, root);
            for (int i = 0; i < 4; i++)
            {
                float ang = i * 90f * Mathf.Deg2Rad;
                Vector3 lp = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 16f;
                EnvironmentKit.Lamp($"Lamp_Plaza_{i}", lp, matWall, matCyan, artRoot);
            }

            WorldZoneSign.Create("RUNTIME PLAZA", "Hub · Ciudad-Runtime", new Vector3(-12f, 2.4f, -6f), new Color(0.2f, 0.85f, 1f));

            Checkpoint cpHub = MakeCheckpoint("Checkpoint_Plaza", "plaza", new Vector3(0f, 0.1f, -6f), root);
            RegionDefinition plazaDef = RegionDefinition.CreateRuntime(RegionId.RuntimePlaza, "Runtime Plaza",
                new Color(0.42f, 0.5f, 0.6f), new Color(0.35f, 0.45f, 0.55f), 0.003f, 0f);
            MakeRegion("Region_Plaza", plazaDef, new Vector3(0f, 2f, 0f), new Vector3(55f, 12f, 55f), cpHub, null, root);
            MakeTerminal("ft_hub", "Plaza", new Vector3(-4f, 0.6f, -8f), matCyan, root);
            MakeNpcs(matNpc, matShop, root);
            MakeShopKeeper(matShop, root);

            // Healthy ring forest around hub
            ScatterTrees(artRoot, Vector3.zero, 28f, 42f, 12, matBark, matLeaf, false);

            // === HEAP WILDS (+Z) ===
            ScatterTrees(artRoot, new Vector3(0f, 0f, 70f), 8f, 40f, 16, matBark, matLeafBad, true);
            ScatterRocks(artRoot, new Vector3(0f, 0f, 70f), 36f, 9, matRock);
            EnvironmentKit.Building("Ruin_Heap_A", new Vector3(-14f, 0f, 78f), new Vector3(5f, 1f, 4f), 2f, matMag, matBoss, artRoot);
            EnvironmentKit.Building("Ruin_Heap_B", new Vector3(16f, 0f, 66f), new Vector3(4f, 1f, 6f), 3f, matWall, matMag, artRoot);
            WorldZoneSign.Create("HEAP WILDS", "Fragmentación · bosque corrupto", new Vector3(-12f, 2.4f, 48f), new Color(0.95f, 0.2f, 0.65f));
            Checkpoint cpHeap = MakeCheckpoint("Checkpoint_Heap", "heap", new Vector3(0f, 0.1f, 48f), root);
            RegionDefinition heap = RegionDefinition.CreateRuntime(RegionId.HeapWilds, "Heap Wilds",
                new Color(0.45f, 0.28f, 0.35f), new Color(0.35f, 0.15f, 0.22f), 0.007f, 0.25f);
            MakeRegion("Region_Heap", heap, new Vector3(0f, 2f, 70f), new Vector3(58f, 12f, 55f), cpHeap, "poi_heap", root);
            MakeTerminal("ft_heap", "Heap", new Vector3(6f, 0.6f, 52f), matMag, root);
            SpawnLeak("Leak_Heap_A", new Vector3(-6f, 1.15f, 62f), LeakVariant.Basic, matLeak, root.transform);
            SpawnLeak("Leak_Heap_B", new Vector3(5f, 1.15f, 74f), LeakVariant.Basic, matLeak, root.transform);
            SpawnLeak("Leak_Heap_C", new Vector3(-2f, 1.2f, 84f), LeakVariant.Spawner, matLeak, root.transform);
            MakePortal("Portal_HeapCave", "Dungeon_HeapCave", "Heap Cave", new Vector3(-8f, 1.5f, 90f), false, matMag, root);
            DungeonLayoutBuilder.BuildInline("Dungeon_HeapCave", new Vector3(-8f, 0f, 102f), root);

            // === STACK RIDGE (-X) ===
            for (int i = 0; i < 7; i++)
            {
                Vector3 terrace = new Vector3(-52f - i * 4.5f, i * 0.55f, 16f + i * 2.2f);
                EnvironmentKit.Building($"StackTerrace_{i}", terrace, new Vector3(5.5f, 1f, 5.5f), 1f + i * 0.35f, matWall, matCyan, artRoot);
            }

            EnvironmentKit.Building("Landmark_StackTower", new Vector3(-82f, 2f, 28f), new Vector3(4f, 1f, 4f), 8f, matWallAccent, matCyan, artRoot);
            ScatterRocks(artRoot, new Vector3(-70f, 2f, 22f), 30f, 7, matRock);
            WorldZoneSign.Create("STACK RIDGE", "Profundidad de llamada", new Vector3(-48f, 4f, 8f), new Color(0.4f, 0.8f, 1f));
            Checkpoint cpStack = MakeCheckpoint("Checkpoint_Stack", "stack", new Vector3(-48f, 2.1f, 16f), root);
            RegionDefinition stack = RegionDefinition.CreateRuntime(RegionId.StackRidge, "Stack Ridge",
                new Color(0.35f, 0.42f, 0.55f), new Color(0.25f, 0.32f, 0.42f), 0.004f, 0.1f);
            MakeRegion("Region_Stack", stack, new Vector3(-70f, 4f, 22f), new Vector3(52f, 14f, 50f), cpStack, null, root);
            MakeTerminal("ft_stack", "Stack", new Vector3(-52f, 2.6f, 14f), matCyan, root);
            SpawnLeak("Leak_Stack_A", new Vector3(-68f, 3.2f, 24f), LeakVariant.Spawner, matLeak, root.transform);
            SpawnLeak("Leak_Stack_B", new Vector3(-76f, 3.2f, 32f), LeakVariant.NullRef, matLeak, root.transform);
            MakePortal("Portal_StackChasm", "Dungeon_StackChasm", "Stack Chasm", new Vector3(-88f, 3.5f, 36f), false, matCyan, root);
            DungeonLayoutBuilder.BuildInline("Dungeon_StackChasm", new Vector3(-96f, 2f, 48f), root);

            // === I/O WASTES (+X) ===
            EnvironmentKit.Antenna("Landmark_IoTower", new Vector3(82f, 0f, 28f), 16f, matWall, matShop, artRoot);
            EnvironmentKit.Antenna("Relay_Io_B", new Vector3(62f, 0f, 34f), 10f, matWall, matCyan, artRoot);
            ScatterRocks(artRoot, new Vector3(70f, 0f, 20f), 32f, 10, matRock);
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = new Vector3(70f, 0f, 20f) + Random.insideUnitSphere * 28f;
                p.y = 0f;
                EnvironmentKit.DeadTree($"IoDead_{i}", p, Random.Range(0.7f, 1.3f), matBark, artRoot);
            }

            WorldZoneSign.Create("I/O WASTES", "Relay · dunas de bus", new Vector3(52f, 2.4f, 6f), new Color(1f, 0.7f, 0.25f));
            Checkpoint cpIo = MakeCheckpoint("Checkpoint_IO", "io", new Vector3(50f, 0.1f, 10f), root);
            RegionDefinition io = RegionDefinition.CreateRuntime(RegionId.IoWastes, "I/O Wastes",
                new Color(0.5f, 0.4f, 0.28f), new Color(0.45f, 0.35f, 0.2f), 0.004f, 0.05f);
            MakeRegion("Region_IO", io, new Vector3(70f, 2f, 20f), new Vector3(55f, 12f, 52f), cpIo, "poi_io_tower", root);
            MakeTerminal("ft_io", "I/O", new Vector3(58f, 0.6f, 12f), matShop, root);
            SpawnLeak("Leak_IO_A", new Vector3(74f, 1.15f, 26f), LeakVariant.Basic, matLeak, root.transform);

            // === KERNEL HEIGHTS (+Z far) — ground level (biome disc removed) ===
            EnvironmentKit.Bridge("Bridge_ToKernel", new Vector3(0f, 0.2f, 115f), new Vector3(0f, 0.2f, 138f), 6f, matPath, matBoss, artRoot);
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f * Mathf.Deg2Rad;
                Vector3 p = new Vector3(0f, 0f, 155f) + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 14f;
                EnvironmentKit.Building($"KernelPillar_{i}", p, new Vector3(2.2f, 1f, 2.2f), 3.5f, matWall, matBoss, artRoot);
            }

            WorldZoneSign.Create("KERNEL HEIGHTS", "Núcleo Corrupto", new Vector3(0f, 2.4f, 138f), new Color(0.75f, 0.15f, 1f));
            Checkpoint cpKernel = MakeCheckpoint("Checkpoint_Kernel", "kernel", new Vector3(0f, 0.1f, 136f), root);
            RegionDefinition kernel = RegionDefinition.CreateRuntime(RegionId.KernelHeights, "Kernel Heights",
                new Color(0.32f, 0.22f, 0.45f), new Color(0.15f, 0.08f, 0.22f), 0.008f, 0.35f);
            MakeRegion("Region_Kernel", kernel, new Vector3(0f, 2f, 150f), new Vector3(46f, 14f, 48f), cpKernel, null, root);
            MakePortal("Portal_KernelGate", "KernelGate", "Kernel Gate", new Vector3(0f, 1.5f, 142f), true, matBoss, root);
            SpawnLeak("Leak_Boss_Kernel", new Vector3(0f, 1.2f, 155f), LeakVariant.Boss, matBoss, root.transform, true);

            // Roads connecting biomes
            EnvironmentKit.PathSegment("Road_Plaza_Heap", new Vector3(0f, 0.08f, 18f), new Vector3(0f, 0.08f, 48f), 5f, matPath, artRoot);
            EnvironmentKit.PathSegment("Road_Plaza_Stack", new Vector3(-18f, 0.08f, 4f), new Vector3(-48f, 2.1f, 14f), 4.5f, matPath, artRoot);
            EnvironmentKit.PathSegment("Road_Plaza_IO", new Vector3(18f, 0.08f, 4f), new Vector3(50f, 0.08f, 10f), 4.5f, matPath, artRoot);
            EnvironmentKit.Bridge("Bridge_HeapApproach", new Vector3(-2f, 0.1f, 40f), new Vector3(-2f, 0.1f, 55f), 4f, matPath, matMag, artRoot);

            GameObject player = PlayerKit.Create(new Vector3(0f, 1.1f, -4f), matPlayer, matCore, PlayerLoadout.WorldRPG);
            player.transform.SetParent(root.transform, true);

            AmbientSpawnDirector ambient = systems.GetComponent<AmbientSpawnDirector>();
            ambient?.SetMaterial(matLeak);

            if (systems.GetComponent<WorldBootstrap>() == null) systems.AddComponent<WorldBootstrap>();

            Debug.Log("WorldLayoutBuilder: Ciudad-Runtime con entorno procedural (terreno, árboles, edificios, caminos).");
        }

        private static void ScatterTrees(Transform parent, Vector3 center, float inner, float outer, int count, Material bark, Material leaf, bool deadMix)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 ring = Random.insideUnitCircle.normalized * Random.Range(inner, outer);
                Vector3 p = center + new Vector3(ring.x, 0f, ring.y);
                float s = Random.Range(0.85f, 1.6f);
                if (deadMix && Random.value < 0.45f)
                    EnvironmentKit.DeadTree($"TreeDead_{center.z:0}_{i}", p, s, bark, parent);
                else
                    EnvironmentKit.Tree($"Tree_{center.z:0}_{i}", p, s, bark, leaf, parent);
            }
        }

        private static void ScatterRocks(Transform parent, Vector3 center, float radius, int count, Material mat)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 p = center + new Vector3(Random.Range(-radius, radius), 0.2f, Random.Range(-radius, radius));
                EnvironmentKit.Rock($"Rock_{i}_{center.x:0}", p, Random.Range(0.6f, 1.8f), mat, parent);
            }
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
            Color zenith = new Color(0.15f, 0.35f, 0.55f);
            Color horizon = new Color(0.45f, 0.55f, 0.65f);
            Color nadir = new Color(0.08f, 0.1f, 0.14f);
            Cubemap skyCube = ProceduralTextures.GradientSky(64, zenith, horizon, nadir);
            Shader skyShader = Shader.Find("Skybox/Cubemap") ?? Shader.Find("Skybox/6 Sided");
            if (skyShader != null)
            {
                Material skyMat = new Material(skyShader);
                if (skyMat.HasProperty("_Tex")) skyMat.SetTexture("_Tex", skyCube);
                if (skyMat.HasProperty("_FrontTex"))
                {
                    // 6-sided fallback ignored if cubemap works
                }

                RenderSettings.skybox = skyMat;
            }
            else
            {
                Shader proc = Shader.Find("Skybox/Procedural");
                if (proc != null) RenderSettings.skybox = new Material(proc);
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = zenith;
            RenderSettings.ambientEquatorColor = horizon;
            RenderSettings.ambientGroundColor = nadir;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0045f;
            RenderSettings.fogColor = new Color(0.28f, 0.36f, 0.42f);
            DynamicGI.UpdateEnvironment();
        }

        private static void EnsureLight()
        {
            Light existing = Object.FindAnyObjectByType<Light>();
            if (existing != null && existing.type == LightType.Directional)
            {
                existing.color = new Color(1f, 0.95f, 0.88f);
                existing.intensity = 1.35f;
                existing.shadows = LightShadows.None;
                existing.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
                return;
            }

            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.88f);
            light.intensity = 1.35f;
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
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

        private static void Box(string name, Vector3 pos, Vector3 scale, Material mat, GameObject parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent.transform, true);
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
