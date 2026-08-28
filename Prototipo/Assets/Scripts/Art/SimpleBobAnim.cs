using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Animación procedural mínima para avatares placeholder (idle bob).
    /// </summary>
    public class SimpleBobAnim : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.05f;
        [SerializeField] private float speed = 2.2f;
        private Vector3 origin;

        private void Awake()
        {
            origin = transform.localPosition;
        }

        private void Update()
        {
            if (Time.timeScale <= 0.01f) return;
            transform.localPosition = origin + Vector3.up * (Mathf.Sin(Time.time * speed) * amplitude);
        }
    }
}
