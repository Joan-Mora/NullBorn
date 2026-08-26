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
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null) return;
            active = checkpoint;
            Debug.Log($"Checkpoint activo: {checkpoint.ZoneId}");
        }

        public void RespawnPlayer()
        {
            if (player == null || active == null) return;

            Time.timeScale = 1f;

            Vector3 pos = active.SpawnPoint.position;
            Quaternion rot = active.SpawnPoint.rotation;

            if (playerController != null) playerController.enabled = false;
            player.SetPositionAndRotation(pos, rot);
            if (playerController != null) playerController.enabled = true;

            integrity?.ResetIntegrity();
            cpu?.ResetCycles();
        }
    }
}
