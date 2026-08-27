using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.Player;
using NullBorn.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.World
{
    /// <summary>
    /// Cableado de Combat_Arena (Fase 0b).
    /// </summary>
    public class CombatArenaBootstrap : MonoBehaviour
    {
        private GameSession session;
        private CanvasHUD hud;
        private Transform playerTransform;

        private void Awake()
        {
            Wire();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            NullBorn.Demo.TutorialDirector.Instance?.ShowRaw(
                "Arena: Shift sprint · Espacio dash (i-frames) · Click/F Corte de Traza · G Purga · V Restaurar · Q lock-on",
                8f);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && Time.timeScale > 0.5f)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            UpdateHudTarget();

            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void Wire()
        {
            GameObject player = GameObject.Find("Player_Thread");
            Camera mainCam = Camera.main;
            GameObject systems = GameObject.Find("GameSystems");
            if (player == null || mainCam == null || systems == null) return;

            player.tag = "Player";
            playerTransform = player.transform;
            SystemIntegrity integrity = player.GetComponent<SystemIntegrity>();
            CpuCycles cpu = player.GetComponent<CpuCycles>();
            session = systems.GetComponent<GameSession>();
            hud = systems.GetComponent<CanvasHUD>();
            if (hud == null) hud = systems.AddComponent<CanvasHUD>();

            ThirdPersonCamera orbit = mainCam.GetComponent<ThirdPersonCamera>();
            if (orbit == null) orbit = mainCam.gameObject.AddComponent<ThirdPersonCamera>();
            orbit.SetTarget(player.transform);

            player.GetComponent<ThirdPersonController>()?.Initialize(mainCam.transform);
            player.GetComponent<TraceCutCombo>()?.Initialize(cpu, session);
            player.GetComponent<PurgeWaveAbility>()?.Initialize(cpu, session);
            player.GetComponent<RestoreAbility>()?.Initialize(cpu, integrity, session);
            player.GetComponent<CombatFeedback>()?.BindCamera(orbit);
            session.Bind(integrity);
            hud.Bind(integrity, cpu, session);

            CheckpointSystem checkpoints = systems.GetComponent<CheckpointSystem>();
            checkpoints?.RegisterPlayer(player.transform, integrity, cpu);
            Checkpoint start = GameObject.Find("Checkpoint_Arena")?.GetComponent<Checkpoint>();
            if (start != null) checkpoints?.SetCheckpoint(start);

            MemoryLeakEnemy[] leaks = FindObjectsByType<MemoryLeakEnemy>(FindObjectsSortMode.None);
            foreach (MemoryLeakEnemy leak in leaks)
            {
                Renderer rend = leak.GetComponentInChildren<Renderer>();
                leak.Initialize(integrity, session, rend != null ? rend.material : null);
            }
        }

        private void UpdateHudTarget()
        {
            if (hud == null || playerTransform == null) return;
            MemoryLeakEnemy nearest = null;
            float best = 40f;
            MemoryLeakEnemy[] leaks = FindObjectsByType<MemoryLeakEnemy>(FindObjectsSortMode.None);
            foreach (MemoryLeakEnemy leak in leaks)
            {
                if (leak == null || !leak.IsAlive) continue;
                float d = Vector3.Distance(leak.transform.position, playerTransform.position);
                if (d < best)
                {
                    best = d;
                    nearest = leak;
                }
            }

            hud.TrackLeak(nearest);
        }
    }
}
