using NullBorn.Core;
using NullBorn.Demo;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.UI
{
    /// <summary>
    /// HUD del mini-demo. Usa OnGUI dibujado limpio + mensajes de tutorial
    /// (equivalente funcional a Canvas para el vertical slice; fácil de migrar a uGUI).
    /// </summary>
    public class DemoHUD : MonoBehaviour
    {
        private SystemIntegrity integrity;
        private CpuCycles cpu;
        private GameSession session;
        private MemoryLeakEnemy trackedLeak;
        private bool paused;

        public void Bind(SystemIntegrity systemIntegrity, CpuCycles cpuCycles, GameSession gameSession)
        {
            integrity = systemIntegrity;
            cpu = cpuCycles;
            session = gameSession;
        }

        public void TrackLeak(MemoryLeakEnemy leak)
        {
            trackedLeak = leak;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (session != null && session.CurrentState != GameSession.State.Playing)
                {
                    return;
                }

                paused = !paused;
                Time.timeScale = paused ? 0f : 1f;
                Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = paused;
            }
        }

        private void OnGUI()
        {
            DrawBar(20f, 20f, 340f, 28f, "Integridad del Sistema", integrity != null ? integrity.Normalized : 0f, new Color(0.2f, 0.85f, 0.45f));
            DrawBar(20f, 56f, 340f, 28f, "Ciclos de CPU", cpu != null ? cpu.Normalized : 0f, new Color(0.25f, 0.55f, 1f));

            if (trackedLeak != null && trackedLeak.IsAlive)
            {
                string label = trackedLeak.Variant == LeakVariant.Boss
                    ? (trackedLeak.IsEnraged ? "LEAK BOSS [ENRAGE]" : "LEAK BOSS")
                    : "Memory Leak";
                DrawBar(20f, 92f, 340f, 28f, label, trackedLeak.NormalizedHealth, new Color(0.95f, 0.25f, 0.35f));
            }

            string zone = DemoFlowController.Instance != null
                ? DemoFlowController.Instance.CurrentZone.ToString()
                : "-";
            GUI.Label(new Rect(20f, 128f, 480f, 24f), $"Zona: {zone}  |  Tiempo: {(session != null ? session.ElapsedSeconds : 0f):0}s");

            string tip = TutorialDirector.Instance != null ? TutorialDirector.Instance.CurrentMessage : string.Empty;
            if (!string.IsNullOrEmpty(tip))
            {
                GUI.Box(new Rect(20f, Screen.height - 110f, Mathf.Min(900f, Screen.width - 40f), 80f), tip);
            }

            GUI.Label(new Rect(20f, 152f, 800f, 24f), "WASD · Mouse · Click/F depurar · E/G GC · Esc pausa · R reiniciar / respawn");

            if (paused)
            {
                DrawCenter("PAUSA\nEsc continuar", Color.white);
            }

            if (session == null) return;

            if (session.CurrentState == GameSession.State.Defeat)
            {
                DrawCenter("SYSTEM COLLAPSE\nIntegridad en 0.\nR = respawn en checkpoint", new Color(1f, 0.35f, 0.35f));
            }
            else if (session.CurrentState == GameSession.State.Victory)
            {
                DrawCenter("SYSTEM STABILIZED\nKernel contenido.\nR = reiniciar demo", new Color(0.4f, 1f, 0.65f));
            }
        }

        private static void DrawBar(float x, float y, float width, float height, string label, float normalized, Color fill)
        {
            normalized = Mathf.Clamp01(normalized);
            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            Color prev = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(new Rect(x + 2f, y + 2f, (width - 4f) * normalized, height - 4f), Texture2D.whiteTexture);
            GUI.color = prev;
            GUI.Label(new Rect(x + 8f, y + 4f, width, height), $"{label}: {normalized * 100f:0}%");
        }

        private static void DrawCenter(string text, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.Box(new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.5f - 70f, 480f, 140f), text);
            GUI.color = prev;
        }
    }
}
