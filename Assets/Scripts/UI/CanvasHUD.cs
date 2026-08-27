using NullBorn.Core;
using NullBorn.Demo;
using NullBorn.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace NullBorn.UI
{
    /// <summary>
    /// Canvas uGUI en runtime. Vitalidad/Energía + overlays de mundo (quest, RAM, inventario).
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
        private Image ramFill;
        private Text integrityLabel;
        private Text cpuLabel;
        private Text leakLabel;
        private Text ramLabel;
        private Text tipLabel;
        private Text zoneLabel;
        private Text centerLabel;
        private Text questLabel;
        private Text rpgLabel;
        private Text promptLabel;
        private Text dialogueLabel;
        private Text inventoryLabel;
        private Text minimapLabel;
        private GameObject leakRow;
        private GameObject ramRow;
        private GameObject centerPanel;
        private GameObject dialoguePanel;
        private GameObject inventoryPanel;
        private GameObject minimapPanel;
        private bool built;
        private bool paused;
        private bool worldMode;
        private bool energyFlash;
        private float energyFlashUntil;
        private string regionName = "-";
        private string questText = string.Empty;
        private string promptText = string.Empty;
        private string dialogueText = string.Empty;
        private string inventoryText = string.Empty;
        private int threadLevel = 1;
        private float xpNorm;
        private int credits;
        private float ramNorm;
        private bool ramAlert;

        public void Bind(SystemIntegrity systemIntegrity, CpuCycles cpuCycles, GameSession gameSession)
        {
            integrity = systemIntegrity;
            cpu = cpuCycles;
            session = gameSession;
            EnsureCanvas();
        }

        public void TrackLeak(MemoryLeakEnemy leak) => trackedLeak = leak;
        public void SetWorldMode(bool enabled) => worldMode = enabled;
        public void SetRegion(string name) => regionName = string.IsNullOrEmpty(name) ? "-" : name;
        public void SetQuestTracker(string text) => questText = text ?? string.Empty;
        public void SetInteractPrompt(string text) => promptText = text ?? string.Empty;
        public void SetDialogue(string text) => dialogueText = text ?? string.Empty;
        public void SetInventoryPanel(string text) => inventoryText = text ?? string.Empty;
        public void SetRam(float normalized, bool alert)
        {
            ramNorm = normalized;
            ramAlert = alert;
        }

        public void SetRpgStats(int level, float xpNormalized, int cpuCredits)
        {
            threadLevel = level;
            xpNorm = xpNormalized;
            credits = cpuCredits;
        }

        public void NotifyEnergyDenied()
        {
            energyFlash = true;
            energyFlashUntil = Time.unscaledTime + 0.35f;
        }

        private void Update()
        {
            if (!built) return;

            if (Input.GetKeyDown(KeyCode.Escape) &&
                (session == null || session.CurrentState == GameSession.State.Playing) &&
                string.IsNullOrEmpty(dialogueText))
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
            integrityLabel.text = $"Vitalidad  {i * 100f:0}%";
            cpuLabel.text = $"Energía  {c * 100f:0}%";
            if (energyFlash && Time.unscaledTime < energyFlashUntil && cpuFill != null)
            {
                cpuFill.color = Color.Lerp(new Color(0.25f, 0.55f, 1f), Color.white, Mathf.PingPong(Time.unscaledTime * 12f, 1f));
            }
            else if (cpuFill != null)
            {
                cpuFill.color = new Color(0.25f, 0.55f, 1f);
                energyFlash = false;
            }

            bool showLeak = trackedLeak != null && trackedLeak.IsAlive;
            leakRow.SetActive(showLeak);
            if (showLeak)
            {
                leakFill.fillAmount = trackedLeak.NormalizedHealth;
                leakLabel.text = $"Objetivo: {trackedLeak.DisplayName}  {trackedLeak.NormalizedHealth * 100f:0}%";
            }

            if (ramRow != null)
            {
                ramRow.SetActive(worldMode);
                if (worldMode && ramFill != null)
                {
                    ramFill.fillAmount = ramNorm;
                    ramFill.color = ramAlert ? new Color(1f, 0.35f, 0.2f) : new Color(0.95f, 0.7f, 0.2f);
                    ramLabel.text = ramAlert ? $"RAM crítica  {ramNorm * 100f:0}%" : $"RAM  {ramNorm * 100f:0}%";
                }
            }

            string zone = worldMode
                ? regionName
                : (DemoFlowController.Instance != null ? ZoneDisplayName(DemoFlowController.Instance.CurrentZone) : regionName);
            float t = session != null ? session.ElapsedSeconds : 0f;
            if (worldMode)
            {
                zoneLabel.text = $"{zone}   {t:0}s   PT {threadLevel}   Créditos {credits}";
            }
            else
            {
                zoneLabel.text = $"Zona: {zone}   Tiempo: {t:0}s   |  WASD · Shift sprint · Espacio dash · Click combo · E/G GC · Esc · R";
            }

            if (questLabel != null)
            {
                questLabel.text = questText;
                questLabel.gameObject.SetActive(worldMode && !string.IsNullOrEmpty(questText));
            }

            if (rpgLabel != null)
            {
                rpgLabel.text = worldMode ? $"Prioridad {threadLevel}   XP {xpNorm * 100f:0}%" : string.Empty;
            }

            if (promptLabel != null)
            {
                promptLabel.text = promptText;
                promptLabel.gameObject.SetActive(!string.IsNullOrEmpty(promptText));
            }

            if (dialoguePanel != null)
            {
                bool showDlg = !string.IsNullOrEmpty(dialogueText);
                dialoguePanel.SetActive(showDlg);
                if (showDlg) dialogueLabel.text = dialogueText;
            }

            if (inventoryPanel != null)
            {
                bool showInv = !string.IsNullOrEmpty(inventoryText);
                inventoryPanel.SetActive(showInv);
                if (showInv) inventoryLabel.text = inventoryText;
            }

            if (minimapPanel != null)
            {
                minimapPanel.SetActive(worldMode);
                if (worldMode && minimapLabel != null)
                {
                    minimapLabel.text = $"MAPA\n{zone}\n· Plaza\n· Heap N\n· Stack O\n· I/O E\n· Kernel";
                }
            }

            string tip = TutorialDirector.Instance != null ? TutorialDirector.Instance.CurrentMessage : string.Empty;
            tipLabel.text = tip;
            tipLabel.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(tip) && string.IsNullOrEmpty(dialogueText));

            if (paused)
            {
                ShowCenter(worldMode
                    ? "PAUSA\nEsc continuar · F5 guardar · F9 cargar\nClick/F Corte · G Purga · V Restaurar · Q lock · E interactuar"
                    : "PAUSA\nEsc para continuar");
            }
            else if (session != null && session.CurrentState == GameSession.State.Defeat)
            {
                ShowCenter("SYSTEM COLLAPSE\nR = respawn");
            }
            else if (session != null && session.CurrentState == GameSession.State.Victory)
            {
                ShowCenter(worldMode ? "RUNTIME ESTABLE\nArco Kernel contenido\nR = reiniciar" : "SYSTEM STABILIZED\nKernel contenido\nR = reiniciar demo");
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

            ramRow = new GameObject("RamRow");
            ramRow.transform.SetParent(canvasGo.transform, false);
            RectTransform ramRt = ramRow.AddComponent<RectTransform>();
            ramRt.anchorMin = new Vector2(0, 1);
            ramRt.anchorMax = new Vector2(0, 1);
            ramRt.pivot = new Vector2(0, 1);
            ramRt.anchoredPosition = Vector2.zero;
            ramFill = CreateBar(ramRow.transform, "RAM", new Vector2(24, -138), new Color(0.95f, 0.7f, 0.2f), out ramLabel);
            ramRow.SetActive(false);

            zoneLabel = CreateText(canvasGo.transform, "Zone", new Vector2(24, -180), 20, TextAnchor.UpperLeft, new Vector2(1200, 32));
            rpgLabel = CreateText(canvasGo.transform, "Rpg", new Vector2(24, -208), 18, TextAnchor.UpperLeft, new Vector2(600, 28));
            questLabel = CreateText(canvasGo.transform, "Quest", new Vector2(24, -240), 18, TextAnchor.UpperLeft, new Vector2(640, 70));
            promptLabel = CreateText(canvasGo.transform, "Prompt", new Vector2(0, 140), 22, TextAnchor.MiddleCenter, new Vector2(700, 40));
            promptLabel.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            promptLabel.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            promptLabel.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            GameObject tipBox = CreatePanel(canvasGo.transform, "TipBox", new Vector2(0.5f, 0f), new Vector2(0, 80), new Vector2(1100, 96), new Color(0f, 0f, 0f, 0.72f));
            tipLabel = CreateText(tipBox.transform, "Tip", Vector2.zero, 22, TextAnchor.MiddleCenter, new Vector2(1060, 80));
            StretchFill(tipLabel.rectTransform, 12f, 8f);

            centerPanel = CreatePanel(canvasGo.transform, "Center", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 200), new Color(0f, 0f, 0f, 0.78f));
            centerLabel = CreateText(centerPanel.transform, "CenterText", Vector2.zero, 26, TextAnchor.MiddleCenter, new Vector2(600, 180));
            StretchFill(centerLabel.rectTransform, 16f, 12f);
            centerPanel.SetActive(false);

            dialoguePanel = CreatePanel(canvasGo.transform, "Dialogue", new Vector2(0.5f, 0.2f), new Vector2(0, 40), new Vector2(900, 220), new Color(0f, 0.02f, 0.06f, 0.88f));
            dialogueLabel = CreateText(dialoguePanel.transform, "Dlg", Vector2.zero, 22, TextAnchor.UpperLeft, new Vector2(860, 200));
            StretchFill(dialogueLabel.rectTransform, 16f, 12f);
            dialoguePanel.SetActive(false);

            inventoryPanel = CreatePanel(canvasGo.transform, "Inventory", new Vector2(1f, 0.5f), new Vector2(-220, 0), new Vector2(380, 280), new Color(0f, 0.04f, 0.08f, 0.86f));
            inventoryLabel = CreateText(inventoryPanel.transform, "Inv", Vector2.zero, 20, TextAnchor.UpperLeft, new Vector2(360, 260));
            StretchFill(inventoryLabel.rectTransform, 12f, 10f);
            inventoryPanel.SetActive(false);

            minimapPanel = CreatePanel(canvasGo.transform, "Minimap", new Vector2(1f, 1f), new Vector2(-90, -90), new Vector2(160, 160), new Color(0.02f, 0.06f, 0.08f, 0.72f));
            minimapLabel = CreateText(minimapPanel.transform, "Mini", Vector2.zero, 16, TextAnchor.MiddleCenter, new Vector2(150, 150));
            StretchFill(minimapLabel.rectTransform, 8f, 8f);
            minimapPanel.SetActive(false);
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
            text.verticalOverflow = VerticalWrapMode.Overflow;
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
