using NullBorn.Enemies;
using NullBorn.UI;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Soft lock-on (Q / click rueda). Strafe alrededor del objetivo más centrado.
    /// </summary>
    public class LockOnController : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 18f;
        [SerializeField] private float maxAngle = 70f;

        public Transform CurrentTarget { get; private set; }
        public bool IsLocked => CurrentTarget != null;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(2))
            {
                if (CurrentTarget != null) Clear();
                else Acquire();
            }

            if (CurrentTarget == null) return;

            MemoryLeakEnemy leak = CurrentTarget.GetComponent<MemoryLeakEnemy>();
            if (leak == null || !leak.IsAlive)
            {
                Clear();
                return;
            }

            if (Vector3.Distance(transform.position, CurrentTarget.position) > maxDistance * 1.25f)
            {
                Clear();
            }
        }

        private void Acquire()
        {
            Camera cam = Camera.main;
            MemoryLeakEnemy[] leaks = FindObjectsByType<MemoryLeakEnemy>(FindObjectsSortMode.None);
            MemoryLeakEnemy best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < leaks.Length; i++)
            {
                MemoryLeakEnemy leak = leaks[i];
                if (leak == null || !leak.IsAlive) continue;
                Vector3 to = leak.transform.position - transform.position;
                if (to.magnitude > maxDistance) continue;

                Vector3 view = cam != null ? cam.transform.forward : transform.forward;
                float angle = Vector3.Angle(view, to);
                if (angle > maxAngle) continue;

                float score = angle * 0.6f + to.magnitude * 0.4f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = leak;
                }
            }

            CurrentTarget = best != null ? best.transform : null;
            CanvasHUD hud = FindAnyObjectByType<CanvasHUD>();
            hud?.TrackLeak(best);
        }

        public void Clear()
        {
            CurrentTarget = null;
        }
    }
}
