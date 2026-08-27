using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.Persistence;
using NullBorn.Player;
using NullBorn.RPG.Dialogue;
using NullBorn.RPG.Economy;
using NullBorn.RPG.Inventory;
using NullBorn.RPG.Quest;
using NullBorn.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerInventory = NullBorn.RPG.Inventory.Inventory;

namespace NullBorn.World
{
    /// <summary>
    /// Bootstrap del overworld. No usa DemoBootstrap.
    /// </summary>
    public class WorldBootstrap : MonoBehaviour
    {
        private GameSession session;
        private Transform playerTransform;
        private CanvasHUD hud;
        private MemoryLeakEnemy[] leaks;

        private void Awake()
        {
            Wire();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && Time.timeScale > 0.5f)
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
                PushWorldHud();
            }

            if (Input.GetKeyDown(KeyCode.F5)) SaveManager.Instance?.Save();
            if (Input.GetKeyDown(KeyCode.F9)) SaveManager.Instance?.TryLoad();

            if (!Input.GetKeyDown(KeyCode.R)) return;
            Time.timeScale = 1f;

            // Victoria: reinicio limpio de escena.
            if (session != null && session.CurrentState == GameSession.State.Victory)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            // Derrota / muerte: respawn en checkpoint CON vida (no recargar save con HP 0).
            if (session != null && session.CurrentState == GameSession.State.Defeat)
            {
                CheckpointSystem.Instance?.RespawnPlayer();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
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
                Debug.LogError("WorldBootstrap: faltan Player_Thread / Camera / GameSystems. NullBorn → Build World Main.");
                return;
            }

            player.tag = "Player";
            playerTransform = player.transform;

            // Asegurar pie sobre el suelo al nacer (evita caer fuera del mapa).
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerTransform.position = CheckpointSystem.SnapToGround(playerTransform.position);
            if (cc != null) cc.enabled = true;

            SystemIntegrity integrity = player.GetComponent<SystemIntegrity>();
            CpuCycles cpu = player.GetComponent<CpuCycles>();
            session = systems.GetComponent<GameSession>();
            CheckpointSystem checkpoints = systems.GetComponent<CheckpointSystem>();
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
            player.GetComponent<PlayerProgression>()?.Initialize(integrity, cpu);
            session.Bind(integrity);
            checkpoints.RegisterPlayer(player.transform, integrity, cpu);

            Checkpoint start = GameObject.Find("Checkpoint_Plaza")?.GetComponent<Checkpoint>();
            if (start != null) checkpoints.SetCheckpoint(start);

            hud.Bind(integrity, cpu, session);
            hud.SetWorldMode(true);
            WorldMapController.Instance?.UnlockTerminal("ft_hub");

            QuestManager quests = systems.GetComponent<QuestManager>();
            quests?.LoadStarterQuests();
            quests?.TryAutoStart();

            SaveManager save = systems.GetComponent<SaveManager>();
            save?.RegisterPlayer(player.transform);
            save?.BeginApiSession(player.GetComponent<PlayerProgression>() != null ? player.GetComponent<PlayerProgression>().Level : 1);

            // Auto-load: si el save tenía muerte (HP 0), Apply ya rellena vida.
            // Si el save deja al jugador bajo el suelo, SnapToGround lo corrige.
            if (save != null && save.HasSave)
            {
                save.TryLoad();
                // Garantía post-load: nunca empezar colapsado.
                if (integrity != null && integrity.IsCollapsed)
                {
                    integrity.ResetIntegrity();
                    session.ResumePlaying();
                    checkpoints.RespawnPlayer();
                }
            }

            leaks = FindObjectsByType<MemoryLeakEnemy>();
            foreach (MemoryLeakEnemy leak in leaks)
            {
                Renderer rend = leak.GetComponentInChildren<Renderer>();
                leak.Initialize(integrity, session, rend != null ? rend.material : null);
                if (leak.Variant == LeakVariant.Boss)
                {
                    leak.OnDied += _ =>
                    {
                        quests?.NotifyDungeonCleared("KernelGate");
                        session.NotifyVictory();
                        save?.EndSession(true);
                    };
                }
            }

            NullBorn.Demo.TutorialDirector.Instance?.ShowRaw(
                "Runtime Plaza. Shift sprint · Espacio dash · Click combo · G Purga · V Restaurar · Q lock · E interactuar · Tab inventario",
                8f);

            session.StateChanged += state =>
            {
                if (state != GameSession.State.Playing)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    // Derrota: notificar API sin guardar HP 0.
                    if (state == GameSession.State.Defeat) save?.EndSession(false);
                }
            };
        }

        private void UpdateHudTarget()
        {
            if (hud == null || playerTransform == null) return;
            LockOnController lockOn = playerTransform.GetComponent<LockOnController>();
            if (lockOn != null && lockOn.CurrentTarget != null)
            {
                hud.TrackLeak(lockOn.CurrentTarget.GetComponent<MemoryLeakEnemy>());
                return;
            }

            leaks = FindObjectsByType<MemoryLeakEnemy>();
            MemoryLeakEnemy nearest = null;
            float best = 80f;
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

        private void PushWorldHud()
        {
            if (hud == null) return;
            hud.SetRegion(WorldMapController.Instance != null ? WorldMapController.Instance.CurrentRegionName : "Runtime Plaza");
            hud.SetQuestTracker(QuestManager.Instance != null ? QuestManager.Instance.TrackerLine() : string.Empty);
            RamPressure ram = RamPressure.Instance;
            if (ram != null) hud.SetRam(ram.Normalized, ram.IsCritical);
            CpuCredits credits = CpuCredits.Instance;
            PlayerProgression prog = PlayerProgression.Instance;
            hud.SetRpgStats(prog != null ? prog.Level : 1, prog != null ? prog.XpNormalized : 0f, credits != null ? credits.Amount : 0);
            PlayerInventory inv = PlayerInventory.Instance;
            hud.SetInventoryPanel(inv != null && inv.PanelOpen ? inv.PanelText() : string.Empty);
        }
    }
}
