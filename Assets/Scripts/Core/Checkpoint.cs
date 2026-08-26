using UnityEngine;

namespace NullBorn.Core
{
    /// <summary>
    /// Punto de reaparición por zona.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private string zoneId = "zone1";
        [SerializeField] private Transform spawnPoint;

        public string ZoneId => zoneId;
        public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;

        public void SetZoneId(string id)
        {
            zoneId = id;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.6f);
            Gizmos.DrawWireSphere(SpawnPoint.position, 0.5f);
            Gizmos.DrawLine(SpawnPoint.position, SpawnPoint.position + SpawnPoint.forward * 1.2f);
        }
    }
}
