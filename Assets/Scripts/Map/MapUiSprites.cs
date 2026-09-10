using UnityEngine;

namespace NullBorn.Game.Map
{
    public static class MapUiSprites
    {
        public static Sprite Circle(int size, Color fill, Color? ring = null, float ringWidth = 0.08f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var cx = (size - 1) * 0.5f;
            var r = cx - 1f;
            var ringInner = r * (1f - ringWidth);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cx));
                Color c;
                if (d > r) c = Color.clear;
                else if (ring.HasValue && d > ringInner) c = ring.Value;
                else c = fill;
                tex.SetPixel(x, y, c);
            }

            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite Triangle(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var tip = new Vector2(size * 0.5f, size * 0.92f);
            var left = new Vector2(size * 0.12f, size * 0.12f);
            var right = new Vector2(size * 0.88f, size * 0.12f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                tex.SetPixel(x, y, PointInTriangle(p, tip, left, right) ? color : Color.clear);
            }

            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite Diamond(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var c = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Mathf.Abs(x - c) + Mathf.Abs(y - c);
                tex.SetPixel(x, y, d <= c * 0.85f ? color : Color.clear);
            }

            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            var v0 = c - a;
            var v1 = b - a;
            var v2 = p - a;
            var dot00 = Vector2.Dot(v0, v0);
            var dot01 = Vector2.Dot(v0, v1);
            var dot02 = Vector2.Dot(v0, v2);
            var dot11 = Vector2.Dot(v1, v1);
            var dot12 = Vector2.Dot(v1, v2);
            var inv = 1f / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * inv;
            var v = (dot00 * dot12 - dot01 * dot02) * inv;
            return u >= 0f && v >= 0f && u + v < 1f;
        }
    }
}
