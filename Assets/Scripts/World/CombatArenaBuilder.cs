using NullBorn.Art;
using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.Player;
using NullBorn.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.World
{
    /// <summary>
    /// Arena de playtest Fase 0b: 3 Goteos, suelo, feedback de combate.
    /// </summary>
    public static class CombatArenaBuilder
    {
        public static void Build()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.32f, 0.4f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.01f;
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.1f);

            Material floor = RuntimeMaterials.Lit(new Color(0.1f, 0.12f, 0.16f), new Color(0.1f, 0.3f, 0.4f) * 0.3f);
            Material wall = RuntimeMaterials.Lit(new Color(0.16f, 0.18f, 0.22f), Color.black);
            Material accent = RuntimeMaterials.Lit(new Color(0.15f, 0.35f, 0.4f), new Color(0.2f, 0.8f, 1f) * 1.4f);
            Material playerMat = RuntimeMaterials.Lit(new Color(0.2f, 0.55f, 0.75f), new Color(0.1f, 0.4f, 0.6f) * 0.3f);
            Material core = RuntimeMaterials.Lit(new Color(0.25f, 0.9f, 1f), new Color(0.25f, 0.9f, 1f) * 2f);
            Material leak = RuntimeMaterials.Lit(new Color(0.85f, 0.12f, 0.35f), new Color(0.9f, 0.1f, 0.4f) * 1.4f);

            if (Object.FindAnyObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                Light l = lightGo.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.2f;
                lightGo.transform.rotation = Quaternion.Euler(50f, -20f, 0f);
            }

            if (Camera.main == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.AddComponent<ThirdPersonCamera>();
            }

            GameObject systems = GameObject.Find("GameSystems");
            if (systems == null) systems = new GameObject("GameSystems");
            if (systems.GetComponent<GameSession>() == null) systems.AddComponent<GameSession>();
            if (systems.GetComponent<CheckpointSystem>() == null) systems.AddComponent<CheckpointSystem>();
            if (systems.GetComponent<NullBorn.Demo.TutorialDirector>() == null) systems.AddComponent<NullBorn.Demo.TutorialDirector>();
            if (systems.GetComponent<CanvasHUD>() == null) systems.AddComponent<CanvasHUD>();
            if (systems.GetComponent<RamPressure>() == null) systems.AddComponent<RamPressure>();

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "ArenaFloor";
            plane.transform.localScale = new Vector3(3.2f, 1f, 3.2f);
            plane.GetComponent<Renderer>().sharedMaterial = floor;

            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.name = $"Pillar_{i}";
                p.transform.position = new Vector3(Mathf.Cos(a), 1.5f, Mathf.Sin(a)) * 12f;
                p.transform.localScale = new Vector3(1.2f, 3f, 1.2f);
                p.GetComponent<Renderer>().sharedMaterial = i % 2 == 0 ? wall : accent;
            }

            WorldZoneSign.Create("COMBAT ARENA", "Fase 0b · 60 s", new Vector3(0f, 2.6f, -10f), new Color(0.3f, 0.9f, 1f));

            Checkpoint cp = new GameObject("Checkpoint_Arena").AddComponent<Checkpoint>();
            cp.transform.position = new Vector3(0f, 0.1f, -6f);
            cp.SetZoneId("arena");

            WorldLayoutBuilder.SpawnLeak("Goteo_A", new Vector3(-4f, 1.15f, 5f), LeakVariant.Basic, leak);
            WorldLayoutBuilder.SpawnLeak("Goteo_B", new Vector3(4f, 1.15f, 6f), LeakVariant.Basic, leak);
            WorldLayoutBuilder.SpawnLeak("Goteo_C", new Vector3(0f, 1.15f, 9f), LeakVariant.Basic, leak);

            PlayerKit.Create(new Vector3(0f, 1.1f, -4f), playerMat, core, PlayerLoadout.CombatArena);
            if (systems.GetComponent<CombatArenaBootstrap>() == null) systems.AddComponent<CombatArenaBootstrap>();
            Debug.Log("CombatArenaBuilder: arena lista.");
        }
    }
}
