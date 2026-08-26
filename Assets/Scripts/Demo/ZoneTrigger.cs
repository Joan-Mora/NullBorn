using System;
using NullBorn.Core;
using UnityEngine;
using UnityEngine.Events;

namespace NullBorn.Demo
{
    public enum DemoZoneId
    {
        BootSector = 1,
        HeapCorrupt = 2,
        KernelCore = 3
    }

    /// <summary>
    /// Trigger de zona: activa checkpoint, tutorial y spawns.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ZoneTrigger : MonoBehaviour
    {
        [SerializeField] private DemoZoneId zoneId = DemoZoneId.BootSector;
        [SerializeField] private string tutorialId;
        [SerializeField] private Checkpoint checkpoint;
        [SerializeField] private GameObject[] enableOnEnter;
        [SerializeField] private UnityEvent onEnter;

        private bool entered;

        public DemoZoneId ZoneId => zoneId;
        public event Action<ZoneTrigger> Entered;

        public void Configure(DemoZoneId id, string tip, Checkpoint cp, GameObject[] enable = null)
        {
            zoneId = id;
            tutorialId = tip;
            checkpoint = cp;
            enableOnEnter = enable;
        }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (entered) return;
            if (!other.CompareTag("Player") && other.GetComponentInParent<NullBorn.Player.ThirdPersonController>() == null)
            {
                return;
            }

            entered = true;

            if (checkpoint != null && CheckpointSystem.Instance != null)
            {
                CheckpointSystem.Instance.SetCheckpoint(checkpoint);
            }

            if (enableOnEnter != null)
            {
                foreach (GameObject go in enableOnEnter)
                {
                    if (go != null) go.SetActive(true);
                }
            }

            onEnter?.Invoke();
            Entered?.Invoke(this);

            if (!string.IsNullOrEmpty(tutorialId) && TutorialDirector.Instance != null)
            {
                TutorialDirector.Instance.Show(tutorialId);
            }

            DemoFlowController.Instance?.NotifyZoneEntered(zoneId);
        }
    }
}
