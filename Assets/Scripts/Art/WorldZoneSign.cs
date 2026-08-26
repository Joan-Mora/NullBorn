using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Cartel 3D legible en el mundo para marcar zonas del demo.
    /// </summary>
    public static class WorldZoneSign
    {
        public static GameObject Create(string title, string subtitle, Vector3 worldPos, Color accent)
        {
            GameObject root = new GameObject($"Sign_{title.Replace(' ', '_')}");
            root.transform.position = worldPos;

            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel";
            panel.transform.SetParent(root.transform, false);
            panel.transform.localScale = new Vector3(4.2f, 1.4f, 0.12f);
            Object.DestroyImmediate(panel.GetComponent<Collider>());

            Material panelMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            panelMat.color = new Color(0.06f, 0.08f, 0.12f, 1f);
            if (panelMat.HasProperty("_EmissionColor"))
            {
                panelMat.EnableKeyword("_EMISSION");
                panelMat.SetColor("_EmissionColor", accent * 0.35f);
            }
            panel.GetComponent<Renderer>().sharedMaterial = panelMat;

            CreateLabel(root.transform, title, new Vector3(0f, 0.22f, -0.08f), 64, accent);
            CreateLabel(root.transform, subtitle, new Vector3(0f, -0.18f, -0.08f), 28, Color.white);

            return root;
        }

        private static void CreateLabel(Transform parent, string text, Vector3 localPos, int size, Color color)
        {
            GameObject go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;

            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = size;
            tm.characterSize = 0.04f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
