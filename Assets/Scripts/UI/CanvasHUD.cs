using NullBorn.Core;
using NullBorn.Demo;
using NullBorn.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace NullBorn.UI
{
    /// <summary>
    /// Construye un Canvas uGUI en runtime y lo alimenta cada frame.
    /// </summary>
    public class CanvasHUD : MonoBehaviour
    {
        private const float RefWidth = 1920f;
        private const float RefHeight = 1080f;

        private SystemIntegrity integrity;
        private CpuCycles cpu;
        private GameSession session;
        private MemoryLeakEnemy trackedLeak;

        private Image integrityFill;
        private Image cpuFill;
        private Image leakFill;
        private Text integrityLabel;
        private Text cpuLabel;
        private Text leakLabel;
        private Text tipLabel;
        private Text zoneLabel;
        private Text centerLabel;
        private GameObject leakRow;
        private GameObject centerPanel;
        private bool built;
        private bool paused;

        public void Bind(SystemIntegrity systemIntegrity, CpuCycles cpuCycles, GameSession gameSession)
        {
            integrity = systemIntegrity;
            cpu = cpuCycles;
            session = gameSession;
            EnsureCanvas();
        }

        public void TrackLeak(MemoryLeakEnemy leak)
        {
            trackedLeak = leak;
        }

        private void Update()
        {
            if (!built) return;

            if (Input.GetKeyDown(KeyCode.Escape) &&
                (session == null || session.CurrentState == GameSession.State.Playing))
            {
                paused = !paused;
                Time.timeScale = paused ? 0f : 1f;
                Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = paused;
            }

            float i = integrity != null ? integrity.Normalized : 0f;
            float c = cpu != null ? cpu.Normalized : 0f;
            integrityFill.fillAmount = i;
            cpuFill.fillAmount = c;
            integrityLabel.text = $"Integridad del Sistema: {i * 100f:0}%";
            cpuLabel.text = $"Ciclos de CPU: {c * 100f:0}%";

            bool showLeak = trackedLeak != null && trackedLeak.IsAlive;
            leakRow.SetActive(showLeak);
            if (showLeak)
            {
                leakFill.fillAmount = trackedLeak.NormalizedHealth;
                string name = trackedLeak.Variant == LeakVariant.Boss
                    ? (trackedLeak.IsEnraged ? "LEAK BOSS [ENRAGE]" : "LEAK BOSS")
                    : "Memory Leak";
                leakLabel.text = $"{name}: {trackedLeak.NormalizedHealth * 100f:0}%";
            }

            string zone = DemoFlowController.Instance != null
                ? ZoneDisplayName(DemoFlowController.Instance.CurrentZone)
                : "-";
            float t = session != null ? session.ElapsedSeconds : 0f;
            zoneLabel.text = $"Zona: {zone}   Tiempo: {t:0}s   |  WASD · Mouse · Click/F · E/G GC · Esc · R";

            string tip = TutorialDirector.Instance != null ? TutorialDirector.Instance.CurrentMessage : string.Empty;
            tipLabel.text = tip;
            tipLabel.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(tip));

            if (paused)
            {
                ShowCenter("PAUSA\nEsc para continuar");
            }
            else if (session != null && session.CurrentState == GameSession.State.Defeat)
            {
                ShowCenter("SYSTEM COLLAPSE\nR = respawn en checkpoint");
            }
            else if (session != null && session.CurrentState == GameSession.State.Victory)
            {
                ShowCenter("SYSTEM STABILIZED\nKernel contenido\nR = reiniciar demo");
            }
            else
            {
                centerPanel.SetActive(false);
            }
        }

        private static string ZoneDisplayName(DemoZoneId zone)
        {
            switch (zone)
            {
                case DemoZoneId.BootSector: return "Boot Sector";
                case DemoZoneId.HeapCorrupt: return "Heap Corrupto";
                case DemoZoneId.KernelCore: return "Kernel Core";
                default: return zone.ToString();
            }
        }

        private void ShowCenter(string msg)
        {
            centerPanel.SetActive(true);
            centerLabel.text = msg;
        }

        private void EnsureCanvas()
        {
            if (built) return;
            built = true;

            GameObject canvasGo = new GameObject("CanvasHUD");
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            integrityFill = CreateBar(canvasGo.transform, "Integrity", new Vector2(24, -24), new Color(0.2f, 0.85f, 0.45f), out integrityLabel);
            cpuFill = CreateBar(canvasGo.transform, "CPU", new Vector2(24, -62), new Color(0.25f, 0.55f, 1f), out cpuLabel);

            leakRow = new GameObject("LeakRow");
            leakRow.transform.SetParent(canvasGo.transform, false);
            RectTransform leakRowRt = leakRow.AddComponent<RectTransform>();
            leakRowRt.anchorMin = new Vector2(0, 1);
            leakRowRt.anchorMax = new Vector2(0, 1);
            leakRowRt.pivot = new Vector2(0, 1);
            leakRowRt.anchoredPosition = Vector2.zero;
            leakRowRt.sizeDelta = new Vector2(360, 36);
            leakFill = CreateBar(leakRow.transform, "Leak", new Vector2(24, -100), new Color(0.95f, 0.25f, 0.35f), out leakLabel);

            zoneLabel = CreateText(canvasGo.transform, "Zone", new Vector2(24, -142), 20, TextAnchor.UpperLeft, new Vector2(1200, 32));

            GameObject tipBox = CreatePanel(canvasGo.transform, "TipBox", new Vector2(0.5f, 0f), new Vector2(0, 80), new Vector2(1100, 96), new Color(0f, 0f, 0f, 0.72f));
            tipLabel = CreateText(tipBox.transform, "Tip", Vector2.zero, 22, TextAnchor.MiddleCenter, new Vector2(1060, 80));
            StretchFill(tipLabel.rectTransform, 12f, 8f);

            centerPanel = CreatePanel(canvasGo.transform, "Center", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 160), new Color(0f, 0f, 0f, 0.78f));
            centerLabel = CreateText(centerPanel.transform, "CenterText", Vector2.zero, 28, TextAnchor.MiddleCenter, new Vector2(480, 140));
            StretchFill(centerLabel.rectTransform, 16f, 12f);
            centerPanel.SetActive(false);
        }

        private static Image CreateBar(Transform parent, string name, Vector2 anchoredPos, Color fillColor, out Text label)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(360, 32);

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);

            GameObject fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(root.transform, false);
            RectTransform frt = fillGo.AddComponent<RectTransform>();
            StretchFill(frt, 2f, 2f);
            Image fill = fillGo.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;

            label = CreateText(root.transform, "Label", Vector2.zero, 17, TextAnchor.MiddleLeft, new Vector2(340, 32));
            StretchFill(label.rectTransform, 10f, 2f);
            return fill;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static Text CreateText(Transform parent, string name, Vector2 pos, int fontSize, TextAnchor anchor, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();

            bool topLeft = anchor == TextAnchor.UpperLeft || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.LowerLeft;
            bool top = anchor == TextAnchor.UpperLeft || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.UpperRight;
            bool centerH = anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight;
            bool centerV = anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight;

            if (centerH && centerV)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }
            else if (topLeft)
            {
                rt.anchorMin = new Vector2(0, top ? 1f : 0.5f);
                rt.anchorMax = new Vector2(0, top ? 1f : 0.5f);
                rt.pivot = new Vector2(0, top ? 1f : 0.5f);
            }
            else
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
            }

            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            return text;
        }

        private static void StretchFill(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }
    }
}
