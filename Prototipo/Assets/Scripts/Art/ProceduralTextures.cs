using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Texturas runtime (noise) para suelo, corteza, follaje y cielo — sin Asset Store.
    /// </summary>
    public static class ProceduralTextures
    {
        public static Texture2D NoiseAlbedo(int size, Color a, Color b, float scale, int seed)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                name = "ProcNoise"
            };

            Random.InitState(seed);
            float ox = Random.Range(0f, 1000f);
            float oy = Random.Range(0f, 1000f);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(ox + x * scale, oy + y * scale);
                    float n2 = Mathf.PerlinNoise(ox + x * scale * 2.7f, oy + y * scale * 2.7f);
                    float t = Mathf.Clamp01(n * 0.7f + n2 * 0.3f);
                    pixels[y * size + x] = Color.Lerp(a, b, t);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        public static Texture2D GridAlbedo(int size, Color baseCol, Color lineCol, int cells)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                name = "ProcGrid"
            };

            int cell = Mathf.Max(1, size / cells);
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool line = x % cell == 0 || y % cell == 0 || x % cell == cell - 1 || y % cell == cell - 1;
                    pixels[y * size + x] = line ? lineCol : baseCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        public static Cubemap GradientSky(int faceSize, Color zenith, Color horizon, Color nadir)
        {
            Cubemap cube = new Cubemap(faceSize, TextureFormat.RGB24, false);
            for (int face = 0; face < 6; face++)
            {
                Color[] pixels = new Color[faceSize * faceSize];
                for (int y = 0; y < faceSize; y++)
                {
                    for (int x = 0; x < faceSize; x++)
                    {
                        Vector3 dir = FaceDir((CubemapFace)face, x, y, faceSize);
                        float h = dir.y * 0.5f + 0.5f;
                        Color c = h > 0.5f
                            ? Color.Lerp(horizon, zenith, (h - 0.5f) * 2f)
                            : Color.Lerp(nadir, horizon, h * 2f);
                        pixels[y * faceSize + x] = c;
                    }
                }

                cube.SetPixels(pixels, (CubemapFace)face);
            }

            cube.Apply();
            return cube;
        }

        private static Vector3 FaceDir(CubemapFace face, int x, int y, int size)
        {
            float u = (x + 0.5f) / size * 2f - 1f;
            float v = (y + 0.5f) / size * 2f - 1f;
            switch (face)
            {
                case CubemapFace.PositiveX: return new Vector3(1f, -v, -u).normalized;
                case CubemapFace.NegativeX: return new Vector3(-1f, -v, u).normalized;
                case CubemapFace.PositiveY: return new Vector3(u, 1f, v).normalized;
                case CubemapFace.NegativeY: return new Vector3(u, -1f, -v).normalized;
                case CubemapFace.PositiveZ: return new Vector3(u, -v, 1f).normalized;
                default: return new Vector3(-u, -v, -1f).normalized;
            }
        }
    }
}
