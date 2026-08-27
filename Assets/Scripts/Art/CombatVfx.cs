using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Pulso VFX parametrizable (purga magenta vs restore verde-cian).
    /// </summary>
    public class CombatVfx : MonoBehaviour
    {
        public static void PlayPulse(Vector3 position, float radius, Color color, Color emission)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "VFX_Pulse";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.2f;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Lit(color, emission);
            CombatVfx vfx = go.AddComponent<CombatVfx>();
            vfx.radius = radius;
            vfx.mode = 0;
        }

        public static void PlayRestore(Vector3 position)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "VFX_RestoreBit";
                go.transform.position = position + Vector3.up * 0.3f + Random.insideUnitSphere * 0.4f;
                go.transform.localScale = Vector3.one * 0.12f;
                Object.Destroy(go.GetComponent<Collider>());
                go.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Lit(
                    new Color(0.4f, 1f, 0.7f),
                    new Color(0.2f, 1f, 0.6f) * 2f);
                CombatVfx vfx = go.AddComponent<CombatVfx>();
                vfx.mode = 1;
                vfx.delay = i * 0.04f;
            }
        }

        private float radius = 7f;
        private float t;
        private int mode;
        private float delay;

        private void Update()
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            t += Time.unscaledDeltaTime;
            if (mode == 0)
            {
                float p = t / 0.4f;
                transform.localScale = Vector3.one * Mathf.Lerp(0.2f, radius * 2f, p);
                if (p >= 1f) Destroy(gameObject);
            }
            else
            {
                transform.position += Vector3.up * (3.2f * Time.deltaTime);
                if (t >= 0.55f) Destroy(gameObject);
            }
        }
    }
}
