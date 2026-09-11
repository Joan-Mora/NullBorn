using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using NullBorn.Game.Map;

namespace NullBorn.Game
{
    /// <summary>
    /// Pause without timeScale=0 (that breaks UI). Esc toggles; listeners bind at runtime
    /// because AddListener from the scene builder does not persist in the .unity file.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] Slider sensitivitySlider;
        [SerializeField] Text sensitivityValueLabel;
        [SerializeField] Text schemeLabel;
        [SerializeField] Image withMouseHighlight;
        [SerializeField] Image keyboardHighlight;
        [SerializeField] Button withMouseButton;
        [SerializeField] Button keyboardButton;
        [SerializeField] Button minusButton;
        [SerializeField] Button plusButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button quitButton;

        public static bool IsPaused { get; private set; }

        SimpleFollowCamera followCamera;
        MapHudController mapHud;
        bool wasCursorLocked;
        bool listenersBound;
        readonly Color highlightOn = new Color(0.76f, 0.47f, 0.23f, 1f);
        readonly Color highlightOff = new Color(0.18f, 0.22f, 0.18f, 1f);

        public void Wire(
            GameObject panel,
            Slider sensSlider,
            Text sensValue,
            Text schemeValueLabel,
            Image mouseBtnImage,
            Image keyboardBtnImage,
            Button mouseBtn,
            Button keyboardBtn,
            Button minusBtn,
            Button plusBtn,
            Button resumeBtn,
            Button quitBtn = null)
        {
            panelRoot = panel;
            sensitivitySlider = sensSlider;
            sensitivityValueLabel = sensValue;
            schemeLabel = schemeValueLabel;
            withMouseHighlight = mouseBtnImage;
            keyboardHighlight = keyboardBtnImage;
            withMouseButton = mouseBtn;
            keyboardButton = keyboardBtn;
            minusButton = minusBtn;
            plusButton = plusBtn;
            resumeButton = resumeBtn;
            quitButton = quitBtn;
            BindListeners();
            SetPaused(false);
            RefreshAll();
        }

        void Awake()
        {
            followCamera = FindAnyObjectByType<SimpleFollowCamera>();
            mapHud = FindAnyObjectByType<MapHudController>();
            ResolveMissingRefs();
        }

        void Start()
        {
            ResolveMissingRefs();
            BindListeners();
            RefreshAll();
        }

        void OnEnable()
        {
            ResolveMissingRefs();
            BindListeners();
            GameSettings.Changed -= RefreshAll;
            GameSettings.Changed += RefreshAll;
            RefreshAll();
        }

        void OnDisable()
        {
            GameSettings.Changed -= RefreshAll;
        }

        void OnDestroy()
        {
            GameSettings.Changed -= RefreshAll;
            if (IsPaused)
            {
                IsPaused = false;
                GameSettings.Save();
            }
        }

        void OnApplicationQuit() => GameSettings.Save();

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.escapeKey.wasPressedThisFrame) return;

            if (mapHud == null)
                mapHud = FindAnyObjectByType<MapHudController>();
            if (mapHud != null && mapHud.IsFullMapOpen)
                return;

            SetPaused(!IsPaused);
        }

        void ResolveMissingRefs()
        {
            if (panelRoot == null)
            {
                var t = transform.Find("PauseRoot");
                if (t != null) panelRoot = t.gameObject;
            }

            if (panelRoot == null) return;

            var root = panelRoot.transform;
            if (sensitivitySlider == null)
                sensitivitySlider = root.GetComponentInChildren<Slider>(true);
            if (sensitivityValueLabel == null)
            {
                var sensValue = root.Find("PauseCard/SensValue");
                if (sensValue != null) sensitivityValueLabel = sensValue.GetComponent<Text>();
            }
            if (schemeLabel == null)
            {
                var schemeInfo = root.Find("PauseCard/SchemeInfo");
                if (schemeInfo != null) schemeLabel = schemeInfo.GetComponent<Text>();
            }

            withMouseButton ??= FindButton(root, "PauseCard/SchemeRow/WithMouse");
            keyboardButton ??= FindButton(root, "PauseCard/SchemeRow/KeyboardOnly");
            minusButton ??= FindButton(root, "PauseCard/SensRow/Minus");
            plusButton ??= FindButton(root, "PauseCard/SensRow/Plus");
            resumeButton ??= FindButton(root, "PauseCard/ResumeBtn");
            quitButton ??= FindButton(root, "PauseCard/QuitBtn");
            EnsureQuitButton();

            if (withMouseHighlight == null && withMouseButton != null)
                withMouseHighlight = withMouseButton.targetGraphic as Image;
            if (keyboardHighlight == null && keyboardButton != null)
                keyboardHighlight = keyboardButton.targetGraphic as Image;
        }

        static Button FindButton(Transform root, string path)
        {
            var t = root.Find(path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        void EnsureQuitButton()
        {
            if (quitButton != null || panelRoot == null) return;
            var card = panelRoot.transform.Find("PauseCard");
            if (card == null) return;

            var resume = card.Find("ResumeBtn") as RectTransform;
            var go = new GameObject("QuitBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(card, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(240f, 44f);
            if (resume != null)
                rt.anchoredPosition = resume.anchoredPosition + new Vector2(0f, -56f);
            else
                rt.anchoredPosition = new Vector2(0f, -400f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.42f, 0.18f, 0.16f, 1f);
            quitButton = go.GetComponent<Button>();
            quitButton.targetGraphic = img;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(rt, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var text = labelGo.GetComponent<Text>();
            var sample = card.GetComponentInChildren<Text>();
            text.font = sample != null ? sample.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "Salir";
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.86f, 0.82f);
            text.raycastTarget = false;

            var cardRt = card as RectTransform;
            if (cardRt != null && cardRt.sizeDelta.y < 530f)
                cardRt.sizeDelta = new Vector2(cardRt.sizeDelta.x, 540f);
        }

        void BindListeners()
        {
            if (!listenersBound)
            {
                if (sensitivitySlider != null)
                {
                    sensitivitySlider.minValue = 0.1f;
                    sensitivitySlider.maxValue = 2f;
                    sensitivitySlider.wholeNumbers = false;
                    sensitivitySlider.onValueChanged.RemoveAllListeners();
                    sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
                }

                BindClick(minusButton, () => NudgeSensitivity(-0.1f));
                BindClick(plusButton, () => NudgeSensitivity(0.1f));
                BindClick(withMouseButton, () => SetScheme(ControlScheme.WithMouse));
                BindClick(keyboardButton, () => SetScheme(ControlScheme.KeyboardOnly));
                BindClick(resumeButton, Resume);

                listenersBound = resumeButton != null && sensitivitySlider != null
                                 && withMouseButton != null && keyboardButton != null;
            }

            BindClick(quitButton, QuitGame);
        }

        static void BindClick(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;

            if (panelRoot != null)
                panelRoot.SetActive(paused);

            if (followCamera == null)
                followCamera = FindAnyObjectByType<SimpleFollowCamera>();

            if (paused)
            {
                wasCursorLocked = followCamera != null && followCamera.IsCursorLocked;
                if (followCamera != null)
                    followCamera.SetCursorLocked(false);
                RefreshAll();
            }
            else
            {
                GameSettings.Save();
                if (followCamera != null)
                    followCamera.SetCursorLocked(GameSettings.UsesMouse);
            }
        }

        public void Resume() => SetPaused(false);

        public void QuitGame()
        {
            GameSettings.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void NudgeSensitivity(float delta)
        {
            ApplySensitivity(GameSettings.MouseSensitivity + delta);
            if (sensitivitySlider != null)
                sensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivity);
        }

        public void SetScheme(ControlScheme scheme)
        {
            GameSettings.Scheme = scheme;
            RefreshSchemeUi();
            wasCursorLocked = GameSettings.UsesMouse;
            if (followCamera != null && !IsPaused)
                followCamera.SetCursorLocked(GameSettings.UsesMouse);
        }

        void RefreshAll()
        {
            if (sensitivitySlider != null)
                sensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivity);
            UpdateSensLabel(GameSettings.MouseSensitivity);
            RefreshSchemeUi();
        }

        void RefreshSchemeUi()
        {
            var mouse = GameSettings.UsesMouse;
            if (schemeLabel != null)
            {
                schemeLabel.text = mouse
                    ? "Estilo: Mouse (A/D giran cuerpo · W camina hacia donde miras)"
                    : "Estilo: Solo teclado (A/D giran la cámara)";
            }

            if (withMouseHighlight != null)
                withMouseHighlight.color = mouse ? highlightOn : highlightOff;
            if (keyboardHighlight != null)
                keyboardHighlight.color = !mouse ? highlightOn : highlightOff;
        }

        void OnSensitivityChanged(float value) => ApplySensitivity(value);

        void ApplySensitivity(float value)
        {
            GameSettings.MouseSensitivity = value;
            UpdateSensLabel(GameSettings.MouseSensitivity);
        }

        void UpdateSensLabel(float value)
        {
            if (sensitivityValueLabel != null)
                sensitivityValueLabel.text = value.ToString("0.00") + "x";
        }
    }
}
