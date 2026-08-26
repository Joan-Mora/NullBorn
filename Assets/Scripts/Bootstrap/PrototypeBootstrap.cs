using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.Player;
using NullBorn.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.Bootstrap
{
    /// <summary>
    /// Conecta referencias al iniciar Play cuando el escenario YA está en la escena
    /// (previsualizable en el editor). También maneja cursor y reinicio.
    /// </summary>
    public class PrototypeBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            WireGameplay();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private static void WireGameplay()
        {
            GameObject player = GameObject.Find("Player_Thread");
            GameObject leakGo = GameObject.Find("Enemy_MemoryLeak");
            GameObject systems = GameObject.Find("GameSystems");
            Camera mainCam = Camera.main;

            if (player == null || leakGo == null || systems == null || mainCam == null)
            {
                Debug.LogError(
                    "NullBorn: faltan objetos en la escena. En el menú usa " +
                    "NullBorn → Generar escenario previsualizable");
                return;
            }

            SystemIntegrity integrity = player.GetComponent<SystemIntegrity>();
            CpuCycles cpu = player.GetComponent<CpuCycles>();
            ThirdPersonController movement = player.GetComponent<ThirdPersonController>();
            GarbageCollectionAbility gc = player.GetComponent<GarbageCollectionAbility>();
            DebugStrikeAbility strike = player.GetComponent<DebugStrikeAbility>();

            GameSession session = systems.GetComponent<GameSession>();
            PrototypeHUD hud = systems.GetComponent<PrototypeHUD>();
            MemoryLeakEnemy leak = leakGo.GetComponent<MemoryLeakEnemy>();

            ThirdPersonCamera orbit = mainCam.GetComponent<ThirdPersonCamera>();
            if (orbit == null) orbit = mainCam.gameObject.AddComponent<ThirdPersonCamera>();
            orbit.SetTarget(player.transform);

            movement.Initialize(mainCam.transform);
            gc.Initialize(cpu, integrity, session);
            strike.Initialize(cpu, session);
            session.Bind(integrity);

            Renderer leakRenderer = leakGo.GetComponentInChildren<Renderer>();
            // .material crea una instancia en runtime (no ensucia el asset del editor).
            leak.Initialize(integrity, session, leakRenderer != null ? leakRenderer.material : null);
            leak.OnDied += _ => session.NotifyVictory();

            hud.Bind(integrity, cpu, session, leak);

            Debug.Log("NullBorn: escenario enlazado. Listo para jugar.");
        }
    }
}
