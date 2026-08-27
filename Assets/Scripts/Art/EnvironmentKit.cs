using UnityEngine;

namespace NullBorn.Art
{
    /// <summary>
    /// Kit modular runtime: árboles, edificios, rocas, caminos, farolas, puentes.
    /// Estética stylized digital-city (no fotorealismo).
    /// </summary>
    public static class EnvironmentKit
    {
        public static GameObject Tree(string name, Vector3 pos, float scale, Material bark, Material leaf, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.position = pos;
            root.transform.SetParent(parent, true);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1.1f * scale, 0f);
            trunk.transform.localScale = new Vector3(0.28f * scale, 1.1f * scale, 0.28f * scale);
            trunk.GetComponent<Renderer>().sharedMaterial = bark;

            int crowns = 3;
            for (int i = 0; i < crowns; i++)
            {
                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = $"Crown_{i}";
                crown.transform.SetParent(root.transform, false);
                float y = (1.8f + i * 0.55f) * scale;
                float s = (1.4f - i * 0.25f) * scale;
                crown.transform.localPosition = new Vector3(
                    Random.Range(-0.15f, 0.15f) * scale,
                    y,
                    Random.Range(-0.15f, 0.15f) * scale);
                crown.transform.localScale = Vector3.one * s;
                crown.GetComponent<Renderer>().sharedMaterial = leaf;
                Object.Destroy(crown.GetComponent<Collider>());
            }

            return root;
        }

        public static GameObject DeadTree(string name, Vector3 pos, float scale, Material bark, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.position = pos;
            root.transform.SetParent(parent, true);
            root.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1.4f * scale, 0f);
            trunk.transform.localScale = new Vector3(0.22f * scale, 1.4f * scale, 0.22f * scale);
            trunk.GetComponent<Renderer>().sharedMaterial = bark;

            for (int i = 0; i < 3; i++)
            {
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.transform.SetParent(root.transform, false);
                branch.transform.localPosition = new Vector3(0f, (1.6f + i * 0.4f) * scale, 0f);
                branch.transform.localRotation = Quaternion.Euler(0f, i * 120f, 55f + i * 10f);
                branch.transform.localScale = new Vector3(0.08f * scale, 0.7f * scale, 0.08f * scale);
                branch.GetComponent<Renderer>().sharedMaterial = bark;
                Object.Destroy(branch.GetComponent<Collider>());
            }

            return root;
        }

        public static GameObject Rock(string name, Vector3 pos, float scale, Material mat, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale * Random.Range(0.8f, 1.4f), scale * Random.Range(0.4f, 0.8f), scale * Random.Range(0.8f, 1.3f));
            go.transform.rotation = Quaternion.Euler(Random.Range(0f, 30f), Random.Range(0f, 360f), Random.Range(0f, 20f));
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent, true);
            return go;
        }

        public static GameObject Building(string name, Vector3 pos, Vector3 footprint, float floors, Material wall, Material accent, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.position = pos;
            root.transform.SetParent(parent, true);

            float height = Mathf.Max(2.5f, floors * 3.2f);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(footprint.x, height, footprint.z);
            body.GetComponent<Renderer>().sharedMaterial = wall;

            // Roof lip
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(root.transform, false);
            roof.transform.localPosition = new Vector3(0f, height + 0.15f, 0f);
            roof.transform.localScale = new Vector3(footprint.x * 1.08f, 0.3f, footprint.z * 1.08f);
            roof.GetComponent<Renderer>().sharedMaterial = accent;
            Object.Destroy(roof.GetComponent<Collider>());

            // Window strips
            int rows = Mathf.Clamp(Mathf.RoundToInt(floors), 1, 8);
            for (int r = 0; r < rows; r++)
            {
                float y = 1.2f + r * (height / (rows + 0.5f));
                GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = $"Windows_{r}";
                win.transform.SetParent(root.transform, false);
                win.transform.localPosition = new Vector3(0f, y, footprint.z * 0.51f);
                win.transform.localScale = new Vector3(footprint.x * 0.85f, 0.55f, 0.08f);
                win.GetComponent<Renderer>().sharedMaterial = accent;
                Object.Destroy(win.GetComponent<Collider>());
            }

            return root;
        }

        public static GameObject Antenna(string name, Vector3 pos, float height, Material mast, Material tip, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.position = pos;
            root.transform.SetParent(parent, true);

            GameObject basePad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePad.transform.SetParent(root.transform, false);
            basePad.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            basePad.transform.localScale = new Vector3(2.2f, 0.25f, 2.2f);
            basePad.GetComponent<Renderer>().sharedMaterial = mast;

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.SetParent(root.transform, false);
            pole.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            pole.transform.localScale = new Vector3(0.35f, height * 0.5f, 0.35f);
            pole.GetComponent<Renderer>().sharedMaterial = mast;

            GameObject dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dish.transform.SetParent(root.transform, false);
            dish.transform.localPosition = new Vector3(0.8f, height * 0.72f, 0f);
            dish.transform.localScale = new Vector3(1.6f, 0.35f, 1.6f);
            dish.GetComponent<Renderer>().sharedMaterial = tip;
            Object.Destroy(dish.GetComponent<Collider>());

            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.transform.SetParent(root.transform, false);
            beacon.transform.localPosition = new Vector3(0f, height + 0.4f, 0f);
            beacon.transform.localScale = Vector3.one * 0.55f;
            beacon.GetComponent<Renderer>().sharedMaterial = tip;
            Object.Destroy(beacon.GetComponent<Collider>());

            return root;
        }

        public static GameObject Lamp(string name, Vector3 pos, Material pole, Material glow, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.position = pos;
            root.transform.SetParent(parent, true);

            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            p.transform.SetParent(root.transform, false);
            p.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            p.transform.localScale = new Vector3(0.12f, 1.6f, 0.12f);
            p.GetComponent<Renderer>().sharedMaterial = pole;
            Object.Destroy(p.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0.35f, 3.15f, 0f);
            head.transform.localScale = new Vector3(0.9f, 0.2f, 0.4f);
            head.GetComponent<Renderer>().sharedMaterial = glow;
            Object.Destroy(head.GetComponent<Collider>());

            Light light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = glow.HasProperty("_EmissionColor") ? glow.GetColor("_EmissionColor") : Color.cyan;
            light.intensity = 1.4f;
            light.range = 12f;
            light.transform.localPosition = new Vector3(0.35f, 3f, 0f);

            return root;
        }

        public static GameObject PathSegment(string name, Vector3 from, Vector3 to, float width, Material mat, Transform parent)
        {
            Vector3 mid = (from + to) * 0.5f;
            mid.y = Mathf.Max(from.y, to.y) + 0.02f;
            Vector3 delta = to - from;
            float len = delta.magnitude;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = mid;
            go.transform.rotation = Quaternion.LookRotation(new Vector3(delta.x, 0f, delta.z).normalized, Vector3.up);
            go.transform.localScale = new Vector3(width, 0.08f, len);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent, true);
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Bridge(string name, Vector3 from, Vector3 to, float width, Material deck, Material rail, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, true);
            PathSegment(name + "_Deck", from, to, width, deck, root.transform);

            Vector3 dir = (to - from).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized * (width * 0.48f);
            for (int i = 0; i <= 6; i++)
            {
                float t = i / 6f;
                Vector3 p = Vector3.Lerp(from, to, t) + Vector3.up * 0.6f;
                GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                postL.transform.SetParent(root.transform, true);
                postL.transform.position = p + side;
                postL.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                postL.GetComponent<Renderer>().sharedMaterial = rail;
                Object.Destroy(postL.GetComponent<Collider>());

                GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                postR.transform.SetParent(root.transform, true);
                postR.transform.position = p - side;
                postR.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                postR.GetComponent<Renderer>().sharedMaterial = rail;
                Object.Destroy(postR.GetComponent<Collider>());
            }

            return root;
        }

        public static GameObject GroundMesh(string name, Vector3 center, float size, int res, float heightAmp, int seed, Material mat, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.position = center;
            go.transform.SetParent(parent, true);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();
            mr.sharedMaterial = mat;

            Mesh mesh = BuildHeightMesh(size, res, heightAmp, seed);
            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            return go;
        }

        private static Mesh BuildHeightMesh(float size, int res, float heightAmp, int seed)
        {
            int vCount = (res + 1) * (res + 1);
            Vector3[] verts = new Vector3[vCount];
            Vector2[] uvs = new Vector2[vCount];
            int[] tris = new int[res * res * 6];

            Random.InitState(seed);
            float ox = Random.Range(0f, 100f);
            float oz = Random.Range(0f, 100f);
            float half = size * 0.5f;

            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    int i = z * (res + 1) + x;
                    float wx = -half + (x / (float)res) * size;
                    float wz = -half + (z / (float)res) * size;
                    float n = Mathf.PerlinNoise(ox + x * 0.12f, oz + z * 0.12f);
                    float n2 = Mathf.PerlinNoise(ox + x * 0.35f, oz + z * 0.35f);
                    float h = (n * 0.75f + n2 * 0.25f) * heightAmp;
                    // Flatten center plaza disk
                    float dist = Mathf.Sqrt(wx * wx + wz * wz);
                    float flatten = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((dist - 18f) / 22f));
                    h *= flatten;
                    verts[i] = new Vector3(wx, h, wz);
                    uvs[i] = new Vector2(x / (float)res * 8f, z / (float)res * 8f);
                }
            }

            int t = 0;
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    int i = z * (res + 1) + x;
                    tris[t++] = i;
                    tris[t++] = i + res + 1;
                    tris[t++] = i + 1;
                    tris[t++] = i + 1;
                    tris[t++] = i + res + 1;
                    tris[t++] = i + res + 2;
                }
            }

            Mesh mesh = new Mesh { name = "ProcTerrain" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
