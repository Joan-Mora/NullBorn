using NullBorn.Core;
using NullBorn.RPG.Quest;
using UnityEngine;

namespace NullBorn.World
{
    /// <summary>
    /// Trigger de bioma: HUD, fog, checkpoint y quest Reach.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RegionTrigger : MonoBehaviour
    {
        [SerializeField] private RegionDefinition definition;
        [SerializeField] private Checkpoint checkpoint;
        [SerializeField] private string reachQuestId;

        public RegionDefinition Definition => definition;

        public void Configure(RegionDefinition def, Checkpoint cp, string reachId = null)
        {
            definition = def;
            checkpoint = cp;
            reachQuestId = reachId;
        }

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (definition == null) return;
            if (!other.CompareTag("Player") && other.GetComponentInParent<NullBorn.Player.ThirdPersonController>() == null)
            {
                return;
            }

            WorldMapController.Instance?.EnterRegion(definition);
            if (checkpoint != null) CheckpointSystem.Instance?.SetCheckpoint(checkpoint);
            if (!string.IsNullOrEmpty(reachQuestId))
            {
                QuestManager.Instance?.NotifyReach(reachQuestId);
            }
        }
    }
}
