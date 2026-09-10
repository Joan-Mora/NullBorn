using NullBorn.Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace NullBorn.Game.Map
{
    public class NavigationCompass : MonoBehaviour
    {
        [SerializeField] GameSyncClient sync;
        [SerializeField] Transform player;
        [SerializeField] RectTransform needle;
        [SerializeField] RectTransform distanceLabelRoot;
        [SerializeField] Text distanceLabel;
        [SerializeField] CanvasGroup canvasGroup;

        MarkerDto active;
        int lastDistanceMeters = int.MinValue;
        string lastLabel;
        float lastRelative = float.NaN;
        float lastAlpha = -1f;
        int frameGate;

        void Awake()
        {
            if (sync == null)
                sync = GetComponent<GameSyncClient>() ?? FindAnyObjectByType<GameSyncClient>();
            if (player == null)
            {
                var p = GameObject.Find("Player");
                if (p != null)
                {
                    var visual = p.transform.Find("Visual");
                    player = visual != null ? visual : p.transform;
                }
            }
        }

        void OnEnable()
        {
            if (sync != null)
                sync.MarkersUpdated += OnMarkers;
        }

        void OnDisable()
        {
            if (sync != null)
                sync.MarkersUpdated -= OnMarkers;
        }

        void OnMarkers(MarkerDto[] markers)
        {
            active = null;
            foreach (var m in markers)
            {
                if (!m.isActive) continue;
                active = m;
                break;
            }
        }

        void LateUpdate()
        {
            frameGate++;
            if ((frameGate & 2) != 0) return;

            var show = active != null && player != null;
            if (canvasGroup != null)
            {
                var targetAlpha = show ? 1f : 0f;
                var next = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * 6f);
                if (Mathf.Abs(next - lastAlpha) > 0.01f)
                {
                    canvasGroup.alpha = next;
                    lastAlpha = next;
                }
            }

            if (!show || needle == null) return;

            var to = new Vector3(active.x, player.position.y, active.z) - player.position;
            to.y = 0f;
            var dist = to.magnitude;
            if (dist < 0.05f)
            {
                if (distanceLabel != null && lastDistanceMeters != 0)
                {
                    distanceLabel.text = "Aquí";
                    lastDistanceMeters = 0;
                    lastLabel = null;
                }
                return;
            }

            var worldYaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
            var relative = Mathf.DeltaAngle(player.eulerAngles.y, worldYaw);
            if (float.IsNaN(lastRelative) || Mathf.Abs(Mathf.DeltaAngle(lastRelative, relative)) > 1.5f)
            {
                needle.localRotation = Quaternion.Euler(0f, 0f, -relative);
                lastRelative = relative;
            }

            if (distanceLabel == null) return;
            var meters = Mathf.RoundToInt(dist);
            var label = active.label;
            if (meters == lastDistanceMeters && label == lastLabel) return;
            lastDistanceMeters = meters;
            lastLabel = label;
            distanceLabel.text = meters < 100 ? meters + " m → " + label : meters + " m";
        }

        public void SetPlayer(Transform t) => player = t;
        public void SetNeedle(RectTransform t) => needle = t;
        public void SetDistanceLabel(Text t) => distanceLabel = t;
        public void SetCanvasGroup(CanvasGroup g) => canvasGroup = g;

        public void SetSync(GameSyncClient client)
        {
            if (sync != null)
                sync.MarkersUpdated -= OnMarkers;
            sync = client;
            if (sync != null && isActiveAndEnabled)
                sync.MarkersUpdated += OnMarkers;
        }
    }
}
