using System.Collections;
using UnityEngine;

namespace NullBorn.Game.Net
{
    public class PlayerPosePublisher : MonoBehaviour
    {
        [SerializeField] GameSyncClient sync;
        [SerializeField] Transform poseSource;
        [SerializeField] NullBornApiConfig config;

        const float MoveDeadzoneMeters = 0.01f;
        const float YawDeadzoneDegrees = 1.5f;

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

            var moveDeadzoneSqr = MoveDeadzoneMeters * MoveDeadzoneMeters;

            while (isActiveAndEnabled)
            {
                if (sync == null || poseSource == null)
                {
                    ResolveRefs();
                    yield return null;
                    continue;
                }

                // Critical: never open HTTP while API is down — each failed connect hitch spikes FPS.
                if (!sync.IsOnline)
                {
                    elapsed = 0f;
                    yield return null;
                    continue;
                }

                // One in-flight PUT. Keep elapsed so the next sample fires as soon as it completes.
                if (publishing)
                {
                    yield return null;
                    continue;
                }

                var interval = config != null ? config.PoseInterval : 1f / 14f;
                elapsed += Time.unscaledDeltaTime;
                if (elapsed < interval)
                {
                    yield return null;
                    continue;
                }

                var p = poseSource.position;
                var yaw = poseSource.eulerAngles.y;
                if (!float.IsNaN(lastX)
                    && (p.x - lastX) * (p.x - lastX) + (p.z - lastZ) * (p.z - lastZ) < moveDeadzoneSqr
                    && Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw)) < YawDeadzoneDegrees)
                {
                    elapsed = 0f;
                    yield return null;
                    continue;
                }

                elapsed = 0f;
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
