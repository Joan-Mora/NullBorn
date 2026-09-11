using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.Game.Art
{
    /// <summary>
    /// Rim highlight for Interactable layer only (Cap 0: no comic outline on scenery).
    /// Extra MeshRenderer overlay — never extra material slots on the source mesh
    /// (Unity warns when materials.Length &gt; subMeshCount).
    /// </summary>
    public static class InteractableHighlight
    {
        public const string LayerName = "Interactable";
        public const string TagName = "Interactable";
        public const string ShaderName = "NullBorn/InteractableRim";
        const string OverlayName = "InteractableRim";

        static Material rimTemplate;

        public static void Apply(GameObject root)
        {
            if (root == null) return;

            var layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0) layer = 6;
            SetLayerRecurse(root.transform, layer);

            try
            {
                root.tag = TagName;
            }
            catch (System.Exception)
            {
                // Tag missing until Cap0ArtSetup creates it in editor.
            }

            var shader = Shader.Find(ShaderName);
            if (shader == null) return;

            if (rimTemplate == null || rimTemplate.shader != shader)
            {
                rimTemplate = new Material(shader) { name = "Cap0_InteractableRim" };
                rimTemplate.SetColor("_RimColor", new Color(0.24f, 0.42f, 0.55f, 1f));
                rimTemplate.SetFloat("_RimPower", 2.6f);
                rimTemplate.SetFloat("_RimIntensity", 1.25f);
            }

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponent<TextMesh>() != null) continue;
                if (renderer.gameObject.name == OverlayName) continue;

                StripExtraMaterialSlots(renderer, shader);
                EnsureOverlay(renderer, rimTemplate, layer);
            }
        }

        static void StripExtraMaterialSlots(Renderer renderer, Shader rimShader)
        {
            var mats = renderer.sharedMaterials;
            if (mats == null || mats.Length == 0) return;

            Mesh mesh = null;
            if (renderer is SkinnedMeshRenderer skinned)
                mesh = skinned.sharedMesh;
            else
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter != null) mesh = filter.sharedMesh;
            }

            var max = mesh != null ? mesh.subMeshCount : mats.Length;
            var kept = new System.Collections.Generic.List<Material>(max);
            for (var i = 0; i < mats.Length && kept.Count < max; i++)
            {
                if (mats[i] != null && mats[i].shader == rimShader) continue;
                kept.Add(mats[i]);
            }

            if (kept.Count == mats.Length) return;
            renderer.sharedMaterials = kept.ToArray();
        }

        static void EnsureOverlay(Renderer source, Material rim, int layer)
        {
            var existing = source.transform.Find(OverlayName);
            if (existing != null) return;

            var go = new GameObject(OverlayName);
            go.transform.SetParent(source.transform, false);
            go.layer = layer;

            if (source is SkinnedMeshRenderer skinned)
            {
                if (skinned.sharedMesh == null) return;
                var overlay = go.AddComponent<SkinnedMeshRenderer>();
                overlay.sharedMesh = skinned.sharedMesh;
                overlay.bones = skinned.bones;
                overlay.rootBone = skinned.rootBone;
                overlay.sharedMaterial = rim;
                ConfigureOverlay(overlay);
                return;
            }

            var filter = source.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                if (Application.isPlaying) Object.Destroy(go);
                else Object.DestroyImmediate(go);
                return;
            }

            go.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = rim;
            ConfigureOverlay(meshRenderer);
        }

        static void ConfigureOverlay(Renderer overlay)
        {
            overlay.shadowCastingMode = ShadowCastingMode.Off;
            overlay.receiveShadows = false;
            overlay.lightProbeUsage = LightProbeUsage.Off;
            overlay.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        static void SetLayerRecurse(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (var i = 0; i < t.childCount; i++)
                SetLayerRecurse(t.GetChild(i), layer);
        }
    }
}
