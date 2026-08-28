using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Avatar placeholder del Thread (hasta importar FBX de Blender/Mixamo).
    /// </summary>
    public static class ThreadAvatarBuilder
    {
        public static GameObject Build(Transform parent, Material bodyMat, Material coreMat)
        {
            GameObject root = new GameObject("Visual_ThreadAvatar");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;

            CreatePart(root.transform, PrimitiveType.Capsule, "Torso", new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.55f, 0.4f), bodyMat);
            CreatePart(root.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.95f, 0f), Vector3.one * 0.38f, bodyMat);
            CreatePart(root.transform, PrimitiveType.Sphere, "Core", new Vector3(0f, 0.15f, 0.22f), Vector3.one * 0.22f, coreMat);
            CreatePart(root.transform, PrimitiveType.Capsule, "Arm_L", new Vector3(-0.45f, 0.15f, 0f), new Vector3(0.18f, 0.35f, 0.18f), bodyMat);
            CreatePart(root.transform, PrimitiveType.Capsule, "Arm_R", new Vector3(0.45f, 0.15f, 0f), new Vector3(0.18f, 0.35f, 0.18f), bodyMat);
            CreatePart(root.transform, PrimitiveType.Capsule, "Leg_L", new Vector3(-0.18f, -0.75f, 0f), new Vector3(0.2f, 0.35f, 0.2f), bodyMat);
            CreatePart(root.transform, PrimitiveType.Capsule, "Leg_R", new Vector3(0.18f, -0.75f, 0f), new Vector3(0.2f, 0.35f, 0.2f), bodyMat);

            CreatePart(root.transform, PrimitiveType.Cylinder, "Halo", new Vector3(0f, 0.5f, 0f), new Vector3(1.1f, 0.03f, 1.1f), coreMat);
            root.AddComponent<SimpleBobAnim>();
            return root;
        }

        private static void CreatePart(Transform parent, PrimitiveType type, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = scale;
            StripCollider(part);
            part.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void StripCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col == null) return;
            if (Application.isPlaying) Object.Destroy(col);
            else Object.DestroyImmediate(col);
        }
    }
}
