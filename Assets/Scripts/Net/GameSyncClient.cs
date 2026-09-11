using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace NullBorn.Game.Net
{
    /// <summary>
    /// Companion sync. Offline-first: local zone is ready immediately.
    /// One short connect probe; on failure stays offline for the whole Play session
    /// (no repeating HTTP timeouts that hitch the game on movement).
    /// </summary>
    public class GameSyncClient : MonoBehaviour
    {
        [SerializeField] NullBornApiConfig config;

        public event System.Action<MarkerDto[]> MarkersUpdated;
        public event System.Action<ZoneDto> ZoneLoaded;

        public MarkerDto[] LatestMarkers { get; private set; } = System.Array.Empty<MarkerDto>();
        public ZoneDto Zone { get; private set; }
        public bool IsOnline { get; private set; }

        UnityWebRequest activeRequest;
        UnityWebRequest poseRequest;
        bool sessionGaveUp;
        bool loggedOffline;

        void Awake()
        {
            EnsureConfig();
            EnsureLocalZone();
        }

        void OnEnable()
        {
            EnsureConfig();
            EnsureLocalZone();
            StartCoroutine(BootLoop());
        }

        void OnDisable()
        {
            AbortActiveRequest();
            StopAllCoroutines();
        }

        void OnDestroy()
        {
            AbortActiveRequest();
            MarkersUpdated = null;
            ZoneLoaded = null;
        }

        void AbortActiveRequest()
        {
            AbortOne(ref activeRequest);
            AbortOne(ref poseRequest);
        }

        static void AbortOne(ref UnityWebRequest req)
        {
            if (req == null) return;
            try
            {
                if (!req.isDone)
                    req.Abort();
            }
            catch { /* domain reload */ }

            try { req.Dispose(); }
            catch { /* ignored */ }

            req = null;
        }

        NullBornApiConfig EnsureConfig()
        {
            if (config == null)
                config = GetComponent<NullBornApiConfig>();
            if (config == null)
                config = gameObject.AddComponent<NullBornApiConfig>();
            return config;
        }

        void EnsureLocalZone()
        {
            if (Zone != null) return;
            var cfg = EnsureConfig();
            Zone = new ZoneDto
            {
                id = cfg.ZoneId,
                name = "ArtTest (local)",
                minX = -40f,
                maxX = 40f,
                minZ = -40f,
                maxZ = 40f,
                mapImageUrl = ""
            };
            ZoneLoaded?.Invoke(Zone);
        }

        void GiveUpOffline(string reason)
        {
            IsOnline = false;
            sessionGaveUp = true;
            EnsureLocalZone();
            if (loggedOffline) return;
            loggedOffline = true;
            Debug.Log(
                "[NullBorn] Companion API no disponible (" + reason + "). " +
                "Jugando en modo local (sin red). Para sync: en Server/ corre " +
                "`docker compose up -d` y `dotnet run` en NullBorn.Api → " + EnsureConfig().BaseUrl);
        }

        IEnumerator BootLoop()
        {
            var cfg = EnsureConfig();
            EnsureLocalZone();

            if (cfg == null || !cfg.AutoConnect)
            {
                GiveUpOffline("autoConnect=false");
                yield break;
            }

            yield return FetchHealthProbe();
            if (!IsOnline)
            {
                GiveUpOffline("API no responde en " + cfg.BaseUrl);
                yield break;
            }

            yield return FetchZoneProbe();
            yield return FetchMarkers();

            Debug.Log(
                "[NullBorn] Companion API online → " + cfg.BaseUrl
                + " zona=" + cfg.ZoneId
                + " · waypoints=" + LatestMarkers.Length
                + ". Pins del teléfono aparecen en el mundo / minimapa.");

            while (isActiveAndEnabled && IsOnline && !sessionGaveUp)
            {
                yield return new WaitForSecondsRealtime(cfg.MarkersInterval);
                if (!isActiveAndEnabled || sessionGaveUp || !IsOnline)
                    yield break;
                yield return FetchMarkers();
            }
        }

        IEnumerator FetchHealthProbe()
        {
            var cfg = EnsureConfig();
            if (cfg == null || !isActiveAndEnabled)
                yield break;

            var req = UnityWebRequest.Get($"{cfg.BaseUrl}/api/health");
            req.timeout = 2;
            activeRequest = req;
            yield return req.SendWebRequest();
            if (activeRequest == req)
                activeRequest = null;

            if (!isActiveAndEnabled)
            {
                SafeDispose(req);
                yield break;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                SafeDispose(req);
                IsOnline = false;
                yield break;
            }

            IsOnline = true;
            SafeDispose(req);
        }

        public IEnumerator PublishPose(float x, float z, float yaw)
        {
            if (sessionGaveUp || !IsOnline)
                yield break;

            // Don't stack PUTs — one in flight. API broadcasts PlayerPoseUpdated over SignalR.
            if (poseRequest != null && !poseRequest.isDone)
                yield break;

            var cfg = EnsureConfig();
            if (cfg == null || !isActiveAndEnabled)
                yield break;

            var body = JsonUtility.ToJson(new UpdatePlayerPoseRequest { x = x, z = z, yaw = yaw });
            var url = $"{cfg.BaseUrl}/api/player/{UnityWebRequest.EscapeURL(cfg.PlayerId)}";
            var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 1;

            poseRequest = req;
            yield return req.SendWebRequest();
            if (poseRequest == req)
                poseRequest = null;

            if (!isActiveAndEnabled)
            {
                SafeDispose(req);
                yield break;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                SafeDispose(req);
                GiveUpOffline(req.error);
                yield break;
            }

            SafeDispose(req);
        }

        IEnumerator FetchZoneProbe()
        {
            var cfg = EnsureConfig();
            if (cfg == null || !isActiveAndEnabled)
                yield break;

            var req = UnityWebRequest.Get($"{cfg.BaseUrl}/api/zones/{UnityWebRequest.EscapeURL(cfg.ZoneId)}");
            req.timeout = 2;
            activeRequest = req;
            yield return req.SendWebRequest();
            if (activeRequest == req)
                activeRequest = null;

            if (!isActiveAndEnabled)
            {
                SafeDispose(req);
                yield break;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                SafeDispose(req);
                yield break;
            }

            var remote = JsonUtility.FromJson<ZoneDto>(req.downloadHandler.text);
            SafeDispose(req);
            if (remote != null)
            {
                Zone = remote;
                ZoneLoaded?.Invoke(Zone);
            }
        }

        IEnumerator FetchMarkers()
        {
            if (sessionGaveUp || !IsOnline)
                yield break;

            var cfg = EnsureConfig();
            if (cfg == null || !isActiveAndEnabled)
                yield break;

            var req = UnityWebRequest.Get($"{cfg.BaseUrl}/api/markers?zoneId={UnityWebRequest.EscapeURL(cfg.ZoneId)}");
            req.timeout = 1;
            activeRequest = req;
            yield return req.SendWebRequest();
            if (activeRequest == req)
                activeRequest = null;

            if (!isActiveAndEnabled)
            {
                SafeDispose(req);
                yield break;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                SafeDispose(req);
                GiveUpOffline(req.error);
                yield break;
            }

            LatestMarkers = JsonHelper.FromJsonArray(req.downloadHandler.text);
            SafeDispose(req);
            MarkersUpdated?.Invoke(LatestMarkers);
        }

        static void SafeDispose(UnityWebRequest req)
        {
            if (req == null) return;
            try { req.Dispose(); }
            catch { /* domain reload */ }
        }
    }
}
