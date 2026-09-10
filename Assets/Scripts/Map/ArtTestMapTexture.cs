using UnityEngine;

namespace NullBorn.Game.Map
{
    /// <summary>
    /// Procedural ArtTest map — same landmarks as companion SVG (offline / SVG fallback).
    /// </summary>
    public static class ArtTestMapTexture
    {
        public static Texture2D Build(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "ArtTestMap"
            };

            var grassA = new Color(0.16f, 0.24f, 0.15f);
            var grassB = new Color(0.20f, 0.28f, 0.20f);
            var plaza = new Color(0.35f, 0.44f, 0.28f);
            var wood = new Color(0.36f, 0.29f, 0.20f);
            var wall = new Color(0.54f, 0.48f, 0.41f);
            var path = new Color(0.42f, 0.49f, 0.32f, 0.55f);
            var tree = new Color(0.18f, 0.29f, 0.17f);

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var u = x / (size - 1f);
                var v = y / (size - 1f);
                // World: x [-40,40], z [-40,40]; tex v = world z
                var wx = Mathf.Lerp(-40f, 40f, u);
                var wz = Mathf.Lerp(-40f, 40f, v);

                var c = ((x / 8 + y / 8) % 2 == 0) ? grassA : grassB;

                var dist = Mathf.Sqrt(wx * wx + wz * wz);
                if (Mathf.Abs(dist - 16.5f) < 1.1f)
                    c = Color.Lerp(c, path, 0.7f);

                if (Mathf.Abs(wx) < 9.5f && Mathf.Abs(wz) < 9.5f)
                    c = plaza;
                if (Mathf.Abs(wx) < 6.5f && Mathf.Abs(wz) < 6.5f)
                    c = wood;

                // Walls / props approximate
                if (wz > 10f && wz < 12.5f && Mathf.Abs(wx) < 5f) c = wall;
                if (wx < -10.5f && wx > -13f && Mathf.Abs(wz - 2f) < 4f) c = wall;
                if (wx > 10.5f && wx < 13f && Mathf.Abs(wz - 2f) < 4f) c = wall;

                if (Vector2.Distance(new Vector2(wx, wz), new Vector2(-8f, -6f)) < 2.2f) c = tree;
                if (Vector2.Distance(new Vector2(wx, wz), new Vector2(8f, -4f)) < 2.5f) c = tree;
                if (Vector2.Distance(new Vector2(wx, wz), new Vector2(3f, 5f)) < 1.2f) c = new Color(0.24f, 0.36f, 0.22f);

                tex.SetPixel(x, y, c);
            }

            // Spawn dot
            var sx = Mathf.RoundToInt(Mathf.InverseLerp(-40f, 40f, 0f) * (size - 1));
            var sz = Mathf.RoundToInt(Mathf.InverseLerp(-40f, 40f, -3f) * (size - 1));
            for (var dy = -2; dy <= 2; dy++)
            for (var dx = -2; dx <= 2; dx++)
            {
                var px = Mathf.Clamp(sx + dx, 0, size - 1);
                var py = Mathf.Clamp(sz + dy, 0, size - 1);
                tex.SetPixel(px, py, new Color(0.88f, 0.57f, 0.29f));
            }

            tex.Apply(false, true);
            return tex;
        }
    }
}
