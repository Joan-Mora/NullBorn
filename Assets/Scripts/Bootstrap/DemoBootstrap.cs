using NullBorn.Core;
using NullBorn.Demo;
using NullBorn.Enemies;
using NullBorn.Player;
using NullBorn.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.Bootstrap
{
    /// <summary>
    /// Enlaza sistemas del Demo_10min al pulsar Play + cursor/reinicio/respawn.
    /// </summary>
    public class DemoBootstrap : MonoBehaviour
    {
        private GameSession session;
        private Transform playerTransform;
        private MemoryLeakEnemy[] allLeaks;
        private CanvasHUD hud;
        private DemoFlowController flow;

        private void Awake()
        {
            Wire();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            TutorialDirector.Instance?.Show("boot_welcome");
            ZoneSignEnsurer.Ensure();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && Time.timeScale > 0.01f)
            {
                if (session == null || session.CurrentState == GameSession.State.Playing)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            if (session != null && session.CurrentState == GameSession.State.Playing)
            {
                UpdateHudTarget();
            }

            if (!Input.GetKeyDown(KeyCode.R)) return;

            if (session != null && session.CurrentState == GameSession.State.Victory)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            Time.timeScale = 1f;
            if (session != null && session.CurrentState == GameSession.State.Defeat)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            CheckpointSystem.Instance?.RespawnPlayer();
        }

        private void Wire()
        {
            GameObject player = GameObject.Find("Player_Thread");
            Camera mainCam = Camera.main;
            GameObject systems = GameObject.Find("GameSystems");

            if (player == null || mainCam == null || systems == null)
            {
                Debug.LogError("DemoBootstrap: faltan Player_Thread / Main Camera / GameSystems. Genera la escena con NullBorn → Generar Demo 10min.");
                return;
            }

            player.tag = "Player";
            playerTransform = player.transform;

            SystemIntegrity integrity = player.GetComponent<SystemIntegrity>();
            CpuCycles cpu = player.GetComponent<CpuCycles>();
            ThirdPersonController movement = player.GetComponent<ThirdPersonController>();
            GarbageCollectionAbility gc = player.GetComponent<GarbageCollectionAbility>();
            DebugStrikeAbility strike = player.GetComponent<DebugStrikeAbility>();

            session = systems.GetComponent<GameSession>();
            CheckpointSystem checkpoints = systems.GetComponent<CheckpointSystem>();
            hud = systems.GetComponent<CanvasHUD>();
            if (hud == null) hud = systems.AddComponent<CanvasHUD>();

            flow = systems.GetComponent<DemoFlowController>();
            if (systems.GetComponent<TutorialDirector>() == null) systems.AddComponent<TutorialDirector>();

            ThirdPersonCamera orbit = mainCam.GetComponent<ThirdPersonCamera>();
            if (orbit == null) orbit = mainCam.gameObject.AddComponent<ThirdPersonCamera>();
            orbit.SetTarget(player.transform);

            movement.Initialize(mainCam.transform);
            gc.Initialize(cpu, integrity, session);
            strike.Initialize(cpu, session);
            session.Bind(integrity);
            checkpoints.RegisterPlayer(player.transform, integrity, cpu);

            Checkpoint start = GameObject.Find("Checkpoint_Boot")?.GetComponent<Checkpoint>();
            if (start != null) checkpoints.SetCheckpoint(start);

            hud.Bind(integrity, cpu, session);

            allLeaks = Object.FindObjectsByType<MemoryLeakEnemy>();
            foreach (MemoryLeakEnemy leak in allLeaks)
            {
                Renderer rend = leak.GetComponentInChildren<Renderer>();
                Material mat = rend != null ? rend.material : null;
                leak.Initialize(integrity, session, mat);

                if (leak.Variant == LeakVariant.Boss || leak.name.Contains("Boss"))
                {
                    flow.SetBoss(leak);
                }
            }

            UpdateHudTarget();

            session.StateChanged += state =>
            {
                if (state != GameSession.State.Playing)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            };
        }

        private void UpdateHudTarget()
        {
            if (hud == null || playerTransform == null || allLeaks == null) return;

            DemoZoneId zone = flow != null ? flow.CurrentZone : DemoZoneId.BootSector;

            if (zone == DemoZoneId.KernelCore && flow?.BossLeak != null && flow.BossLeak.IsAlive)
            {
                hud.TrackLeak(flow.BossLeak);
                return;
            }

            MemoryLeakEnemy nearest = null;
            float bestDist = float.MaxValue;
            foreach (MemoryLeakEnemy leak in allLeaks)
            {
                if (leak == null || !leak.IsAlive) continue;
                if (leak.Variant == LeakVariant.Boss) continue;

                float dist = Vector3.SqrMagnitude(leak.transform.position - playerTransform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = leak;
                }
            }

            hud.TrackLeak(nearest);
        }
    }
}
