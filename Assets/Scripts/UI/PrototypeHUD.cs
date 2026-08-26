using NullBorn.Core;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.UI
{
    /// <summary>
    /// HUD simple con OnGUI (sin Canvas) para el primer prototipo.
    /// Más adelante lo cambiaremos a UI Toolkit / Canvas para la entrega final.
    /// </summary>
    public class PrototypeHUD : MonoBehaviour
    {
        private SystemIntegrity integrity;
        private CpuCycles cpu;
        private GameSession session;
        private MemoryLeakEnemy leak;

        public void Bind(SystemIntegrity systemIntegrity, CpuCycles cpuCycles, GameSession gameSession, MemoryLeakEnemy memoryLeak)
        {
            integrity = systemIntegrity;
            cpu = cpuCycles;
            session = gameSession;
            leak = memoryLeak;
        }

        private void OnGUI()
        {
            DrawBar(20f, 20f, 320f, 28f, "Integridad del Sistema", integrity != null ? integrity.Normalized : 0f, new Color(0.2f, 0.85f, 0.45f));
            DrawBar(20f, 58f, 320f, 28f, "Ciclos de CPU", cpu != null ? cpu.Normalized : 0f, new Color(0.25f, 0.55f, 1f));

            float leakHealth = leak != null ? leak.NormalizedHealth : 0f;
            DrawBar(20f, 96f, 320f, 28f, "Memory Leak", leakHealth, new Color(0.95f, 0.25f, 0.35f));

            GUI.Label(new Rect(20f, 136f, 700f, 70f),
                "WASD mover | Mouse mirar | Click/F depurar (cuesta CPU) | E/G Garbage Collection | R reiniciar");

            if (session == null) return;

            GUI.Label(new Rect(20f, 170f, 400f, 30f),
                $"Tiempo: {session.ElapsedSeconds:0.0}s");

            if (session.CurrentState == GameSession.State.Defeat)
            {
                DrawCenterMessage("SYSTEM COLLAPSE\nIntegridad en 0. Presiona R para reiniciar.", new Color(1f, 0.3f, 0.3f));
            }
            else if (session.CurrentState == GameSession.State.Victory)
            {
                DrawCenterMessage("LEAK CONTAINED\n¡Sistema estabilizado! Presiona R para reiniciar.", new Color(0.4f, 1f, 0.6f));
            }
        }

        private static void DrawBar(float x, float y, float width, float height, string label, float normalized, Color fill)
        {
            normalized = Mathf.Clamp01(normalized);
            GUI.Box(new Rect(x, y, width, height), GUIContent.none);

            Texture2D tex = Texture2D.whiteTexture;
            Color prev = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(new Rect(x + 2f, y + 2f, (width - 4f) * normalized, height - 4f), tex);
            GUI.color = prev;
            GUI.Label(new Rect(x + 8f, y + 4f, width, height), $"{label}: {normalized * 100f:0}%");
        }

        private static void DrawCenterMessage(string text, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.Box(new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 60f, 440f, 120f), text);
            GUI.color = prev;
        }
    }
}
