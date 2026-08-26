using UnityEngine;

namespace NullBorn.Art
{
    public static class LeakAvatarBuilder
    {
        public static GameObject Build(Transform parent, Material mat, float scale = 1f)
        {
            GameObject root = new GameObject("Visual_Leak");
            root.transform.SetParent(parent, false);

            CreateSphere(root.transform, "Core", Vector3.zero, Vector3.one * (1.1f * scale), mat);
            CreateSphere(root.transform, "InnerPulse", Vector3.up * (0.15f * scale), Vector3.one * (0.55f * scale), mat);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(a), 0.15f + (i % 2) * 0.12f, Mathf.Sin(a)) * (0.65f * scale);
                CreateSphere(root.transform, $"Nodule_{i}", pos, Vector3.one * (0.32f * scale), mat);
            }
            CreateSpike(root.transform, "Spike_T", Vector3.up * (0.75f * scale), Vector3.one * (0.18f * scale, 0.55f * scale, 0.18f * scale), mat);

            root.AddComponent<SimpleBobAnim>();
            return root;
        }

        private static void CreateSphere(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = scale;
            Collider col = part.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(col);
            else Object.DestroyImmediate(col);
            part.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateSpike(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = scale;
            Collider col = part.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(col);
            else Object.DestroyImmediate(col);
            part.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
