using UnityEngine;

namespace NullBorn.Art
{
    public static class RuntimeMaterials
    {
        public static Material Lit(Color color, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            if (mat.HasProperty("_EmissionColor") && emission.maxColorComponent > 0.01f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }

            return mat;
        }

        public static Material Textured(Texture2D albedo, Color tint, float tiling = 1f, Color emission = default)
        {
            Material mat = Lit(tint, emission);
            if (albedo != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", albedo);
                mat.mainTexture = albedo;
                mat.mainTextureScale = new Vector2(tiling, tiling);
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.25f);
            return mat;
        }

        public static void ApplyColor(Material mat, Color color)
        {
            if (mat == null) return;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
        }

        public static void ApplyEmission(Material mat, Color emission)
        {
            if (mat == null || !mat.HasProperty("_EmissionColor")) return;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
        }
    }
}
