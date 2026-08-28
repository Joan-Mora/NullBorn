using System.Collections;
using NullBorn.Art;
using NullBorn.UI;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Hitstop, flash de daño, camera shake, SFX procedural y números de daño.
    /// </summary>
    public class CombatFeedback : MonoBehaviour
    {
        public static CombatFeedback Instance { get; private set; }

        private ThirdPersonCamera orbit;
        private Coroutine hitstopRoutine;
        private float cachedScale = 1f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam != null) orbit = cam.GetComponent<ThirdPersonCamera>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BindCamera(ThirdPersonCamera camera)
        {
            orbit = camera;
        }

        public void HitStop(float seconds)
        {
            if (seconds <= 0f) return;
            if (hitstopRoutine != null) StopCoroutine(hitstopRoutine);
            hitstopRoutine = StartCoroutine(HitStopCo(seconds));
        }

        public void Shake(float amount)
        {
            if (orbit == null && Camera.main != null)
            {
                orbit = Camera.main.GetComponent<ThirdPersonCamera>();
            }

            orbit?.AddShake(amount);
        }

        public void Flash(Renderer[] renderers, Color color, float duration = 0.12f)
        {
            if (renderers == null) return;
            StartCoroutine(FlashCo(renderers, color, duration));
        }

        public void DamagePopup(Vector3 position, float amount, bool heavy)
        {
            GameObject go = new GameObject("DmgPopup");
            go.transform.position = position + Vector3.up * 1.6f;
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = Mathf.RoundToInt(amount).ToString();
            tm.fontSize = 42;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = heavy ? new Color(1f, 0.85f, 0.2f) : Color.white;
            StartCoroutine(PopupCo(go));
        }

        public static void NotifyDeny(Vector3 position)
        {
            PlayTone(position, 180f, 0.12f);
            CanvasHUD hud = FindAnyObjectByType<CanvasHUD>();
            hud?.NotifyEnergyDenied();
        }

        public static void PlayHitSfx(Vector3 position, bool heavy)
        {
            PlayTone(position, heavy ? 220f : 340f, 0.08f);
        }

        public static void PlayMissSfx(Vector3 position)
        {
            PlayTone(position, 140f, 0.05f);
        }

        public static void PlayDodgeSfx(Vector3 position)
        {
            PlayTone(position, 520f, 0.07f);
        }

        public static void PlayTone(Vector3 position, float freq, float duration)
        {
            int sampleRate = 22050;
            int samples = Mathf.Max(256, (int)(sampleRate * duration));
            AudioClip clip = AudioClip.Create("nb_tone", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float env = 1f - (i / (float)samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.35f;
            }

            clip.SetData(data, 0);
            AudioSource.PlayClipAtPoint(clip, position, 0.7f);
        }

        private IEnumerator HitStopCo(float seconds)
        {
            if (Time.timeScale <= 0.05f) yield break;
            cachedScale = Time.timeScale > 0.2f ? Time.timeScale : 1f;
            Time.timeScale = 0.08f;
            yield return new WaitForSecondsRealtime(seconds);
            if (NullBorn.Core.GameSession.Instance != null &&
                NullBorn.Core.GameSession.Instance.CurrentState != NullBorn.Core.GameSession.State.Playing)
            {
                yield break;
            }

            Time.timeScale = cachedScale;
            hitstopRoutine = null;
        }

        private static IEnumerator FlashCo(Renderer[] renderers, Color color, float duration)
        {
            Color[] originals = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Material mat = renderers[i].material;
                originals[i] = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : mat.color;
                RuntimeMaterials.ApplyEmission(mat, color * 3f);
                RuntimeMaterials.ApplyColor(mat, color);
            }

            yield return new WaitForSecondsRealtime(duration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                RuntimeMaterials.ApplyEmission(renderers[i].material, originals[i]);
            }
        }

        private static IEnumerator PopupCo(GameObject go)
        {
            float t = 0f;
            Vector3 start = go.transform.position;
            TextMesh tm = go.GetComponent<TextMesh>();
            while (t < 0.7f)
            {
                t += Time.unscaledDeltaTime;
                go.transform.position = start + Vector3.up * (t * 1.4f);
                if (tm != null)
                {
                    Color c = tm.color;
                    c.a = 1f - t / 0.7f;
                    tm.color = c;
                }

                yield return null;
            }

            Destroy(go);
        }
    }
}
