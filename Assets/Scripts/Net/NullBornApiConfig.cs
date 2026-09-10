using UnityEngine;

namespace NullBorn.Game.Net
{
    public class NullBornApiConfig : MonoBehaviour
    {
        public const string DefaultBaseUrl = "http://127.0.0.1:5080";
        public const string DefaultZoneId = "arttest";
        public const string DefaultPlayerId = "local-player";

        [SerializeField] string baseUrl = DefaultBaseUrl;
        [SerializeField] string zoneId = DefaultZoneId;
        [SerializeField] string playerId = DefaultPlayerId;
        [Tooltip("If false, never opens HTTP — local zone only (no hitch from dead API).")]
        [SerializeField] bool autoConnect = false;
        [SerializeField] float poseHz = 2f;
        [SerializeField] float markersHz = 1f;

        public string BaseUrl => string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.TrimEnd('/');
        public string ZoneId => string.IsNullOrWhiteSpace(zoneId) ? DefaultZoneId : zoneId;
        public string PlayerId => string.IsNullOrWhiteSpace(playerId) ? DefaultPlayerId : playerId;
        public bool AutoConnect => autoConnect;
        public float PoseInterval => 1f / Mathf.Max(0.5f, poseHz);
        public float MarkersInterval => 1f / Mathf.Max(0.5f, markersHz);

        public void Configure(string url, string zone, string player)
        {
            baseUrl = url;
            zoneId = zone;
            playerId = player;
        }

        public void SetRates(float pose, float markers)
        {
            poseHz = pose;
            markersHz = markers;
        }

        public void SetAutoConnect(bool enabled) => autoConnect = enabled;
    }
}
