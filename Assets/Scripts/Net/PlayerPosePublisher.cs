using System.Collections;
using UnityEngine;

namespace NullBorn.Game.Net
{
    public class PlayerPosePublisher : MonoBehaviour
    {
        [SerializeField] GameSyncClient sync;
        [SerializeField] Transform poseSource;
        [SerializeField] NullBornApiConfig config;

        bool publishing;

        void Awake() => ResolveRefs();

        void OnEnable()
        {
            ResolveRefs();
            StartCoroutine(PublishLoop());
        }

        void OnDisable()
        {
            StopAllCoroutines();
            publishing = false;
        }

        void ResolveRefs()
        {
            if (sync == null) sync = GetComponent<GameSyncClient>();
            if (config == null) config = GetComponent<NullBornApiConfig>();
            if (config == null && sync != null)
                config = sync.GetComponent<NullBornApiConfig>();
            if (poseSource == null)
            {
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    // Prefer root — Visual yaw may differ; pose is world position + body facing.
                    poseSource = player.transform;
                }
                else
                {
                    poseSource = transform;
                }
            }
        }

        IEnumerator PublishLoop()
        {
            yield return null;
            if (!isActiveAndEnabled) yield break;
            ResolveRefs();

            float elapsed = 0f;
            float lastX = float.NaN, lastZ = float.NaN, lastYaw = float.NaN;

            while (isActiveAndEnabled)
            {
                var interval = config != null ? config.PoseInterval : 0.35f;
                elapsed += Time.unscaledDeltaTime;
                if (elapsed < interval)
                {
                    yield return null;
                    continue;
                }

                elapsed = 0f;
                if (sync == null || poseSource == null)
                {
                    ResolveRefs();
                    yield return null;
                    continue;
                }

                // Critical: never open HTTP while API is down — each failed connect hitch spikes FPS.
                if (!sync.IsOnline || publishing)
                {
                    yield return null;
                    continue;
                }

                var p = poseSource.position;
                var yaw = poseSource.eulerAngles.y;
                if (!float.IsNaN(lastX)
                    && (p.x - lastX) * (p.x - lastX) + (p.z - lastZ) * (p.z - lastZ) < 0.04f
                    && Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw)) < 8f)
                {
                    yield return null;
                    continue;
                }

                lastX = p.x;
                lastZ = p.z;
                lastYaw = yaw;
                yield return PublishOnce(p.x, p.z, yaw);
            }
        }

        IEnumerator PublishOnce(float x, float z, float yaw)
        {
            if (sync == null || !isActiveAndEnabled || !sync.IsOnline)
                yield break;

            publishing = true;
            yield return sync.PublishPose(x, z, yaw);
            publishing = false;
        }

        public void SetPoseSource(Transform t) => poseSource = t;
    }
}
