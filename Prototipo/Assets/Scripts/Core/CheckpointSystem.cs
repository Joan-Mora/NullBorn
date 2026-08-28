using UnityEngine;

namespace NullBorn.Core
{
    /// <summary>
    /// Gestiona el último checkpoint y respawn del jugador.
    /// </summary>
    public class CheckpointSystem : MonoBehaviour
    {
        public static CheckpointSystem Instance { get; private set; }

        private Checkpoint active;
        private Transform player;
        private CharacterController playerController;
        private SystemIntegrity integrity;
        private CpuCycles cpu;
        private Vector3 fallbackSpawn = new Vector3(0f, 1.2f, -4f);

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterPlayer(Transform playerTransform, SystemIntegrity systemIntegrity, CpuCycles cpuCycles)
        {
            player = playerTransform;
            integrity = systemIntegrity;
            cpu = cpuCycles;
            playerController = player.GetComponent<CharacterController>();
            if (player != null) fallbackSpawn = player.position;
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null) return;
            active = checkpoint;
            Debug.Log($"Checkpoint activo: {checkpoint.ZoneId}");
        }

        public void RespawnPlayer()
        {
            if (player == null) return;

            Time.timeScale = 1f;
            GameSession.Instance?.ResumePlaying();

            Vector3 pos = active != null ? active.SpawnPoint.position : fallbackSpawn;
            Quaternion rot = active != null ? active.SpawnPoint.rotation : Quaternion.identity;
            pos = SnapToGround(pos);

            if (playerController != null) playerController.enabled = false;
            player.SetPositionAndRotation(pos, rot);
            if (playerController != null) playerController.enabled = true;

            integrity?.ResetIntegrity();
            cpu?.ResetCycles();
        }

        public static Vector3 SnapToGround(Vector3 pos, float hover = 1.15f)
        {
            Vector3 origin = pos + Vector3.up * 8f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 40f, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * hover;
            }

            if (pos.y < 0.5f || pos.y > 80f)
            {
                pos.y = hover;
            }

            return pos;
        }
    }
}
