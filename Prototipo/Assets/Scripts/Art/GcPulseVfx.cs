using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Flash VFX simple al ejecutar GC.
    /// </summary>
    public class GcPulseVfx : MonoBehaviour
    {
        public static void Play(Vector3 position, float radius = 7f)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "VFX_GC_Pulse";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.2f;
            Object.Destroy(go.GetComponent<Collider>());

            var rend = go.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Color c = new Color(0.3f, 0.95f, 1f, 0.35f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            mat.color = c;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f) * 2f);
            }
            rend.material = mat;

            go.AddComponent<GcPulseVfx>().radius = radius;
        }

        private float radius = 7f;
        private float t;

        private void Update()
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.45f;
            transform.localScale = Vector3.one * Mathf.Lerp(0.2f, radius * 2f, p);
            if (p >= 1f) Destroy(gameObject);
        }
    }
}
