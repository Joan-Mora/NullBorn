using System.Collections.Generic;
using NullBorn.Game.Art;
using NullBorn.Game.Net;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.Game.Map
{
    /// <summary>
    /// Tall colored beacons on the ground so phone waypoints are obvious in 3D,
    /// not only on the minimap / compass.
    /// </summary>
    public class WorldMarkerView : MonoBehaviour
    {
        [SerializeField] GameSyncClient sync;
        [SerializeField] float beamHeight = 7.5f;
        [SerializeField] float beamRadius = 0.28f;

        readonly Dictionary<string, GameObject> spawned = new();
        readonly Dictionary<string, MeshRenderer[]> renderers = new();
        readonly Dictionary<string, TextMesh> labels = new();
        static Shader markerShader;

        void Awake()
        {
            if (sync == null)
                sync = GetComponent<GameSyncClient>() ?? FindAnyObjectByType<GameSyncClient>();
            EnsureShader();
        }

        void Start()
        {
            if (sync != null && sync.LatestMarkers != null && sync.LatestMarkers.Length > 0)
                OnMarkers(sync.LatestMarkers);
        }

        void OnEnable()
        {
            if (sync == null)
                sync = GetComponent<GameSyncClient>() ?? FindAnyObjectByType<GameSyncClient>();
            if (sync == null) return;
            sync.MarkersUpdated -= OnMarkers;
            sync.MarkersUpdated += OnMarkers;
            if (sync.LatestMarkers != null && sync.LatestMarkers.Length > 0)
                OnMarkers(sync.LatestMarkers);
        }

        void OnDisable()
        {
            if (sync != null)
                sync.MarkersUpdated -= OnMarkers;
        }

        void OnMarkers(MarkerDto[] markers)
        {
            if (markers == null) return;

            var seen = new HashSet<string>();
            foreach (var m in markers)
            {
                if (m == null || string.IsNullOrEmpty(m.id)) continue;
                seen.Add(m.id);
                if (!spawned.TryGetValue(m.id, out var go) || go == null)
                {
                    go = CreateBeacon(m);
                    spawned[m.id] = go;
                }

                PlaceOnGround(go.transform, m.x, m.z);
                ApplyColor(m);
                if (labels.TryGetValue(m.id, out var label) && label != null)
                    label.text = string.IsNullOrWhiteSpace(m.label) ? "Pin" : m.label;
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
                labels.Remove(id);
            }
        }

        void PlaceOnGround(Transform t, float x, float z)
        {
            var y = 0.04f;
            var origin = new Vector3(x, 80f, z);
            if (Physics.Raycast(origin, Vector3.down, out var hit, 160f))
                y = hit.point.y + 0.04f;
            t.position = new Vector3(x, y, z);
        }

        GameObject CreateBeacon(MarkerDto m)
        {
            var root = new GameObject("MapMarker_" + (m.label ?? "Pin"));
            root.transform.SetParent(transform, true);

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Beam";
            beam.transform.SetParent(root.transform, false);
            Object.Destroy(beam.GetComponent<Collider>());
            beam.transform.localScale = new Vector3(beamRadius * 2f, beamHeight * 0.5f, beamRadius * 2f);
            beam.transform.localPosition = new Vector3(0f, beamHeight * 0.5f, 0f);

            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Cap";
            cap.transform.SetParent(root.transform, false);
            Object.Destroy(cap.GetComponent<Collider>());
            cap.transform.localScale = Vector3.one * 0.85f;
            cap.transform.localPosition = new Vector3(0f, beamHeight + 0.15f, 0f);

            var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "Pad";
            pad.transform.SetParent(root.transform, false);
            Object.Destroy(pad.GetComponent<Collider>());
            pad.transform.localScale = new Vector3(1.6f, 0.06f, 1.6f);
            pad.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, beamHeight + 1.05f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = string.IsNullOrWhiteSpace(m.label) ? "Pin" : m.label;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 48;
            tm.characterSize = 0.12f;
            tm.color = Color.white;
            labels[m.id] = tm;
            labelGo.AddComponent<BillboardToCamera>();

            var list = new List<MeshRenderer>();
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.lightProbeUsage = LightProbeUsage.Off;
                r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                r.material = NewMarkerMaterial();
                list.Add(r);
            }

            renderers[m.id] = list.ToArray();
            InteractableHighlight.Apply(root);
            return root;
        }

        void ApplyColor(MarkerDto m)
        {
            if (!renderers.TryGetValue(m.id, out var rs) || rs == null) return;
            if (!ColorUtility.TryParseHtmlString(m.color, out var color))
                color = new Color(0.91f, 0.36f, 0.02f, 1f);
            color.a = 1f;
            if (m.isActive)
                color = Color.Lerp(color, Color.white, 0.28f);

            foreach (var r in rs)
            {
                if (r == null) continue;
                var mats = r.materials;
                for (var i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null) continue;
                    if (mat.shader != null && mat.shader.name == InteractableHighlight.ShaderName)
                        continue;
                    SetColor(mat, color);
                }
            }

            if (labels.TryGetValue(m.id, out var label) && label != null)
                label.color = color;
        }

        static void SetColor(Material mat, Color color)
        {
            if (mat == null) return;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_UnlitColor")) mat.SetColor("_UnlitColor", color);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.6f);
            }
        }

        static Material NewMarkerMaterial()
        {
            EnsureShader();
            var mat = new Material(markerShader);
            return mat;
        }

        static void EnsureShader()
        {
            if (markerShader != null) return;
            markerShader = Shader.Find("Universal Render Pipeline/Unlit")
                           ?? Shader.Find("Unlit/Color")
                           ?? Shader.Find("Sprites/Default")
                           ?? Shader.Find("Standard");
        }

        class BillboardToCamera : MonoBehaviour
        {
            void LateUpdate()
            {
                var cam = Camera.main;
                if (cam == null) return;
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }
        }
    }
}
