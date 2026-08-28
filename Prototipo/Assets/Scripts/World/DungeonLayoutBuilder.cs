using NullBorn.Art;
using NullBorn.Core;
using NullBorn.Enemies;
using NullBorn.Player;
using UnityEngine;

namespace NullBorn.World
{
    public static class DungeonLayoutBuilder
    {
        public static void Build(string sceneName)
        {
            Vector3 origin = Vector3.zero;
            GameObject root = new GameObject("DungeonRoot");
            BuildInternal(sceneName, origin, root);
        }

        public static void BuildInline(string sceneName, Vector3 origin, GameObject parent = null)
        {
            GameObject root = new GameObject($"Inline_{sceneName}");
            if (parent != null) root.transform.SetParent(parent.transform, false);
            root.transform.position = Vector3.zero;
            BuildInternal(sceneName, origin, root);
        }

        private static void BuildInternal(string sceneName, Vector3 origin, GameObject root)
        {
            bool stack = sceneName != null && sceneName.Contains("Stack");
            Material floor = RuntimeMaterials.Lit(
                stack ? new Color(0.1f, 0.14f, 0.2f) : new Color(0.16f, 0.08f, 0.12f),
                stack ? new Color(0.1f, 0.3f, 0.5f) * 0.3f : new Color(0.4f, 0.05f, 0.2f) * 0.4f);
            Material accent = RuntimeMaterials.Lit(
                stack ? new Color(0.2f, 0.5f, 0.7f) : new Color(0.7f, 0.15f, 0.45f),
                Color.white * 0.2f);
            Material leakMat = RuntimeMaterials.Lit(new Color(0.85f, 0.12f, 0.35f), new Color(0.9f, 0.1f, 0.4f) * 1.4f);

            PlaneAt("DungeonFloor", origin, new Vector3(2.4f, 1f, 3.6f), floor, root);
            for (int i = 0; i < 6; i++)
            {
                Cube($"{sceneName}_col_{i}", origin + new Vector3((i % 2 == 0 ? -6f : 6f), 1.5f, i * 4f - 6f), new Vector3(1.2f, 3f, 1.2f), accent, root);
            }

            WorldLayoutBuilder.SpawnLeak($"{sceneName}_A", origin + new Vector3(-3f, 1.15f, 4f), LeakVariant.Basic, leakMat, root.transform);
            WorldLayoutBuilder.SpawnLeak($"{sceneName}_B", origin + new Vector3(3f, 1.15f, 8f), stack ? LeakVariant.Spawner : LeakVariant.Basic, leakMat, root.transform);
            WorldLayoutBuilder.SpawnLeak($"{sceneName}_C", origin + new Vector3(0f, 1.15f, 14f), stack ? LeakVariant.NullRef : LeakVariant.Spawner, leakMat, root.transform);

            GameObject exit = new GameObject("DungeonExit");
            exit.transform.SetParent(root.transform, false);
            exit.transform.position = origin + new Vector3(0f, 1f, -8f);
            SphereCollider col = exit.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.8f;
            exit.AddComponent<DungeonExit>();
            Cube("ExitMarker", origin + new Vector3(0f, 1.5f, -8f), new Vector3(2.2f, 3f, 0.4f), accent, root);

            WorldZoneSign.Create(stack ? "STACK CHASM" : "HEAP CAVE", "Instancia", origin + new Vector3(0f, 3f, -4f),
                stack ? new Color(0.4f, 0.8f, 1f) : new Color(0.95f, 0.3f, 0.6f));
        }

        private static void PlaneAt(string name, Vector3 pos, Vector3 scale, Material mat, GameObject parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent.transform, true);
        }

        private static void Cube(string name, Vector3 pos, Vector3 scale, Material mat, GameObject parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.SetParent(parent.transform, true);
        }
    }
}
