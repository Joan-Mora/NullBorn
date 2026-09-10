using System.Collections.Generic;
using NullBorn.Game.Net;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.Game.Map
{
    public class WorldMarkerView : MonoBehaviour
    {
        [SerializeField] GameSyncClient sync;
        [SerializeField] float markerHeight = 2.4f;
        [SerializeField] float beamRadius = 0.18f;

        readonly Dictionary<string, GameObject> spawned = new();
        readonly Dictionary<string, MeshRenderer> renderers = new();
        readonly Dictionary<string, Color> lastColors = new();
        static Material sharedUnlit;
        MaterialPropertyBlock propBlock;

        void Awake()
        {
            if (sync == null)
                sync = GetComponent<GameSyncClient>() ?? FindAnyObjectByType<GameSyncClient>();
            propBlock = new MaterialPropertyBlock();
            EnsureSharedMaterial();
        }

        void OnEnable()
        {
            if (sync != null)
                sync.MarkersUpdated += OnMarkers;
        }

        void OnDisable()
        {
            if (sync != null)
                sync.MarkersUpdated -= OnMarkers;
        }

        void OnMarkers(MarkerDto[] markers)
        {
            var seen = new HashSet<string>();
            foreach (var m in markers)
            {
                seen.Add(m.id);
                if (!spawned.TryGetValue(m.id, out var go) || go == null)
                {
                    go = CreateMarkerVisual(m);
                    spawned[m.id] = go;
                }

                go.transform.position = new Vector3(m.x, 0.02f, m.z);
                ApplyColor(m);
                go.transform.localScale = m.isActive
                    ? new Vector3(1.25f, markerHeight * 1.15f, 1.25f)
                    : new Vector3(1f, markerHeight, 1f);
            }

            var stale = new List<string>();
            foreach (var kv in spawned)
            {
                if (!seen.Contains(kv.Key))
                    stale.Add(kv.Key);
            }

            foreach (var id in stale)
            {
                if (spawned.TryGetValue(id, out var go) && go != null)
                    Destroy(go);
                spawned.Remove(id);
                renderers.Remove(id);
                lastColors.Remove(id);
            }
        }

        GameObject CreateMarkerVisual(MarkerDto m)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            root.name = "MapMarker_" + m.label;
            root.transform.SetParent(transform, true);
            Object.Destroy(root.GetComponent<Collider>());
            root.transform.localScale = new Vector3(beamRadius * 2f, markerHeight * 0.5f, beamRadius * 2f);

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                EnsureSharedMaterial();
                renderer.sharedMaterial = sharedUnlit;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderers[m.id] = renderer;
            }

            ApplyColor(m);
            return root;
        }

        void ApplyColor(MarkerDto m)
        {
            if (!renderers.TryGetValue(m.id, out var renderer) || renderer == null) return;

            if (!ColorUtility.TryParseHtmlString(m.color, out var color))
                color = new Color(0.91f, 0.36f, 0.02f);
            if (m.isActive)
                color = Color.Lerp(color, Color.white, 0.35f);

            if (lastColors.TryGetValue(m.id, out var prev) && ColorsAlmostEqual(prev, color))
                return;

            lastColors[m.id] = color;
            if (propBlock == null)
                propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            if (sharedUnlit != null && sharedUnlit.HasProperty("_BaseColor"))
                propBlock.SetColor("_BaseColor", color);
            else
                propBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propBlock);
        }

        static bool ColorsAlmostEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f
                   && Mathf.Abs(a.g - b.g) < 0.01f
                   && Mathf.Abs(a.b - b.b) < 0.01f
                   && Mathf.Abs(a.a - b.a) < 0.01f;
        }

        static void EnsureSharedMaterial()
        {
            if (sharedUnlit != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Standard");
            sharedUnlit = new Material(shader);
            sharedUnlit.enableInstancing = true;
        }
    }
}
