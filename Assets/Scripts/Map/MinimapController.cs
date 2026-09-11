using System;
using System.Collections;
using NullBorn.Game.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NullBorn.Game.Map
{
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] GameSyncClient sync;
        [SerializeField] RawImage mapImage;
        [SerializeField] RectTransform playerIcon;
        [SerializeField] RectTransform markersRoot;
        [SerializeField] Transform playerTransform;

        ZoneDto zone;
        readonly System.Collections.Generic.Dictionary<string, RectTransform> pins = new();
        readonly System.Collections.Generic.Dictionary<string, Image> pinImages = new();
        Sprite pinSprite;
        Texture2D fallbackTex;
        bool loadingMap;

        public void SetPinSprite(Sprite sprite) => pinSprite = sprite;

        void Awake()
        {
            if (sync == null)
                sync = GetComponent<GameSyncClient>() ?? FindAnyObjectByType<GameSyncClient>();
        }

        void OnEnable()
        {
            if (sync == null) return;
            sync.ZoneLoaded += OnZone;
            sync.MarkersUpdated += OnMarkers;
            if (sync.Zone != null) OnZone(sync.Zone);
            if (sync.LatestMarkers != null) OnMarkers(sync.LatestMarkers);
        }

        void OnDisable()
        {
            StopAllCoroutines();
            loadingMap = false;
            if (sync == null) return;
            sync.ZoneLoaded -= OnZone;
            sync.MarkersUpdated -= OnMarkers;
        }

        void OnDestroy()
        {
            if (fallbackTex != null)
                Destroy(fallbackTex);
        }

        void LateUpdate()
        {
            if (zone == null || playerIcon == null || playerTransform == null) return;

            playerIcon.anchoredPosition = WorldToMinimap(playerTransform.position.x, playerTransform.position.z);
            playerIcon.localRotation = Quaternion.Euler(0f, 0f, -playerTransform.eulerAngles.y);
        }

        void OnZone(ZoneDto z)
        {
            zone = z;
            if (!loadingMap)
                StartCoroutine(LoadMapTexture(z));
        }

        IEnumerator LoadMapTexture(ZoneDto z)
        {
            loadingMap = true;
            if (mapImage != null)
                mapImage.color = Color.white;

            if (z != null && !string.IsNullOrEmpty(z.mapImageUrl) && sync != null
                && !z.mapImageUrl.EndsWith(".svg", System.StringComparison.OrdinalIgnoreCase))
            {
                var cfg = sync.GetComponent<NullBornApiConfig>();
                var baseUrl = cfg != null ? cfg.BaseUrl : NullBornApiConfig.DefaultBaseUrl;
                var url = z.mapImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? z.mapImageUrl
                    : baseUrl + z.mapImageUrl;

                using (var req = UnityWebRequestTexture.GetTexture(url))
                {
                    req.timeout = 3;
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        var tex = DownloadHandlerTexture.GetContent(req);
                        if (mapImage != null && tex != null)
                        {
                            mapImage.texture = tex;
                            loadingMap = false;
                            yield break;
                        }
                    }
                }
            }

            ApplyFallbackTexture();
            loadingMap = false;
        }

        void ApplyFallbackTexture()
        {
            if (fallbackTex == null)
                fallbackTex = ArtTestMapTexture.Build(256);
            if (mapImage != null)
            {
                mapImage.texture = fallbackTex;
                mapImage.color = Color.white;
            }
        }

        void OnMarkers(MarkerDto[] markers)
        {
            if (markersRoot == null || zone == null) return;

            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var m in markers)
            {
                seen.Add(m.id);
                if (!pins.TryGetValue(m.id, out var pin) || pin == null)
                {
                    pin = CreatePin(m);
                    pins[m.id] = pin;
                }

                pin.anchoredPosition = WorldToMinimap(m.x, m.z);
                if (pinImages.TryGetValue(m.id, out var img) && img != null)
                {
                    if (!ColorUtility.TryParseHtmlString(m.color, out var c))
                        c = new Color(0.91f, 0.36f, 0.02f);
                    img.color = m.isActive ? Color.Lerp(c, Color.white, 0.4f) : c;
                }

                pin.localScale = Vector3.one * (m.isActive ? 1.35f : 1f);
            }

            var stale = new System.Collections.Generic.List<string>();
            foreach (var kv in pins)
            {
                if (!seen.Contains(kv.Key))
                    stale.Add(kv.Key);
            }

            foreach (var id in stale)
            {
                if (pins.TryGetValue(id, out var pin) && pin != null)
                    Destroy(pin.gameObject);
                pins.Remove(id);
                pinImages.Remove(id);
            }
        }

        RectTransform CreatePin(MarkerDto m)
        {
            var go = new GameObject("Pin_" + m.label, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(markersRoot, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(14f, 14f);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            if (pinSprite != null)
                img.sprite = pinSprite;
            pinImages[m.id] = img;
            return rt;
        }

        Vector2 WorldToMinimap(float x, float z)
        {
            var parent = markersRoot != null ? markersRoot : (RectTransform)transform;
            var size = parent.rect.size;
            var u = Mathf.InverseLerp(zone.minX, zone.maxX, x);
            var v = Mathf.InverseLerp(zone.minZ, zone.maxZ, z);
            return new Vector2((u - 0.5f) * size.x, (v - 0.5f) * size.y);
        }

        public void Wire(GameSyncClient client, RawImage map, RectTransform playerIconRt, RectTransform markers, Transform player)
        {
            if (sync != null)
            {
                sync.ZoneLoaded -= OnZone;
                sync.MarkersUpdated -= OnMarkers;
            }

            sync = client;
            mapImage = map;
            playerIcon = playerIconRt;
            markersRoot = markers;
            playerTransform = player;

            if (mapImage != null)
                ApplyFallbackTexture();

            if (sync != null && isActiveAndEnabled)
            {
                sync.ZoneLoaded += OnZone;
                sync.MarkersUpdated += OnMarkers;
                if (sync.Zone != null) OnZone(sync.Zone);
                if (sync.LatestMarkers != null) OnMarkers(sync.LatestMarkers);
            }
        }
    }
}
