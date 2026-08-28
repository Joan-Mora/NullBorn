using NullBorn.UI;
using NullBorn.World;
using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Interacción contextual (E). En Demo_10min no se añade: GC sigue en E/G.
    /// </summary>
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField] private float range = 2.8f;

        private IInteractable current;

        private void Update()
        {
            Scan();
            CanvasHUD hud = FindAnyObjectByType<CanvasHUD>();
            hud?.SetInteractPrompt(current != null ? current.Prompt : string.Empty);

            if (current != null && Input.GetKeyDown(KeyCode.E))
            {
                current.Interact(gameObject);
            }
        }

        private void Scan()
        {
            current = null;
            Collider[] hits = Physics.OverlapSphere(transform.position, range);
            float best = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                IInteractable interactable = hits[i].GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract) continue;
                float d = Vector3.Distance(transform.position, hits[i].transform.position);
                if (d < best)
                {
                    best = d;
                    current = interactable;
                }
            }
        }
    }
}
