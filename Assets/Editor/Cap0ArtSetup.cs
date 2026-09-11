#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NullBorn.Game.Art;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace NullBorn.Editor
{
    /// <summary>
    /// Cap 0 Fantasy art lock: URP Lit environment (no toon outline),
    /// rim highlight only on Interactable, Ranger outfit on the Superhero host.
    /// </summary>
    public static class Cap0ArtSetup
    {
        public const string MatFolder = "Assets/Art/Materials/Cap0";
        public const string RangerFbx = "Assets/_ThirdParty/Characters/Outfits/FBX/Outfits/Male_Ranger.fbx";
        public const string HairBuzzedFbx = "Assets/_ThirdParty/Characters/Hairstyles/FBX/Hair_Buzzed.fbx";
        public const string RimShaderName = "NullBorn/InteractableRim";

        public const string WeaponHolsterName = "WeaponHolster";
        public const string WeaponSwordName = "Weapon_Sword";

        // Drawn grip under hand_r — Combat/ only. Cap 0 loco never parents here.
        // <Cap0SwordGrip>
        public static readonly bool SwordGripCaptured = true;
        public const float SwordGripPx = 0.02f;
        public const float SwordGripPy = 0.11f;
        public const float SwordGripPz = -0.126f;
        public const float SwordGripQx = 0.435912f;
        public const float SwordGripQy = -0.5215397f;
        public const float SwordGripQz = -0.4962368f;
        public const float SwordGripQw = 0.5401168f;
        public const float SwordGripSx = 0.63263f;
        public const float SwordGripSy = 0.7302496f;
        public const float SwordGripSz = 0.6806615f;
        // </Cap0SwordGrip>

        // Local pose of Weapon_Sword under WeaponHolster (Chest / spine, not hips).
        // Old hips-local pos (0.32, 0.751, -0.205) clipped the hood in run — recapture on Chest.
        // Rotation/scale kept as the last back-sheath orientation.
        // <Cap0SwordHolster>
        public static readonly bool SwordHolsterCaptured = true;
        public const float SwordHolsterPx = 0.239f;
        public const float SwordHolsterPy = 0.71f;
        public const float SwordHolsterPz = -0.188f;
        public const float SwordHolsterQx = 0.01476489f;
        public const float SwordHolsterQy = 0.01398673f;
        public const float SwordHolsterQz = 0.9766073f;
        public const float SwordHolsterQw = 0.214067f;
        public const float SwordHolsterSx = 0.63263f;
        public const float SwordHolsterSy = 0.7302496f;
        public const float SwordHolsterSz = 0.6806615f;
        // </Cap0SwordHolster>

        public static readonly Color Grass = Hex("4F7A3E");
        public static readonly Color Leaf = Hex("7CAF4A");
        public static readonly Color Earth = Hex("6B4E32");
        public static readonly Color Wood = Hex("A67C52");
        public static readonly Color Sky = Hex("A8C8E8");
        public static readonly Color HeroAccent = Hex("3D6B8C");

        [MenuItem("NullBorn/Art/Log Cap 0 Material Audit")]
        public static void LogSceneAuditMenu()
        {
            LogSceneAudit();
        }

        [MenuItem("NullBorn/Art/Log Player Skin Probe")]
        public static void LogSkinProbeMenu()
        {
            var player = GameObject.Find("Player");
            var visual = player != null ? player.transform.Find("Visual") : null;
            if (visual == null)
            {
                Debug.LogWarning("[NullBorn][Cap0] Player/Visual not found. Open ArtTest or enter Play.");
                return;
            }

            LogSkinProbe(visual.gameObject);
        }

        [MenuItem("NullBorn/Cap0/Capture Sword Grip From Selection", false, 20)]
        public static void CaptureSwordGripFromSelection()
        {
            var sword = FindSwordInEditor(preferHolster: false);
            if (sword == null)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — Sword grip",
                    "No hay Weapon_Sword en la escena.\nPlay o Build Art Test Scene, Pause, Hierarchy: Weapon_Sword bajo hand_r (R para desenvainar).",
                    "OK");
                return;
            }

            Selection.activeTransform = sword;
            var p = sword.localPosition;
            var q = sword.localRotation;
            var s = sword.localScale;
            if (!WriteTaggedPose("Cap0SwordGrip", "SwordGripCaptured", "SwordGrip", p, q, s))
            {
                Debug.LogError("[NullBorn][Cap0] Could not write sword grip constants to Cap0ArtSetup.cs.");
                return;
            }

            LogCapturedPose("sword grip (parent=" + (sword.parent != null ? sword.parent.name : "?") + " local)", p, q, s);
            EditorUtility.DisplayDialog(
                "NullBorn — Sword grip captured",
                "Valores escritos en Cap0ArtSetup (ApplyDrawnGrip).\nRebuild Art Test Scene si quieres hornear el grip.",
                "OK");
        }

        [MenuItem("NullBorn/Cap0/Capture Holster Pose From Selection", false, 21)]
        public static void CaptureHolsterPoseFromSelection()
        {
            var sword = FindSwordInEditor(preferHolster: true);
            if (sword == null)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — Holster pose",
                    "No hay Weapon_Sword en la escena.\nNullBorn → Build Art Test Scene, Play, Pause.\nSi la espada está en la mano, pulsa R para envainar y vuelve a este menú.",
                    "OK");
                return;
            }

            if (sword.parent == null || sword.parent.name != WeaponHolsterName)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — Holster pose",
                    "Weapon_Sword está bajo '" + (sword.parent != null ? sword.parent.name : "(null)")
                    + "', no WeaponHolster.\nPulsa R en Play para envainar, Pause, y captura de nuevo.",
                    "OK");
                return;
            }

            Selection.activeTransform = sword;
            var p = sword.localPosition;
            var q = sword.localRotation;
            var s = sword.localScale;
            if (!WriteTaggedPose("Cap0SwordHolster", "SwordHolsterCaptured", "SwordHolster", p, q, s))
            {
                Debug.LogError("[NullBorn][Cap0] Could not write holster constants to Cap0ArtSetup.cs.");
                return;
            }

            LogCapturedPose("holster pose (WeaponHolster local)", p, q, s);
            EditorUtility.DisplayDialog(
                "NullBorn — Holster pose captured",
                "Valores escritos en Cap0ArtSetup.\nNullBorn → Build Art Test Scene para dejarlos en la escena.",
                "OK");
        }

        /// <summary>
        /// Selection first, then any Weapon_Sword in the loaded scenes (Play or Edit).
        /// Holster capture prefers the instance parented to WeaponHolster.
        /// </summary>
        static Transform FindSwordInEditor(bool preferHolster)
        {
            Transform selected = null;
            if (Selection.activeTransform != null)
                selected = FindSwordUnder(Selection.activeTransform);
            if (selected == null)
            {
                foreach (var go in Selection.gameObjects)
                {
                    selected = FindSwordUnder(go.transform);
                    if (selected != null) break;
                }
            }

            if (preferHolster && selected != null && selected.parent != null
                && selected.parent.name == WeaponHolsterName)
                return selected;

            Transform sheathed = null;
            Transform any = selected;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name != WeaponSwordName) continue;
                any ??= t;
                if (t.parent != null && t.parent.name == WeaponHolsterName)
                    sheathed = t;
            }

            if (preferHolster)
                return sheathed != null ? sheathed : any;
            return selected != null ? selected : any;
        }

        static Transform FindSwordUnder(Transform t)
        {
            if (t == null) return null;

            var p = t;
            while (p != null)
            {
                if (p.name == WeaponSwordName) return p;
                p = p.parent;
            }

            foreach (var c in t.GetComponentsInChildren<Transform>(true))
            {
                if (c.name == WeaponSwordName) return c;
            }

            return null;
        }

        static void LogCapturedPose(string label, Vector3 p, Quaternion q, Vector3 s)
        {
            var e = q.eulerAngles;
            Debug.Log(
                "[NullBorn][Cap0] Captured " + label + " → Cap0ArtSetup constants. " +
                $"pos=({p.x:F4}, {p.y:F4}, {p.z:F4}) rot=Euler({e.x:F2}, {e.y:F2}, {e.z:F2}) " +
                $"quat=({q.x:F4}, {q.y:F4}, {q.z:F4}, {q.w:F4}) scale=({s.x:F4}, {s.y:F4}, {s.z:F4}). " +
                "Rebuild Art Test Scene to bake into ArtTest.");
        }

        /// <summary>
        /// Drawn pose under hand_r. Saved for Combat/; Cap 0 locomotion never calls this.
        /// </summary>
        public static void ApplyDrawnGrip(GameObject sword)
        {
            if (sword == null) return;

            if (!SwordGripCaptured)
            {
                Debug.LogWarning(
                    "[NullBorn][Cap0] Drawn grip NOT captured. Parent Weapon_Sword to hand_r, gizmo the handle into the fist, "
                    + "then NullBorn → Cap0 → Capture Sword Grip From Selection.");
                return;
            }

            sword.transform.localPosition = new Vector3(SwordGripPx, SwordGripPy, SwordGripPz);
            sword.transform.localRotation = new Quaternion(SwordGripQx, SwordGripQy, SwordGripQz, SwordGripQw);
            sword.transform.localScale = new Vector3(SwordGripSx, SwordGripSy, SwordGripSz);
            var e = sword.transform.localEulerAngles;
            Debug.Log(
                "[NullBorn][Cap0] Drawn grip (hand_r, Combat/ only): "
                + $"localPosition=({SwordGripPx:F4}, {SwordGripPy:F4}, {SwordGripPz:F4}) "
                + $"localRotation=Euler({e.x:F2}, {e.y:F2}, {e.z:F2}) "
                + $"localScale=({SwordGripSx:F4}, {SwordGripSy:F4}, {SwordGripSz:F4})");
        }

        /// <summary>Alias kept so older editor calls still compile. Cap 0 loco does not use this.</summary>
        public static void ApplyCapturedSwordGrip(GameObject sword) => ApplyDrawnGrip(sword);

        /// <summary>
        /// Empty socket on hips / spine. Sword is parented here for idle/walk/run.
        /// </summary>
        public static Transform EnsureWeaponHolster(Animator animator, Transform visualRoot)
        {
            var bone = FindHolsterBone(animator, visualRoot);
            if (bone == null)
            {
                Debug.LogWarning("[NullBorn][Cap0] Holster bone not found — WeaponHolster parented to Visual.");
                bone = visualRoot;
            }

            var existing = bone.Find(WeaponHolsterName);
            if (existing != null)
                return existing;

            foreach (var t in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == WeaponHolsterName)
                    Object.DestroyImmediate(t.gameObject);
            }

            var go = new GameObject(WeaponHolsterName);
            go.transform.SetParent(bone, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            Debug.Log("[NullBorn][Cap0] WeaponHolster on bone '" + bone.name + "' (sheathed on chest/spine; Draw → hand_r).");
            return go.transform;
        }

        static Transform FindHolsterBone(Animator animator, Transform visualRoot)
        {
            // Chest/spine — not Hips. A high offset on hips is a lever that clips the hood in run.
            if (animator != null && animator.isHuman)
            {
                var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
                if (chest != null) return chest;
                var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                if (spine != null) return spine;
                var upper = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                if (upper != null) return upper;
            }

            if (visualRoot == null) return null;
            foreach (var t in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                var n = t.name;
                if (n.Equals("spine_02", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("spine_01", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("Spine", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("chest", StringComparison.OrdinalIgnoreCase))
                    return t;
            }

            foreach (var t in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                var n = t.name;
                if (n.Equals("hips", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("pelvis", StringComparison.OrdinalIgnoreCase))
                    return t;
            }

            return null;
        }

        /// <summary>
        /// Cap 0 default: sheath on WeaponHolster. Strips colliders (no overlap vs hair until Combat/).
        /// Does not parent to hand_r.
        /// </summary>
        public static void HolsterEquippedSword(GameObject sword, Animator animator, Transform visualRoot)
        {
            if (sword == null) return;

            sword.name = WeaponSwordName;
            foreach (var col in sword.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);

            ApplyWeaponMaterials(sword);
            DestroyExtraSwords(visualRoot, sword);

            var holster = EnsureWeaponHolster(animator, visualRoot);
            sword.transform.SetParent(holster, false);
            ApplyHolsterPose(sword, visualRoot);
        }

        static void DestroyExtraSwords(Transform visualRoot, GameObject keep)
        {
            if (visualRoot == null || keep == null) return;
            var all = visualRoot.GetComponentsInChildren<Transform>(true);
            for (var i = all.Length - 1; i >= 0; i--)
            {
                var t = all[i];
                if (t == null || t.name != WeaponSwordName) continue;
                if (t.gameObject != keep)
                    Object.DestroyImmediate(t.gameObject);
            }
        }

        public static void ApplyHolsterPose(GameObject sword, Transform visualRoot)
        {
            if (sword == null) return;

            sword.transform.localRotation = new Quaternion(
                SwordHolsterQx, SwordHolsterQy, SwordHolsterQz, SwordHolsterQw);
            sword.transform.localScale = new Vector3(SwordHolsterSx, SwordHolsterSy, SwordHolsterSz);

            if (SwordHolsterCaptured)
            {
                sword.transform.localPosition = new Vector3(SwordHolsterPx, SwordHolsterPy, SwordHolsterPz);
                var e = sword.transform.localEulerAngles;
                Debug.Log(
                    "[NullBorn][Cap0] Holster pose (captured, under WeaponHolster): "
                    + $"localPosition=({SwordHolsterPx:F4}, {SwordHolsterPy:F4}, {SwordHolsterPz:F4}) "
                    + $"localRotation=Euler({e.x:F2}, {e.y:F2}, {e.z:F2}) "
                    + $"localScale=({SwordHolsterSx:F4}, {SwordHolsterSy:F4}, {SwordHolsterSz:F4})");
                return;
            }

            var character = visualRoot != null ? visualRoot : sword.transform.root;
            var holster = sword.transform.parent;
            var origin = holster != null ? holster.position : character.position;
            // Further off the back than the old hips pose (0.12m) so run does not clip hood/cape.
            sword.transform.position = origin
                                       + character.right * 0.18f
                                       + character.up * 0.02f
                                       + character.forward * -0.28f;

            Debug.LogWarning(
                "[NullBorn][Cap0] Holster pose NOT captured (Chest parent, back clearance 0.28). "
                + "Pause Play → select Weapon_Sword → Move/Rotate further off the back, no hood/torso clip → "
                + "NullBorn → Cap0 → Capture Holster Pose From Selection → Rebuild Art Test.");
        }

        static bool WriteTaggedPose(string tag, string capturedConst, string prefix, Vector3 p, Quaternion q, Vector3 s)
        {
            var path = Path.Combine(Application.dataPath, "Editor/Cap0ArtSetup.cs").Replace('\\', '/');
            if (!File.Exists(path)) return false;

            var src = File.ReadAllText(path);
            var open = "// <" + tag + ">";
            var close = "// </" + tag + ">";
            var start = src.IndexOf(open, StringComparison.Ordinal);
            var end = src.IndexOf(close, StringComparison.Ordinal);
            if (start < 0 || end < 0 || end <= start) return false;

            string F(float v)
            {
                return v.ToString("0.########", CultureInfo.InvariantCulture) + "f";
            }

            var block =
                open + "\r\n"
                + "        public static readonly bool " + capturedConst + " = true;\r\n"
                + "        public const float " + prefix + "Px = " + F(p.x) + ";\r\n"
                + "        public const float " + prefix + "Py = " + F(p.y) + ";\r\n"
                + "        public const float " + prefix + "Pz = " + F(p.z) + ";\r\n"
                + "        public const float " + prefix + "Qx = " + F(q.x) + ";\r\n"
                + "        public const float " + prefix + "Qy = " + F(q.y) + ";\r\n"
                + "        public const float " + prefix + "Qz = " + F(q.z) + ";\r\n"
                + "        public const float " + prefix + "Qw = " + F(q.w) + ";\r\n"
                + "        public const float " + prefix + "Sx = " + F(s.x) + ";\r\n"
                + "        public const float " + prefix + "Sy = " + F(s.y) + ";\r\n"
                + "        public const float " + prefix + "Sz = " + F(s.z) + ";\r\n"
                + "        " + close;

            var nl = src.Contains("\r\n") ? "\r\n" : "\n";
            if (nl == "\n")
                block = block.Replace("\r\n", "\n");

            src = src.Substring(0, start) + block + src.Substring(end + close.Length);
            File.WriteAllText(path, src);
            AssetDatabase.Refresh();
            return true;
        }

        public static void EnsureInteractableTagAndLayer()
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            var tags = tagManager.FindProperty("tags");
            var hasTag = false;
            for (var i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == InteractableHighlight.TagName)
                    hasTag = true;
            }

            if (!hasTag)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = InteractableHighlight.TagName;
            }

            var layers = tagManager.FindProperty("layers");
            if (layers != null && layers.arraySize > 6)
            {
                var slot = layers.GetArrayElementAtIndex(6);
                if (string.IsNullOrEmpty(slot.stringValue))
                    slot.stringValue = InteractableHighlight.LayerName;
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tagManager.targetObject);
        }

        public static void EnsureAlwaysIncludedRimShader()
        {
            var shader = Shader.Find(RimShaderName);
            if (shader == null) return;

            var graphics = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
            var so = new SerializedObject(graphics);
            var list = so.FindProperty("m_AlwaysIncludedShaders");
            if (list == null) return;

            for (var i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    return;
            }

            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = shader;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(graphics);
        }

        public static void ConfigureRangerImport()
        {
            var importer = AssetImporter.GetAtPath(RangerFbx) as ModelImporter;
            if (importer == null) return;

            var dirty = false;
            if (importer.importAnimation)
            {
                importer.importAnimation = false;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
                Debug.Log("[NullBorn] Ranger outfit import: animation off (skinned to Superhero host).");
            }
        }

        public static void ConfigureHairImport()
        {
            var importer = AssetImporter.GetAtPath(HairBuzzedFbx) as ModelImporter;
            if (importer == null) return;

            var dirty = false;
            if (importer.importAnimation)
            {
                importer.importAnimation = false;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
                Debug.Log("[NullBorn] Hair_Buzzed import: animation off (skinned to Superhero host).");
            }
        }

        public static void PaintGround(GameObject ground)
        {
            if (ground == null) return;
            EnsureFolder(MatFolder);

            var grass = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ThirdParty/Nature/Textures/Grass.png");
            var mat = GetOrCreateLit("Cap0_Ground", grass, null, clip: false, tint: Grass);
            mat.SetFloat("_Smoothness", 0.12f);
            EditorUtility.SetDirty(mat);

            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mat;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
        }

        public static void ApplyAtmosphere(Camera cam, Light sun)
        {
            if (sun != null && sun.shadows != LightShadows.None)
                sun.shadows = LightShadows.Soft;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(Sky.r * 0.72f, Sky.g * 0.74f, Sky.b * 0.70f);
            RenderSettings.skybox = null;
            RenderSettings.fog = false;

            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Sky;
            cam.allowHDR = false;
            cam.allowMSAA = true;
        }

        public static void ApplyEnvironmentMaterials(GameObject root)
        {
            if (root == null) return;
            EnsureFolder(MatFolder);

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[NullBorn] URP Lit shader missing — cannot unify environment materials.");
                return;
            }

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == "InteractableRim") continue;

                var sourceMats = renderer.sharedMaterials;
                var next = new Material[sourceMats.Length];
                for (var i = 0; i < sourceMats.Length; i++)
                {
                    var source = sourceMats[i];
                    next[i] = ToCap0Lit(source, urpLit, renderer.gameObject.name);
                }

                renderer.sharedMaterials = next;
                ClampMaterialsToSubmeshes(renderer);
            }
        }

        public static void MarkInteractable(GameObject go)
        {
            if (go == null) return;
            EnsureAlwaysIncludedRimShader();
            InteractableHighlight.Apply(go);
        }

        public static void DressHeroWithRanger(GameObject visual, Animator hostAnimator)
        {
            if (visual == null) return;

            ConfigureRangerImport();
            ConfigureHairImport();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RangerFbx);
            if (prefab == null)
            {
                Debug.LogWarning("[NullBorn] Missing Ranger outfit FBX: " + RangerFbx);
                ApplyHostSkinAndVisibility(visual, rangerEquipped: false);
                LogPlayerVisual(visual);
                return;
            }

            var outfit = (GameObject)PrefabUtility.InstantiatePrefab(prefab, visual.transform);
            if (PrefabUtility.IsPartOfPrefabInstance(outfit))
                PrefabUtility.UnpackPrefabInstance(outfit, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            outfit.name = "Outfit_Ranger";
            outfit.transform.localPosition = Vector3.zero;
            outfit.transform.localRotation = Quaternion.identity;
            outfit.transform.localScale = Vector3.one;

            foreach (var col in outfit.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);

            foreach (var anim in outfit.GetComponentsInChildren<Animator>(true))
            {
                if (hostAnimator != null && anim == hostAnimator) continue;
                anim.enabled = false;
                anim.runtimeAnimatorController = null;
            }

            var boneMap = BuildHostBoneMap(visual, outfit.transform);
            var remapped = 0;
            var unmatched = 0;
            foreach (var smr in outfit.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                unmatched += RemapSmrBones(smr, boneMap, ref remapped);

            var snapshots = SnapshotSmrBones(visual);
            ApplyRangerMaterials(outfit);
            StripOutfitSkeletonKeepMeshes(outfit, hostAnimator);

            // Rebind the host Humanoid AFTER stripping the outfit Armature (avoids T-pose),
            // then restore every SMR bone array by name. Rebind-last used to leave Head/Eyes
            // with null bones (enabled mesh, empty hood).
            if (hostAnimator != null)
            {
                hostAnimator.Rebind();
                hostAnimator.Update(0f);
            }

            boneMap = BuildHostBoneMap(visual, outfit.transform);
            var bound = RestoreSmrBones(visual, boneMap, snapshots);

            ApplyHostSkinAndVisibility(visual, rangerEquipped: true);
            EnsureFaceFromSuperheroBody(visual, boneMap);
            EnsureHairBuzzed(visual, boneMap, hostAnimator);
            EnsureHandsFromSuperheroBody(visual, boneMap);
            ForceOpaqueCharacterMaterials(visual);

            if (hostAnimator != null)
                hostAnimator.Update(0f);

            LogSkinProbe(visual);
            LogPlayerVisual(visual);

            Debug.Log(
                $"[NullBorn] Ranger outfit bound to Superhero host (pre-strip remap={remapped}, unmatched={unmatched}, post-rebind binds={bound}). Animator/locomotion unchanged.");
        }

        public static void ApplyWeaponMaterials(GameObject weapon)
        {
            if (weapon == null) return;

            const string albedoA = "Assets/_ThirdParty/Armory/Textures/MAIN_TEXTURE.png";
            const string albedoB = "Assets/_ThirdParty/Armory/FBX/MAIN_TEXTURE.png";
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoA)
                         ?? AssetDatabase.LoadAssetAtPath<Texture2D>(albedoB);
            if (albedo == null)
            {
                Debug.LogWarning(
                    "[NullBorn][Cap0] Sword missing albedo. Looked for:\n  " + albedoA + "\n  " + albedoB);
            }

            var mat = GetOrCreateLit("Cap0_Sword", albedo, null, clip: false, tint: Color.white);
            foreach (var renderer in weapon.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponent<TextMesh>() != null) continue;
                renderer.sharedMaterials = new[] { mat };
                ClampMaterialsToSubmeshes(renderer);
            }
        }

        public static void LogPlayerVisual(GameObject visual)
        {
            if (visual == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("[NullBorn][Cap0] Player renderers:");
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var meshName = "-";
                if (renderer is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                    meshName = smr.sharedMesh.name;
                else
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                        meshName = filter.sharedMesh.name;
                }

                var mat0 = renderer.sharedMaterial;
                var tex = GetTex(mat0, "_BaseMap");
                var shader = mat0 != null && mat0.shader != null ? mat0.shader.name : "(null)";
                sb.AppendLine(
                    $"  {(renderer.enabled ? "ON " : "OFF")} {GetPath(renderer.transform)}  mesh={meshName}  shader={shader}  tex={(tex != null ? tex.name : "-")}  outline={HasOutline(mat0)}");
            }

            Debug.Log(sb.ToString());
        }

        public static void LogSceneAudit()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[NullBorn][Cap0] Material audit (open scene):");
            sb.AppendLine(
                $"  palette grass=#{ColorUtility.ToHtmlStringRGB(Grass)} leaf=#{ColorUtility.ToHtmlStringRGB(Leaf)} earth=#{ColorUtility.ToHtmlStringRGB(Earth)} wood=#{ColorUtility.ToHtmlStringRGB(Wood)} sky=#{ColorUtility.ToHtmlStringRGB(Sky)} hero=#{ColorUtility.ToHtmlStringRGB(HeroAccent)}");
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
            {
                var go = renderer.gameObject;
                if (go.GetComponent<TextMesh>() != null) continue;
                if (go.GetComponentInParent<Canvas>() != null) continue;

                var mats = renderer.sharedMaterials;
                for (var i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    var shaderName = mat != null && mat.shader != null ? mat.shader.name : "(null)";
                    var outline = HasOutline(mat);
                    var clip = mat != null && mat.IsKeywordEnabled("_ALPHATEST_ON");
                    var tex = GetTex(mat, "_BaseMap") ?? GetTex(mat, "_MainTex");
                    sb.AppendLine(
                        $"  {GetPath(go.transform)}  slot={i}  shader={shaderName}  outline={outline}  clip={clip}  tex={(tex != null ? tex.name : "-")}");
                }
            }

            Debug.Log(sb.ToString());
        }

        struct SmrBoneSnap
        {
            public SkinnedMeshRenderer Smr;
            public string[] BoneNames;
            public string RootName;
        }

        /// <summary>
        /// Host Humanoid bones only (Armature). Skip Renderer transforms so a Head
        /// mesh slot never shadows the Head LimbNode.
        /// </summary>
        static Dictionary<string, Transform> BuildHostBoneMap(GameObject visual, Transform excludeOutfit)
        {
            var map = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
            if (visual == null) return map;

            Transform armature = null;
            foreach (var t in visual.GetComponentsInChildren<Transform>(true))
            {
                if (excludeOutfit != null && t != excludeOutfit && t.IsChildOf(excludeOutfit)) continue;
                if (t.name == "Armature")
                {
                    armature = t;
                    break;
                }
            }

            var root = armature != null ? armature : visual.transform;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (excludeOutfit != null && t != excludeOutfit && t.IsChildOf(excludeOutfit)) continue;
                if (t.GetComponent<Renderer>() != null) continue;
                if (!map.ContainsKey(t.name))
                    map[t.name] = t;
            }

            return map;
        }

        static int RemapSmrBones(SkinnedMeshRenderer smr, Dictionary<string, Transform> boneMap, ref int remapped)
        {
            if (smr == null || boneMap == null) return 0;
            var bones = smr.bones;
            if (bones == null) return 0;

            var mapped = new Transform[bones.Length];
            var unmatched = 0;
            for (var i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null) continue;
                if (boneMap.TryGetValue(bones[i].name, out var hostBone))
                {
                    mapped[i] = hostBone;
                    remapped++;
                }
                else
                {
                    mapped[i] = bones[i];
                    unmatched++;
                }
            }

            smr.bones = mapped;
            if (smr.rootBone != null && boneMap.TryGetValue(smr.rootBone.name, out var rootBone))
                smr.rootBone = rootBone;
            smr.updateWhenOffscreen = true;
            smr.skinnedMotionVectors = false;
            smr.quality = SkinQuality.Bone4;
            return unmatched;
        }

        static List<SmrBoneSnap> SnapshotSmrBones(GameObject visual)
        {
            var list = new List<SmrBoneSnap>();
            if (visual == null) return list;
            foreach (var smr in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var bones = smr.bones;
                var names = new string[bones != null ? bones.Length : 0];
                for (var i = 0; i < names.Length; i++)
                    names[i] = bones[i] != null ? bones[i].name : null;
                list.Add(new SmrBoneSnap
                {
                    Smr = smr,
                    BoneNames = names,
                    RootName = smr.rootBone != null ? smr.rootBone.name : null
                });
            }

            return list;
        }

        static int RestoreSmrBones(GameObject visual, Dictionary<string, Transform> boneMap, List<SmrBoneSnap> snapshots)
        {
            var bound = 0;
            if (snapshots == null) return 0;
            foreach (var snap in snapshots)
            {
                if (snap.Smr == null) continue;
                bound += BindSmrToHost(snap.Smr, boneMap, snap.BoneNames, snap.RootName);
            }

            return bound;
        }

        static int BindSmrToHost(
            SkinnedMeshRenderer smr,
            Dictionary<string, Transform> boneMap,
            string[] snapshotNames,
            string snapshotRoot)
        {
            if (smr == null || boneMap == null) return 0;

            string[] names = snapshotNames;
            if (names == null || names.Length == 0)
            {
                var bones = smr.bones;
                if (bones == null || bones.Length == 0) return 0;
                names = new string[bones.Length];
                for (var i = 0; i < bones.Length; i++)
                    names[i] = bones[i] != null ? bones[i].name : null;
            }

            var mapped = new Transform[names.Length];
            var n = 0;
            var missing = 0;
            for (var i = 0; i < names.Length; i++)
            {
                if (string.IsNullOrEmpty(names[i])) continue;
                if (boneMap.TryGetValue(names[i], out var t))
                {
                    mapped[i] = t;
                    n++;
                }
                else missing++;
            }

            smr.bones = mapped;
            var rootName = snapshotRoot;
            if (string.IsNullOrEmpty(rootName) && smr.rootBone != null)
                rootName = smr.rootBone.name;
            if (!string.IsNullOrEmpty(rootName) && boneMap.TryGetValue(rootName, out var root))
                smr.rootBone = root;
            else
            {
                for (var i = 0; i < mapped.Length; i++)
                {
                    if (mapped[i] == null) continue;
                    smr.rootBone = mapped[i];
                    break;
                }
            }

            smr.updateWhenOffscreen = true;
            smr.quality = SkinQuality.Bone4;
            if (missing > 0)
                Debug.LogWarning($"[NullBorn][Cap0] {smr.name}: {missing} bones not on host Armature.");
            return n;
        }

        /// <summary>
        /// This Standard FREE Base has Superhero FullBody only (no Regular Male FBX).
        /// FBX 'Head' is a LimbNode, not a mesh — face/hands live on SuperHero_Male.
        /// Extract head weights (not neck — ranger collar covers the neckline). Do not substitute primitives.
        /// </summary>
        static void EnsureFaceFromSuperheroBody(GameObject visual, Dictionary<string, Transform> boneMap)
        {
            if (visual == null) return;

            for (var i = visual.transform.childCount - 1; i >= 0; i--)
            {
                var child = visual.transform.GetChild(i);
                if (child.name != "Head") continue;
                if (child.GetComponent<SkinnedMeshRenderer>() == null) continue;
                if (HasAncestorNamed(child, "Armature")) continue;
                Object.DestroyImmediate(child.gameObject);
            }

            SkinnedMeshRenderer body = null;
            var hasHeadMesh = false;
            foreach (var smr in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.name == "SuperHero_Male") body = smr;
                if (smr.name == "Head" && smr.sharedMesh != null && smr.sharedMesh.vertexCount > 0)
                    hasHeadMesh = true;
            }

            if (hasHeadMesh)
            {
                Debug.Log("[NullBorn][Cap0] Head mesh slot already present.");
                return;
            }

            Debug.LogWarning(
                "[NullBorn][Cap0] No Regular Male Head FBX in this Standard Base. " +
                "Superhero_Male_FullBody Head is a LimbNode (bone), not a skinned mesh; face is part of SuperHero_Male. " +
                "Extracting Head (no neck) from SuperHero_Male so the ranger collar stays clean. " +
                "For pack-accurate Regular Male head, import Universal Base Characters Regular Male (head only).");

            if (body == null || body.sharedMesh == null)
            {
                Debug.LogWarning("[NullBorn][Cap0] SuperHero_Male SMR missing — cannot extract face.");
                return;
            }

            if (!body.sharedMesh.isReadable)
            {
                Debug.LogWarning(
                    "[NullBorn][Cap0] SuperHero_Male mesh is not readable. Rebuild Art Test Scene (hero Read/Write).");
                return;
            }

            var mesh = ExtractSkinnedByBones(body, IsFaceBone, 0.40f);
            if (mesh == null || mesh.vertexCount < 24)
                mesh = ExtractSkinnedByBones(body, IsFaceBone, 0.22f);
            if (mesh == null)
            {
                Debug.LogWarning(
                    "[NullBorn][Cap0] Superhero Head cannot be separated in this Standard pack. Import Regular Male head.");
                return;
            }

            ShrinkMeshAroundCenter(mesh, 0.97f);

            var go = new GameObject("Head");
            go.transform.SetParent(visual.transform, false);
            go.transform.localPosition = body.transform.localPosition;
            go.transform.localRotation = body.transform.localRotation;
            // Collar clip: Head slightly smaller than Superhero bind pose. Body untouched.
            go.transform.localScale = body.transform.localScale * 0.97f;

            var face = go.AddComponent<SkinnedMeshRenderer>();
            face.sharedMesh = mesh;
            face.bones = body.bones;
            face.rootBone = body.rootBone;
            BindSmrToHost(
                face,
                boneMap,
                BoneNamesOf(body),
                body.rootBone != null ? body.rootBone.name : "Head");
            if (boneMap != null && boneMap.TryGetValue("Head", out var headBone))
                face.rootBone = headBone;

            var skinBase = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Ligh.png");
            var skinNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Normal.png");
            WarnIfMissing(skinBase, "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Ligh.png");
            face.sharedMaterials = new[]
            {
                GetOrCreateLit("Cap0_HeroSkin", skinBase, skinNormal, clip: false, tint: Color.white)
            };
            face.enabled = true;
        }

        /// <summary>
        /// Superhero FullBody has no Hair mesh. Hood ON → Hair_Buzzed only
        /// (never Long / Buns / SimpleParted). Same host Humanoid bones as the face.
        /// </summary>
        static void EnsureHairBuzzed(GameObject visual, Dictionary<string, Transform> boneMap, Animator hostAnimator)
        {
            if (visual == null) return;

            for (var i = visual.transform.childCount - 1; i >= 0; i--)
            {
                var child = visual.transform.GetChild(i);
                if (child.name == "Hair_Buzzed" || child.name == "Hair_Long"
                    || child.name == "Hair_Buns" || child.name == "Hair_SimpleParted")
                    Object.DestroyImmediate(child.gameObject);
            }

            ConfigureHairImport();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HairBuzzedFbx);
            if (prefab == null)
            {
                Debug.LogError(
                    "[NullBorn][Cap0] Missing Hair_Buzzed FBX: " + HairBuzzedFbx
                    + " — scalp would stay bald. Hair stays attempted-ON; not disabled.");
                return;
            }

            var hair = (GameObject)PrefabUtility.InstantiatePrefab(prefab, visual.transform);
            if (PrefabUtility.IsPartOfPrefabInstance(hair))
                PrefabUtility.UnpackPrefabInstance(hair, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            hair.name = "Hair_Buzzed";
            hair.transform.localPosition = Vector3.zero;
            hair.transform.localRotation = Quaternion.identity;
            hair.transform.localScale = Vector3.one * 0.96f;

            foreach (var col in hair.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);

            foreach (var anim in hair.GetComponentsInChildren<Animator>(true))
            {
                if (hostAnimator != null && anim == hostAnimator) continue;
                anim.enabled = false;
                anim.runtimeAnimatorController = null;
            }

            var map = boneMap ?? BuildHostBoneMap(visual, hair.transform);
            var remapped = 0;
            foreach (var smr in hair.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                RemapSmrBones(smr, map, ref remapped);

            StripOutfitSkeletonKeepMeshes(hair, hostAnimator);
            foreach (var smr in hair.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                BindSmrToHost(smr, map, BoneNamesOf(smr), smr.rootBone != null ? smr.rootBone.name : "Head");
                if (map.TryGetValue("Head", out var headBone))
                    smr.rootBone = headBone;
                smr.enabled = true;
                smr.updateWhenOffscreen = true;
            }

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(
                             "Assets/_ThirdParty/Characters/Hairstyles/Textures/T_Hair_1_BaseColor.png")
                         ?? AssetDatabase.LoadAssetAtPath<Texture2D>(
                             "Assets/_ThirdParty/Characters/Base/Textures/T_Hair_1_BaseColor.png");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                             "Assets/_ThirdParty/Characters/Hairstyles/Textures/T_Hair_1_Normal.png")
                         ?? AssetDatabase.LoadAssetAtPath<Texture2D>(
                             "Assets/_ThirdParty/Characters/Base/Textures/T_Hair_1_Normal.png");
            WarnIfMissing(albedo, "Assets/_ThirdParty/Characters/Hairstyles/Textures/T_Hair_1_BaseColor.png");
            var mat = GetOrCreateLit("Cap0_HairBuzzed", albedo, normal, clip: false, tint: Color.white);
            foreach (var renderer in hair.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = true;
                renderer.sharedMaterials = new[] { mat };
            }

            hair.SetActive(true);
            Debug.Log(
                "[NullBorn][Cap0] Hair_Buzzed ON under hood (scale 0.96, rootBone=Head, MI_Hair_1 → Cap0_HairBuzzed). "
                + "Superhero FullBody has no Hair slot. Long/Buns/SimpleParted not instanced. "
                + "Eyebrows: Superhero base (Eyebrows_Regular not swapped). "
                + "Hair stays ON — never disabled for sword overlap / z-sort. If the hood clips the crown, report it.");
        }

        static void EnsureHandsFromSuperheroBody(GameObject visual, Dictionary<string, Transform> boneMap)
        {
            if (visual == null) return;

            SkinnedMeshRenderer body = null;
            SkinnedMeshRenderer arms = null;
            Transform existing = visual.transform.Find("Cap0_Hands");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            foreach (var smr in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.name == "SuperHero_Male") body = smr;
                if (smr.name == "Male_Ranger_Arms") arms = smr;
            }

            if (arms != null && arms.sharedMesh != null && arms.sharedMesh.subMeshCount >= 2)
            {
                Debug.Log(
                    $"[NullBorn][Cap0] Male_Ranger_Arms submeshes={arms.sharedMesh.subMeshCount} mats={arms.sharedMaterials.Length} — using pack Regular Male skin (no Superhero hand extract).");
                return;
            }

            if (body == null || body.sharedMesh == null || !body.sharedMesh.isReadable)
            {
                Debug.LogWarning("[NullBorn][Cap0] Cannot extract Superhero hands (no readable SuperHero_Male).");
                return;
            }

            var mesh = ExtractSkinnedByBones(body, IsHandBone, 0.35f);
            if (mesh == null)
            {
                Debug.LogWarning("[NullBorn][Cap0] Superhero hand weights not separable; hands depend on Male_Ranger_Arms Regular Male submesh.");
                return;
            }

            mesh.name = "Cap0_Hands";
            var go = new GameObject("Cap0_Hands");
            go.transform.SetParent(visual.transform, false);
            go.transform.localPosition = body.transform.localPosition;
            go.transform.localRotation = body.transform.localRotation;
            go.transform.localScale = body.transform.localScale;

            var hands = go.AddComponent<SkinnedMeshRenderer>();
            hands.sharedMesh = mesh;
            hands.bones = body.bones;
            hands.rootBone = body.rootBone;
            BindSmrToHost(hands, boneMap, BoneNamesOf(body), body.rootBone != null ? body.rootBone.name : null);

            var skinAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                 "Assets/_ThirdParty/Characters/Outfits/Textures/Ranger/Base Chars/T_Regular_Male_Dark_BaseColor.png")
                             ?? AssetDatabase.LoadAssetAtPath<Texture2D>(
                                 "Assets/_ThirdParty/Characters/Outfits/Textures/Base/T_Regular_Male_Dark_BaseColor.png");
            var skinNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                                 "Assets/_ThirdParty/Characters/Outfits/Textures/Ranger/Base Chars/T_Regular_Male_Normal.png")
                             ?? AssetDatabase.LoadAssetAtPath<Texture2D>(
                                 "Assets/_ThirdParty/Characters/Outfits/Textures/Base/T_Regular_Male_Normal.png");
            hands.sharedMaterials = new[]
            {
                GetOrCreateLit("Cap0_RangerSkin", skinAlbedo, skinNormal, clip: false, tint: Color.white)
            };
            hands.enabled = true;
            Debug.Log("[NullBorn][Cap0] Extracted Superhero hand weights because Arms has no skin submesh. Textures: T_Regular_Male_* from the pack.");
        }

        static bool IsHandBone(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var n = name.ToLowerInvariant();
            return n.StartsWith("hand_") || n.StartsWith("thumb_") || n.StartsWith("index_")
                   || n.StartsWith("middle_") || n.StartsWith("ring_") || n.StartsWith("pinky_")
                   || n.Contains("finger");
        }

        static string[] BoneNamesOf(SkinnedMeshRenderer smr)
        {
            var bones = smr != null ? smr.bones : null;
            var names = new string[bones != null ? bones.Length : 0];
            for (var i = 0; i < names.Length; i++)
                names[i] = bones[i] != null ? bones[i].name : null;
            return names;
        }

        static bool IsFaceBone(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var n = name.ToLowerInvariant();
            // Neck stays on Male_Ranger_Body collar — do not extract neck_* (clips the round neckline).
            return n == "head" || n.Contains("jaw") || (n.Contains("face") && !n.Contains("surface"));
        }

        static void ShrinkMeshAroundCenter(Mesh mesh, float scale)
        {
            if (mesh == null || scale >= 0.999f) return;
            var verts = mesh.vertices;
            var c = mesh.bounds.center;
            for (var i = 0; i < verts.Length; i++)
                verts[i] = c + (verts[i] - c) * scale;
            mesh.vertices = verts;
            mesh.RecalculateBounds();
        }

        static Mesh ExtractSkinnedByBones(SkinnedMeshRenderer source, Func<string, bool> bonePredicate, float minWeight)
        {
            var src = source != null ? source.sharedMesh : null;
            var bones = source != null ? source.bones : null;
            if (src == null || bones == null || bones.Length == 0 || !src.isReadable) return null;

            var boneKeep = new bool[bones.Length];
            var anyBone = false;
            for (var i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null) continue;
                boneKeep[i] = bonePredicate(bones[i].name);
                if (boneKeep[i]) anyBone = true;
            }

            if (!anyBone) return null;

            var bw = src.boneWeights;
            if (bw == null || bw.Length != src.vertexCount) return null;

            var keepVert = new bool[src.vertexCount];
            var kept = 0;
            for (var i = 0; i < src.vertexCount; i++)
            {
                var w = bw[i];
                var s = 0f;
                if ((uint)w.boneIndex0 < (uint)boneKeep.Length && boneKeep[w.boneIndex0]) s += w.weight0;
                if ((uint)w.boneIndex1 < (uint)boneKeep.Length && boneKeep[w.boneIndex1]) s += w.weight1;
                if ((uint)w.boneIndex2 < (uint)boneKeep.Length && boneKeep[w.boneIndex2]) s += w.weight2;
                if ((uint)w.boneIndex3 < (uint)boneKeep.Length && boneKeep[w.boneIndex3]) s += w.weight3;
                if (s >= minWeight)
                {
                    keepVert[i] = true;
                    kept++;
                }
            }

            if (kept < 16) return null;

            var remap = new int[src.vertexCount];
            for (var i = 0; i < remap.Length; i++) remap[i] = -1;

            var verts = new List<Vector3>(kept);
            var norms = new List<Vector3>(kept);
            var uvs = new List<Vector2>(kept);
            var weights = new List<BoneWeight>(kept);
            var srcVerts = src.vertices;
            var srcNorms = src.normals;
            var srcUv = src.uv;
            var hasN = srcNorms != null && srcNorms.Length == src.vertexCount;
            var hasUv = srcUv != null && srcUv.Length == src.vertexCount;

            for (var i = 0; i < src.vertexCount; i++)
            {
                if (!keepVert[i]) continue;
                remap[i] = verts.Count;
                verts.Add(srcVerts[i]);
                if (hasN) norms.Add(srcNorms[i]);
                if (hasUv) uvs.Add(srcUv[i]);
                weights.Add(bw[i]);
            }

            var trisSrc = src.triangles;
            var tris = new List<int>();
            for (var i = 0; i + 2 < trisSrc.Length; i += 3)
            {
                var a = remap[trisSrc[i]];
                var b = remap[trisSrc[i + 1]];
                var c = remap[trisSrc[i + 2]];
                if (a < 0 || b < 0 || c < 0) continue;
                tris.Add(a);
                tris.Add(b);
                tris.Add(c);
            }

            if (tris.Count < 12) return null;

            var mesh = new Mesh { name = "Head" };
            mesh.indexFormat = verts.Count > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts);
            if (hasN) mesh.SetNormals(norms);
            if (hasUv) mesh.SetUVs(0, uvs);
            mesh.boneWeights = weights.ToArray();
            mesh.bindposes = src.bindposes;
            mesh.triangles = tris.ToArray();
            if (!hasN) mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static void ForceOpaqueCharacterMaterials(GameObject visual)
        {
            if (visual == null) return;
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var n = renderer.name.ToLowerInvariant();
                var isFace = n == "head" || n == "eyes" || n == "eyebrows"
                             || (n.Contains("eye") && !n.Contains("hood"))
                             || n.Contains("buzzed");
                if (!isFace) continue;
                foreach (var mat in renderer.sharedMaterials)
                    ForceOpaqueLit(mat);
            }
        }

        static void ForceOpaqueLit(Material mat)
        {
            if (mat == null) return;
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null && mat.shader != urpLit)
                mat.shader = urpLit;
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.One);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.Zero);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 1f);
            if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 0f);
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.renderQueue = -1;
            EditorUtility.SetDirty(mat);
        }

        public static void LogSkinProbe(GameObject visual)
        {
            if (visual == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("[NullBorn][Cap0] Skin probe (Head, Eyes, Eyebrows, Male_Ranger_Arms, Male_Ranger_Body):");
            var hits = 0;
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                if (!IsProbeTarget(renderer.name)) continue;
                hits++;
                var smr = renderer as SkinnedMeshRenderer;
                var meshName = smr != null && smr.sharedMesh != null ? smr.sharedMesh.name : "-";
                var root = smr != null && smr.rootBone != null ? smr.rootBone.name : "(null)";
                var bonesNull = smr == null || smr.bones == null;
                var boneCount = !bonesNull ? smr.bones.Length : 0;
                var nullEntries = 0;
                if (!bonesNull)
                {
                    foreach (var b in smr.bones)
                    {
                        if (b == null) nullEntries++;
                    }
                }

                var matList = new StringBuilder();
                var mats = renderer.sharedMaterials;
                for (var i = 0; i < mats.Length; i++)
                {
                    if (i > 0) matList.Append(", ");
                    matList.Append(mats[i] != null ? mats[i].name : "(null)");
                }

                sb.AppendLine(
                    $"  {(renderer.enabled ? "ON " : "OFF")} {GetPath(renderer.transform)} parent={renderer.transform.parent?.name} mesh={meshName} rootBone={root} bones==null? {bonesNull} boneCount={boneCount} nullEntries={nullEntries} mats=[{matList}] bounds={renderer.bounds.size}");
            }

            if (hits == 0)
                sb.AppendLine("  (no matching renderers)");
            Debug.Log(sb.ToString());
        }

        static bool IsProbeTarget(string name)
        {
            return name == "Head" || name == "Eyes" || name == "Eyebrows"
                   || name == "Male_Ranger_Arms" || name == "Male_Ranger_Body"
                   || name == "Cap0_Hands" || name == "Hair_Buzzed"
                   || name == "Weapon_Sword";
        }

        /// <summary>
        /// Quaternius Modular Character Outfits (itch.io v2.0): outfits work with
        /// just the Base Character head. Duplicate Armature under the Humanoid
        /// host causes T-pose — keep SMR meshes, delete the outfit skeleton.
        /// </summary>
        static void StripOutfitSkeletonKeepMeshes(GameObject outfit, Animator hostAnimator)
        {
            if (outfit == null) return;

            foreach (var renderer in outfit.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.transform.parent != outfit.transform)
                    renderer.transform.SetParent(outfit.transform, true);
            }

            for (var i = outfit.transform.childCount - 1; i >= 0; i--)
            {
                var child = outfit.transform.GetChild(i);
                if (child.GetComponent<Renderer>() != null) continue;
                Object.DestroyImmediate(child.gameObject);
            }

            foreach (var anim in outfit.GetComponentsInChildren<Animator>(true))
            {
                if (hostAnimator != null && anim == hostAnimator) continue;
                Object.DestroyImmediate(anim);
            }
        }

        /// <summary>
        /// Slot table from pack (no Regular Male Head FBX in this Standard Base).
        /// Superhero_Male_FullBody 'Head' is a LimbNode; the face is on SuperHero_Male.
        /// DressHero extracts a Head SMR from that body mesh (not a primitive).
        ///
        /// Base Superhero_Male_FullBody:
        ///   Head (extracted, no neck) / Eyes / Eyebrows → face (always ON with ranger)
        ///   Hair_Buzzed → ON under hood (Superhero FullBody has no Hair slot)
        ///   SuperHero_Male → combined body/arms/legs/feet/underwear (OFF: replaced by ranger slots)
        ///
        /// Male_Ranger modular slots:
        ///   Body (+ belts) → torso / chest / pelvis
        ///   Arms (+ bracers) → arms; pack includes Regular Male skin on these meshes
        ///   Legs → legs
        ///   Feet_Boots → feet
        ///   Head_Hood → hair only, not the head mesh
        ///   Acc_Pauldron → accessory (no base hide)
        /// </summary>
        static void ApplyHostSkinAndVisibility(GameObject visual, bool rangerEquipped)
        {
            var skinBase = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Ligh.png");
            var skinNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Normal.png");
            var eyeBase = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Eye_Brown.png");
            var eyeNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Eye_Normal.png");
            var hairBase = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Hair_1_BaseColor.png");
            var hairNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Hair_1_Normal.png");

            WarnIfMissing(skinBase, "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Ligh.png");
            WarnIfMissing(eyeBase, "Assets/_ThirdParty/Characters/Base/Textures/T_Eye_Brown.png");

            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                if (HasAncestorNamed(renderer.transform, "Outfit_Ranger")) continue;
                if (HasAncestorNamed(renderer.transform, "Weapon_Sword")) continue;
                if (HasAncestorNamed(renderer.transform, "WeaponHolster")) continue;
                if (HasAncestorNamed(renderer.transform, "Hair_Buzzed"))
                {
                    renderer.enabled = true;
                    renderer.gameObject.SetActive(true);
                    continue;
                }

                var token = RendererToken(renderer);
                var n = token.ToLowerInvariant();

                var isFace = (n.Contains("head") && !n.Contains("hood"))
                             || (n.Contains("eye") && !n.Contains("brow"))
                             || n.Contains("brow") || n.Contains("eyebrow");
                var isBuzzed = n.Contains("buzzed") || renderer.gameObject.name.IndexOf("Buzzed", StringComparison.OrdinalIgnoreCase) >= 0;
                var isHair = !isBuzzed
                             && ((n.Contains("hair") && !n.Contains("brow") && !n.Contains("eyelash"))
                                 || renderer.gameObject.name.StartsWith("Hair", StringComparison.OrdinalIgnoreCase));
                var isUnderwear = n.Contains("underwear") || n.Contains("brief") || n.Contains("undies")
                                  || n.Contains("boxers") || n.Contains("panties");
                var isBodySlot = n.Contains("superhero") || n.Contains("body") || n.Contains("torso")
                                 || n.Contains("chest") || n.Contains("pelvis") || n.Contains("leg")
                                 || n.Contains("arm") || n.Contains("hand") || n.Contains("foot")
                                 || n.Contains("feet");

                bool show;
                if (isFace)
                    show = true;
                else if (isHair || isUnderwear)
                    show = false;
                else if (rangerEquipped && isBodySlot)
                    show = false; // Body+Arms+Legs+Boots replace the FullBody slot
                else
                    show = true;

                renderer.enabled = show;
                if (isHair)
                    renderer.gameObject.SetActive(false);
                if (isBuzzed)
                {
                    renderer.enabled = true;
                    renderer.gameObject.SetActive(true);
                }
                if (isFace)
                    renderer.gameObject.SetActive(true);
                if (!show) continue;

                var isEye = n.Contains("eye") && !n.Contains("brow");
                var isBrow = n.Contains("brow") || n.Contains("eyebrow");
                Texture2D albedo;
                Texture2D normal;
                string key;
                if (isEye)
                {
                    albedo = eyeBase;
                    normal = eyeNormal;
                    key = "Cap0_HeroEye";
                }
                else if (isBrow)
                {
                    albedo = hairBase;
                    normal = hairNormal;
                    key = "Cap0_HeroBrow";
                }
                else
                {
                    albedo = skinBase;
                    normal = skinNormal;
                    key = "Cap0_HeroSkin";
                }

                renderer.sharedMaterials = new[] { GetOrCreateLit(key, albedo, normal, clip: false, tint: Color.white) };
            }
        }

        static void ApplyRangerMaterials(GameObject outfit)
        {
            const string clothAlbedoPath = "Assets/_ThirdParty/Characters/Outfits/Textures/Ranger/T_Ranger_BaseColor.png";
            const string clothNormalPath = "Assets/_ThirdParty/Characters/Outfits/Textures/Ranger/T_Ranger_Normal.png";
            const string skinAlbedoPath = "Assets/_ThirdParty/Characters/Outfits/Textures/Ranger/Base Chars/T_Regular_Male_Dark_BaseColor.png";
            const string skinAlbedoFallback = "Assets/_ThirdParty/Characters/Outfits/Textures/Base/T_Regular_Male_Dark_BaseColor.png";
            const string skinNormalPath = "Assets/_ThirdParty/Characters/Outfits/Textures/Ranger/Base Chars/T_Regular_Male_Normal.png";
            const string skinNormalFallback = "Assets/_ThirdParty/Characters/Outfits/Textures/Base/T_Regular_Male_Normal.png";

            var clothAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(clothAlbedoPath);
            var clothNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(clothNormalPath);
            var skinAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(skinAlbedoPath)
                             ?? AssetDatabase.LoadAssetAtPath<Texture2D>(skinAlbedoFallback);
            var skinNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(skinNormalPath)
                             ?? AssetDatabase.LoadAssetAtPath<Texture2D>(skinNormalFallback);

            WarnIfMissing(clothAlbedo, clothAlbedoPath);
            WarnIfMissing(clothNormal, clothNormalPath);
            WarnIfMissing(skinAlbedo, skinAlbedoPath + " (fallback " + skinAlbedoFallback + ")");

            var cloth = GetOrCreateLit("Cap0_RangerCloth", clothAlbedo, clothNormal, clip: false, tint: Color.white);
            var skin = GetOrCreateLit("Cap0_RangerSkin", skinAlbedo, skinNormal, clip: false, tint: Color.white);

            foreach (var renderer in outfit.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponent<TextMesh>() != null) continue;
                renderer.enabled = true;

                Mesh mesh = null;
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    mesh = skinned.sharedMesh;
                    skinned.updateWhenOffscreen = true;
                    skinned.quality = SkinQuality.Bone4;
                }

                var sourceMats = renderer.sharedMaterials;
                var slots = 1;
                if (mesh != null && mesh.subMeshCount > slots) slots = mesh.subMeshCount;
                if (sourceMats != null && sourceMats.Length > slots) slots = sourceMats.Length;

                var next = new Material[slots];
                var isArms = renderer.name.IndexOf("Arms", StringComparison.OrdinalIgnoreCase) >= 0
                             && renderer.name.IndexOf("Bracer", StringComparison.OrdinalIgnoreCase) < 0;
                for (var i = 0; i < next.Length; i++)
                {
                    var source = sourceMats != null && i < sourceMats.Length ? sourceMats[i] : null;
                    var blob = ((source != null ? source.name : "") + " " + RendererToken(renderer)).ToLowerInvariant();
                    var useSkin = blob.Contains("regular") || blob.Contains("skin");
                    if (isArms && mesh != null && mesh.subMeshCount >= 2)
                        useSkin = i > 0;
                    next[i] = useSkin ? skin : cloth;
                }

                renderer.sharedMaterials = next;
            }
        }

        static string RendererToken(Renderer renderer)
        {
            var token = renderer.name;
            if (renderer is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                token += " " + smr.sharedMesh.name;
            return token;
        }

        static void WarnIfMissing(Texture2D tex, string path)
        {
            if (tex == null)
                Debug.LogWarning("[NullBorn][Cap0] Missing texture (magenta/purple risk): " + path);
        }

        static Material ToCap0Lit(Material source, Shader urpLit, string ownerName)
        {
            Texture albedo = null;
            Texture normal = null;
            var sourceName = source != null ? source.name : "Default";
            var sourceShader = source != null && source.shader != null ? source.shader.name : "(null)";

            if (source != null)
            {
                albedo = GetTex(source, "_BaseMap") ?? GetTex(source, "_MainTex");
                normal = GetTex(source, "_BumpMap") ?? GetTex(source, "_NormalMap");
            }

            if (albedo == null)
                albedo = GuessTexture(sourceName);

            var clip = IsFoliageCutout(sourceName, albedo != null ? albedo.name : "");
            var key = Sanitize((albedo != null ? albedo.name : sourceName) + (clip ? "_Cutout" : "_Lit"));
            var mat = GetOrCreateLit(key, albedo as Texture2D, normal as Texture2D, clip, Color.white);

            if (HasOutlineHint(sourceShader, source))
            {
                Debug.Log(
                    $"[NullBorn][Cap0] Stripped outline/toon on '{ownerName}' mat '{sourceName}' shader='{sourceShader}' → URP Lit {(clip ? "cutout" : "opaque")}.");
            }

            return mat;
        }

        static Material GetOrCreateLit(string key, Texture2D albedo, Texture2D normal, bool clip, Color tint)
        {
            EnsureFolder(MatFolder);
            var path = $"{MatFolder}/{key}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return mat;

            if (mat == null)
            {
                mat = new Material(urpLit) { name = key };
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader != urpLit)
            {
                mat.shader = urpLit;
            }

            if (albedo != null) mat.SetTexture("_BaseMap", albedo);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", clip ? 0.08f : 0.18f);

            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mat.HasProperty("_OutlineWidth"))
                mat.SetFloat("_OutlineWidth", 0f);

            if (clip)
            {
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", 0.35f);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.SetFloat("_Cull", 0f);
                mat.doubleSidedGI = true;
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                mat.renderQueue = 2450;
            }
            else
            {
                mat.SetFloat("_AlphaClip", 0f);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = -1;
                ForceOpaqueLit(mat);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void ClampMaterialsToSubmeshes(Renderer renderer)
        {
            if (renderer == null) return;

            Mesh mesh = null;
            if (renderer is SkinnedMeshRenderer skinned)
                mesh = skinned.sharedMesh;
            else
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter != null) mesh = filter.sharedMesh;
            }

            if (mesh == null) return;

            var mats = renderer.sharedMaterials;
            if (mats == null || mats.Length <= mesh.subMeshCount) return;

            var next = new Material[mesh.subMeshCount];
            System.Array.Copy(mats, next, mesh.subMeshCount);
            renderer.sharedMaterials = next;
        }

        static Texture GetTex(Material mat, string prop)
        {
            if (mat == null || !mat.HasProperty(prop)) return null;
            return mat.GetTexture(prop);
        }

        static Texture2D GuessTexture(string materialName)
        {
            if (string.IsNullOrEmpty(materialName)) return null;
            var clean = materialName.Replace("MI_", "").Replace("(Instance)", "").Trim();
            var hits = AssetDatabase.FindAssets(clean + " t:Texture2D", new[]
            {
                "Assets/_ThirdParty/Nature/Textures",
                "Assets/_ThirdParty/Village/Textures",
                "Assets/_ThirdParty/Characters"
            });
            if (hits.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(hits[0]));
        }

        static bool IsFoliageCutout(string materialName, string textureName)
        {
            var blob = $"{materialName} {textureName}".ToLowerInvariant();
            return blob.Contains("leaf") || blob.Contains("leaves") || blob.Contains("pine")
                   || blob.Contains("flower") || blob.Contains("grass") || blob.Contains("bush")
                   || blob.Contains("foliage") || blob.Contains("petal") || blob.Contains("twig");
        }

        static bool HasOutlineHint(string shaderName, Material mat)
        {
            if (!string.IsNullOrEmpty(shaderName))
            {
                var n = shaderName.ToLowerInvariant();
                if (n.Contains("outline") || n.Contains("toon") || n.Contains("cel"))
                    return true;
            }

            return mat != null && mat.HasProperty("_OutlineWidth") && mat.GetFloat("_OutlineWidth") > 0.0001f;
        }

        static bool HasOutline(Material mat)
        {
            if (mat == null || mat.shader == null) return false;
            if (mat.shader.name == RimShaderName) return false;
            return HasOutlineHint(mat.shader.name, mat);
        }

        static bool HasAncestorNamed(Transform t, string name)
        {
            while (t != null)
            {
                if (t.name == name) return true;
                t = t.parent;
            }

            return false;
        }

        static string GetPath(Transform t)
        {
            var parts = new Stack<string>();
            while (t != null)
            {
                parts.Push(t.name);
                t = t.parent;
            }

            return string.Join("/", parts);
        }

        static string Sanitize(string name)
        {
            var chars = name.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_')
                    chars[i] = '_';
            }

            var s = new string(chars);
            if (s.Length > 80) s = s.Substring(0, 80);
            return string.IsNullOrEmpty(s) ? "Cap0_Lit" : s;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out var c);
            return c;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
