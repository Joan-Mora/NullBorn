using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NullBorn.Game
{
    /// <summary>
    /// FPS overlay via uGUI Text (no OnGUI). Updates the label ~1 Hz to avoid GC/UI thrash.
    /// </summary>
    public class SimpleFpsOverlay : MonoBehaviour
    {
        [SerializeField] bool visible = true;
        [SerializeField] float sampleInterval = 1f;

        float sampleTimer;
        int frames;
        float fps = 60f;
        float fpsMin = 999f;
        float displayMin = 60f;

        Text fpsText;
        Text hintText;
        Image background;
        CanvasGroup canvasGroup;
        bool uiReady;

        static readonly Color Good = new Color(0.45f, 1f, 0.55f);
        static readonly Color Mid = new Color(1f, 0.85f, 0.35f);
        static readonly Color Bad = new Color(1f, 0.4f, 0.35f);
        static readonly Color Hint = new Color(0.85f, 0.85f, 0.85f);

        void Awake()
        {
            if (GetComponent<RuntimePerformanceTweaks>() == null)
                gameObject.AddComponent<RuntimePerformanceTweaks>();
            EnsureUi();
            RefreshLabel();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f3Key.wasPressedThisFrame)
            {
                visible = !visible;
                if (canvasGroup != null)
                    canvasGroup.alpha = visible ? 1f : 0f;
            }

            frames++;
            sampleTimer += Time.unscaledDeltaTime;
            var frameFps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            if (frameFps < fpsMin) fpsMin = frameFps;

            if (sampleTimer < sampleInterval) return;

            fps = frames / sampleTimer;
            displayMin = fpsMin;
            frames = 0;
            sampleTimer = 0f;
            fpsMin = 999f;
            RefreshLabel();
        }

        void EnsureUi()
        {
            if (uiReady) return;
            uiReady = true;

            var canvasGo = new GameObject("FpsOverlayCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGroup = canvasGo.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = visible ? 1f : 0f;

            var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 1f);
            bgRt.anchorMax = new Vector2(0f, 1f);
            bgRt.pivot = new Vector2(0f, 1f);
            bgRt.anchoredPosition = new Vector2(12f, -12f);
            bgRt.sizeDelta = new Vector2(300f, 58f);
            background = bgGo.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);
            background.raycastTarget = false;

            fpsText = CreateLabel(bgGo.transform, "Fps", new Vector2(8f, -4f), new Vector2(284f, 28f), 14, FontStyle.Bold);
            hintText = CreateLabel(bgGo.transform, "Hint", new Vector2(8f, -30f), new Vector2(284f, 22f), 11, FontStyle.Normal);
            hintText.color = Hint;
            hintText.text = "F3 · FPS · M mapa";
        }

        static Text CreateLabel(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        void RefreshLabel()
        {
            if (fpsText == null) return;
            var ms = 1000f / Mathf.Max(fps, 0.01f);
            fpsText.text = ((int)fps) + " FPS avg · min " + ((int)displayMin) + " · " + ms.ToString("0.0") + " ms";
            fpsText.color = fps >= 55f ? Good : fps >= 40f ? Mid : Bad;
        }
    }
}
