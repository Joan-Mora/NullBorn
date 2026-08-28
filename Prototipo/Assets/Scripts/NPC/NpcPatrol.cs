using UnityEngine;

namespace NullBorn.NPC
{
    /// <summary>
    /// Patrol simple entre waypoints (idle de mundo vivo).
    /// </summary>
    public class NpcPatrol : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 1.4f;
        [SerializeField] private float wait = 1.6f;

        private int index;
        private float waitUntil;

        public void SetWaypoints(Vector3[] points)
        {
            GameObject root = new GameObject("Waypoints");
            root.transform.SetParent(transform.parent, true);
            waypoints = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GameObject wp = new GameObject($"wp_{i}");
                wp.transform.SetParent(root.transform, false);
                wp.transform.position = points[i];
                waypoints[i] = wp.transform;
            }
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;
            if (Time.time < waitUntil) return;

            Transform target = waypoints[index];
            Vector3 to = target.position - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.3f)
            {
                waitUntil = Time.time + wait;
                index = (index + 1) % waypoints.Length;
                return;
            }

            transform.position += to.normalized * speed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to.normalized), 5f * Time.deltaTime);
        }
    }
}
