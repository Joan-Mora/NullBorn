using UnityEngine;

namespace NullBorn.Player
{
    /// <summary>
    /// Puente listo para Animator Mixamo: expone velocidad de movimiento.
    /// Cuando importes el FBX, asigna un Animator y usa el parámetro Speed.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerAnimBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParam = "Speed";

        private CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (animator == null || controller == null) return;
            Vector3 flat = controller.velocity;
            flat.y = 0f;
            animator.SetFloat(speedParam, flat.magnitude);
        }
    }
}
