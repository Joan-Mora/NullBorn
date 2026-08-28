using UnityEngine;

namespace NullBorn.Demo
{
    /// <summary>
    /// Volumen que muestra un tip de tutorial al cruzarlo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TutorialVolume : MonoBehaviour
    {
        public string tipId;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<NullBorn.Player.ThirdPersonController>() == null) return;
            if (!string.IsNullOrEmpty(tipId))
            {
                TutorialDirector.Instance?.Show(tipId);
            }
        }
    }
}
