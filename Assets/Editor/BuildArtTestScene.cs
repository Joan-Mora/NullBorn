#if UNITY_EDITOR
using System.IO;
using System.Linq;
using NullBorn.Game;
using NullBorn.Game.Map;
using NullBorn.Game.Net;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullBorn.Editor
{
    public static class BuildArtTestScene
    {
        const string ScenePath = "Assets/Scenes/ArtTest.unity";
        const string AnimFolder = "Assets/Art/Animation";
        const string ControllerPath = AnimFolder + "/PlayerLocomotion.controller";

        const string HeroFbx = "Assets/_ThirdParty/Characters/Base/FBX/Superhero_Male_FullBody.fbx";
        const string AnimLibFbx = "Assets/_ThirdParty/Characters/Animations/UAL1/UAL1_Standard.fbx";
        const string SwordFbx = "Assets/_ThirdParty/Armory/FBX/fbx/Weapons/SFA_Broadsword_002.fbx";

        [MenuItem("NullBorn/Build Art Test Scene")]
        public static void BuildFromMenu()
        {
            Build(showDialog: true);
        }

        [MenuItem("NullBorn/Rebuild Locomotion Controller")]
        public static void RebuildLocomotionControllerMenu()
        {
            EnsureFolder(AnimFolder);
            ConfigureHeroImport();
            ConfigureAnimLibraryImport();
            AssetDatabase.Refresh();
            CreateLocomotionController();
            AssetDatabase.SaveAssets();
            Debug.Log("[NullBorn] PlayerLocomotion.controller rebuilt (discrete Idle/Walk/Run states).");
        }

        public static void Build(bool showDialog)
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder(AnimFolder);

            ConfigureHeroImport();
            ConfigureAnimLibraryImport();
            ConfigureThirdPartyTextures();
            NullBornPerformanceBaseline.Apply(showDialog: false);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            ClearDefaultScene();

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.6f, 0.65f);
            RenderSettings.fog = false;

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.shadows = LightShadows.Hard;
            light.shadowStrength = 0.8f;
            light.shadowBias = 0.05f;
            light.shadowNormalBias = 0.4f;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            BuildGround();
            BuildVillagePlaza();
            ScatterNature();

            var player = BuildPlayer();
            var camera = SetupCamera(player.transform);

            var motor = player.GetComponent<SimpleThirdPersonMotor>();
            motor.SetCamera(camera.transform);
            motor.SetFacingTransform(player.transform);

            SetupMapCompanion(player);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();

            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.LookAt(player.transform.position + Vector3.up, Quaternion.Euler(22f, -30f, 0f), 14f);

            Debug.Log("[NullBorn] Art test scene saved to " + ScenePath);

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — Art Test",
                    "Escena regenerada.\n\n" +
                    "Controles:\n" +
                    "WASD + Shift sprint · Esc pausa (estilo mouse/teclado + sensibilidad)\n" +
                    "Con mouse: mouse=camara, A/D=girar personaje · Solo teclado: A/D=camara\n" +
                    "M mapa · F3 FPS\n\n" +
                    "Play ►",
                    "OK");
            }
        }

        static void ConfigureHeroImport()
        {
            var importer = AssetImporter.GetAtPath(HeroFbx) as ModelImporter;
            if (importer == null) return;

            var dirty = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                dirty = true;
            }

            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                dirty = true;
            }

            if (importer.importAnimation)
            {
                importer.importAnimation = false;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
                Debug.Log("[NullBorn] Configured hero Humanoid: " + HeroFbx);
            }
        }

        static void ConfigureAnimLibraryImport()
        {
            var importer = AssetImporter.GetAtPath(AnimLibFbx) as ModelImporter;
            if (importer == null) return;

            var dirty = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                dirty = true;
            }

            // Own Humanoid avatar — CopyFromOther fails (bone length mismatch vs Superhero).
            // Unity retargets Humanoid→Humanoid at runtime without sharing the same avatar.
            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                dirty = true;
            }

            if (importer.sourceAvatar != null)
            {
                importer.sourceAvatar = null;
                dirty = true;
            }

            // Broken clip list from a previous build had empty takeName / 0 frames.
            var current = importer.clipAnimations;
            var clipsBroken = current != null && current.Length > 0 &&
                              current.Any(c => string.IsNullOrEmpty(c.takeName) || c.lastFrame <= c.firstFrame);

            if (clipsBroken)
            {
                // Reset to defaults from FBX takes, then reimport so clips exist again.
                importer.clipAnimations = System.Array.Empty<ModelImporterClipAnimation>();
                importer.SaveAndReimport();
                importer = AssetImporter.GetAtPath(AnimLibFbx) as ModelImporter;
                if (importer == null) return;
                dirty = false; // just reimported base state
            }

            var defaults = importer.defaultClipAnimations;
            if (defaults != null && defaults.Length > 0)
            {
                EnableLocomotionLoops(defaults);
                if (!ClipListsEquivalent(importer.clipAnimations, defaults))
                {
                    importer.clipAnimations = defaults;
                    dirty = true;
                }
            }

            if (dirty)
            {
                importer.SaveAndReimport();
                Debug.Log("[NullBorn] Configured UAL1 Humanoid (own avatar) + loop clips.");
            }
        }

        static bool ClipListsEquivalent(ModelImporterClipAnimation[] a, ModelImporterClipAnimation[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i].name != b[i].name) return false;
                if (a[i].takeName != b[i].takeName) return false;
                if (a[i].loopTime != b[i].loopTime) return false;
                if (a[i].lastFrame != b[i].lastFrame) return false;
            }

            return true;
        }

        static void EnableLocomotionLoops(ModelImporterClipAnimation[] clips)
        {
            foreach (var clip in clips)
            {
                // FBX take names are like "Armature|Idle_Loop", not bare "Idle_Loop".
                if (!ShouldLoopClip(clip.name) && !ShouldLoopClip(clip.takeName))
                    continue;
                clip.loopTime = true;
                clip.loopPose = true;
            }
        }

        static bool ShouldLoopClip(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.EndsWith("Idle_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("Walk_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("Sprint_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("Jog_Fwd_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("|Idle_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("|Walk_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("|Sprint_Loop", System.StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("|Jog_Fwd_Loop", System.StringComparison.OrdinalIgnoreCase);
        }

        static void ClearDefaultScene()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            {
                if (go.name is "Directional Light" or "Global Volume")
                    Object.DestroyImmediate(go);
            }
        }

        static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            ground.isStatic = true;
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }

        static void BuildVillagePlaza()
        {
            var root = new GameObject("VillagePlaza");
            SpawnProp(root.transform, "Assets/_ThirdParty/Village/FBX/Floor_WoodLight.fbx", new Vector3(0f, 0.01f, 0f), Quaternion.identity, new Vector3(2f, 1f, 2f), castShadows: false);
            SpawnProp(root.transform, "Assets/_ThirdParty/Village/FBX/Wall_Plaster_Straight.fbx", new Vector3(-3.5f, 0f, 2f), Quaternion.Euler(0f, 90f, 0f), castShadows: false);
            SpawnProp(root.transform, "Assets/_ThirdParty/Village/FBX/Wall_Plaster_Straight.fbx", new Vector3(3.5f, 0f, 2f), Quaternion.Euler(0f, -90f, 0f), castShadows: false);
            SpawnProp(root.transform, "Assets/_ThirdParty/Village/FBX/Wall_Plaster_Door_Round.fbx", new Vector3(0f, 0f, 4f), Quaternion.Euler(0f, 180f, 0f), castShadows: false);
            SpawnProp(root.transform, "Assets/_ThirdParty/Village/FBX/Prop_Wagon.fbx", new Vector3(2.8f, 0f, -1.5f), Quaternion.Euler(0f, 35f, 0f), castShadows: false);
        }

        static void ScatterNature()
        {
            var root = new GameObject("Nature");
            // Fewer trees, no cast shadows on foliage for FPS.
            SpawnProp(root.transform, "Assets/_ThirdParty/Nature/FBX/CommonTree_1.fbx", new Vector3(-8f, 0f, -6f), Quaternion.Euler(0f, 20f, 0f), castShadows: false);
            SpawnProp(root.transform, "Assets/_ThirdParty/Nature/FBX/Pine_2.fbx", new Vector3(8f, 0f, -4f), Quaternion.Euler(0f, -15f, 0f), castShadows: false);
            SpawnProp(root.transform, "Assets/_ThirdParty/Nature/FBX/Bush_Common_Flowers.fbx", new Vector3(3f, 0f, 5f), Quaternion.identity, castShadows: false);
        }

        static GameObject BuildPlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0.05f, -3f);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0f;
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;

            player.AddComponent<SimpleThirdPersonMotor>();
            player.AddComponent<SimpleLocomotionAnim>();

            var visual = SpawnFbx(player.transform, HeroFbx, Vector3.zero, Quaternion.identity);
            if (visual == null)
                return player;

            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            StripColliders(visual);

            var animator = visual.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = visual.AddComponent<Animator>();

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = CreateLocomotionController();

            var rollClip = FindBestClip(AnimLibFbx, preferredExact: new[] { "Roll", "Armature|Roll" }, mustContain: "Roll", exclude: new[] { "Crouch" });
            if (rollClip != null)
            {
                // Match motor dash to sped-up Roll state (speed 1.15 in controller).
                player.GetComponent<SimpleThirdPersonMotor>().SetRollDuration(rollClip.length / 1.15f);
            }

            var heroAvatar = AssetDatabase.LoadAllAssetsAtPath(HeroFbx).OfType<Avatar>().FirstOrDefault();
            if (heroAvatar != null)
                animator.avatar = heroAvatar;
            else
                Debug.LogWarning("[NullBorn] Hero avatar not found on " + HeroFbx);

            FixHeroMaterials(visual);

            var animBridge = player.GetComponent<SimpleLocomotionAnim>();
            var so = new SerializedObject(animBridge);
            so.FindProperty("animator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();

            AttachSwordToHand(animator, visual.transform);

            return player;
        }

        static void AttachSwordToHand(Animator animator, Transform visualRoot)
        {
            Transform hand = null;
            if (animator != null && animator.isHuman)
                hand = animator.GetBoneTransform(HumanBodyBones.RightHand);

            if (hand == null)
                hand = FindBone(visualRoot, "Hand_R", "RightHand", "hand_r", "mixamorig:RightHand", "HandR");

            if (hand == null)
            {
                Debug.LogWarning("[NullBorn] Right hand bone not found — sword parented to Visual.");
                hand = visualRoot;
            }

            var sword = SpawnFbx(hand, SwordFbx, Vector3.zero, Quaternion.identity, new Vector3(0.55f, 0.55f, 0.55f));
            if (sword == null) return;

            sword.name = "Weapon_Sword";
            StripColliders(sword);
            // Grip + 180° yaw tweak (blade/handle orientation on Superhero hand).
            sword.transform.localPosition = new Vector3(0.02f, 0.05f, 0.02f);
            sword.transform.localRotation = Quaternion.Euler(0f, 270f, -90f);
        }

        static Transform FindBone(Transform root, params string[] names)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (var n in names)
                {
                    if (string.Equals(t.name, n, System.StringComparison.OrdinalIgnoreCase))
                        return t;
                }
            }

            // Partial match fallback.
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var lower = t.name.ToLowerInvariant();
                if (lower.Contains("righthand") || lower.Contains("hand_r") || lower.EndsWith("hand.r"))
                    return t;
            }

            return null;
        }

        static RuntimeAnimatorController CreateLocomotionController()
        {
            var idleClip = FindBestClip(AnimLibFbx, preferredExact: new[] { "Idle_Loop", "Armature|Idle_Loop", "Idle", "Idle_A", "Idle01" }, mustContain: "Idle_Loop", exclude: new[] { "Crouch", "Sit", "Aim", "Combat", "Injured", "Death", "Talk", "Torch", "Sword", "Spell", "Swim", "Pistol" });
            var walkClip = FindBestClip(AnimLibFbx, preferredExact: new[] { "Walk_Loop", "Armature|Walk_Loop", "Walk", "Walk_A", "WalkForward" }, mustContain: "Walk_Loop", exclude: new[] { "Crouch", "Back", "Strafe", "Formal" });
            var runClip = FindBestClip(AnimLibFbx, preferredExact: new[] { "Sprint_Loop", "Armature|Sprint_Loop" }, mustContain: "Sprint_Loop", exclude: new[] { "Crouch", "Back", "Strafe", "Jog" })
                ?? FindBestClip(AnimLibFbx, preferredExact: new[] { "Jog_Fwd_Loop", "Armature|Jog_Fwd_Loop" }, mustContain: "Jog_Fwd", exclude: new[] { "Crouch", "Back", "Strafe" });
            var rollClip = FindBestClip(AnimLibFbx, preferredExact: new[] { "Roll", "Armature|Roll" }, mustContain: "Roll", exclude: new[] { "Crouch" });

            if (idleClip == null) Debug.LogWarning("[NullBorn] No Idle clip found in UAL1.");
            if (walkClip == null) Debug.LogWarning("[NullBorn] No Walk clip found in UAL1.");
            if (runClip == null) Debug.LogWarning("[NullBorn] No Sprint clip found in UAL1.");
            else Debug.Log("[NullBorn] Run clip: " + runClip.name);
            if (rollClip == null) Debug.LogWarning("[NullBorn] No Roll clip found in UAL1.");
            else Debug.Log("[NullBorn] Roll clip: " + rollClip.name + " (" + rollClip.length + "s)");

            ForceClipLoop(idleClip);
            ForceClipLoop(walkClip);
            ForceClipLoop(runClip);

            if (File.Exists(Path.GetFullPath(ControllerPath)))
                AssetDatabase.DeleteAsset(ControllerPath);

            // Discrete states (NOT a BlendTree): only one humanoid clip evaluates at a time.
            // BlendTrees Idle+Walk together were the FPS hitch on every start/stop.
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var root = controller.layers[0].stateMachine;

            AnimatorState idle = null, walk = null, run = null, backpedal = null;

            if (idleClip != null)
            {
                idle = root.AddState("Idle");
                idle.motion = idleClip;
                idle.writeDefaultValues = true;
                root.defaultState = idle;
            }

            if (walkClip != null)
            {
                walk = root.AddState("Walk");
                walk.motion = walkClip;
                walk.writeDefaultValues = true;

                backpedal = root.AddState("Backpedal");
                backpedal.motion = walkClip;
                backpedal.writeDefaultValues = true;
                backpedal.speed = -1f;
            }

            if (runClip != null)
            {
                run = root.AddState("Run");
                run.motion = runClip;
                run.writeDefaultValues = true;
                run.speed = 0.88f;
            }
            else if (walk != null)
            {
                run = walk;
            }

            if (root.defaultState == null && walk != null)
                root.defaultState = walk;

            if (rollClip != null)
            {
                var roll = root.AddState("Roll");
                roll.motion = rollClip;
                roll.writeDefaultValues = true;
                roll.speed = 1.15f;

                // Return to Idle after roll; script CrossFades into locomotion.
                if (idle != null)
                {
                    var toIdle = roll.AddTransition(idle);
                    toIdle.hasExitTime = true;
                    toIdle.exitTime = 0.88f;
                    toIdle.hasFixedDuration = true;
                    toIdle.duration = 0.12f;
                    toIdle.interruptionSource = TransitionInterruptionSource.None;
                }
            }

            AssetDatabase.SaveAssets();
            return controller;
        }

        static void ForceClipLoop(AnimationClip clip)
        {
            if (clip == null) return;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime) return;
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            Debug.Log("[NullBorn] Forced loop on clip: " + clip.name);
        }

        static void ConfigureThirdPartyTextures()
        {
            ConfigureTextureFolder("Assets/_ThirdParty/Nature/Textures");
            ConfigureTextureFolder("Assets/_ThirdParty/Village/Textures");
            ConfigureTextureFolder("Assets/_ThirdParty/Characters/Base/Textures");
            ConfigureTextureFolder("Assets/_ThirdParty/Armory/Textures");
        }

        static void ConfigureTextureFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                var dirty = false;
                if (importer.filterMode != FilterMode.Trilinear)
                {
                    importer.filterMode = FilterMode.Trilinear;
                    dirty = true;
                }

                if (importer.anisoLevel < 8)
                {
                    importer.anisoLevel = 8;
                    dirty = true;
                }

                if (importer.maxTextureSize < 2048)
                {
                    importer.maxTextureSize = 2048;
                    dirty = true;
                }

                var standalone = importer.GetPlatformTextureSettings("Standalone");
                if (!standalone.overridden || standalone.maxTextureSize < 2048 || standalone.compressionQuality < 90)
                {
                    standalone.overridden = true;
                    standalone.maxTextureSize = 2048;
                    standalone.textureCompression = TextureImporterCompression.CompressedHQ;
                    standalone.compressionQuality = 100;
                    importer.SetPlatformTextureSettings(standalone);
                    dirty = true;
                }

                if (dirty)
                    importer.SaveAndReimport();
            }
        }

        static void FixHeroMaterials(GameObject visual)
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;

            var skinBase = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Ligh.png");
            var skinNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_ThirdParty/Characters/Base/Textures/T_Superhero_Male_Normal.png");

            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                for (var i = 0; i < mats.Length; i++)
                {
                    var mat = new Material(urpLit) { name = "Hero_URP" };
                    if (skinBase != null)
                        mat.SetTexture("_BaseMap", skinBase);
                    if (skinNormal != null)
                    {
                        mat.SetTexture("_BumpMap", skinNormal);
                        mat.EnableKeyword("_NORMALMAP");
                    }

                    mats[i] = mat;
                }

                renderer.sharedMaterials = mats;
            }
        }

        static void UpgradeMaterialsToUrp(GameObject root)
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                for (var i = 0; i < mats.Length; i++)
                {
                    var source = mats[i];
                    if (source == null || source.shader.name.Contains("Universal Render Pipeline"))
                        continue;

                    var mat = new Material(urpLit) { name = source.name + "_URP" };

                    if (source.HasProperty("_MainTex"))
                        mat.SetTexture("_BaseMap", source.GetTexture("_MainTex"));
                    if (source.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == null)
                        mat.SetTexture("_BaseMap", source.GetTexture("_BaseMap"));
                    if (source.HasProperty("_Color"))
                        mat.SetColor("_BaseColor", source.GetColor("_Color"));
                    if (source.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", source.GetColor("_BaseColor"));
                    if (source.HasProperty("_BumpMap"))
                    {
                        mat.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
                        mat.EnableKeyword("_NORMALMAP");
                    }

                    var tex = mat.GetTexture("_BaseMap");
                    if (tex != null && tex.name.IndexOf("Leaf", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        mat.SetFloat("_AlphaClip", 1f);
                        mat.SetFloat("_Cutoff", 0.4f);
                        mat.EnableKeyword("_ALPHATEST_ON");
                    }

                    mats[i] = mat;
                }

                renderer.sharedMaterials = mats;
            }
        }

        static AnimationClip FindBestClip(string fbxPath, string[] preferredExact, string mustContain, string[] exclude)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__"))
                .ToList();

            static string StripPrefix(string name)
            {
                var idx = name.LastIndexOf('|');
                return idx >= 0 ? name[(idx + 1)..] : name;
            }

            foreach (var exact in preferredExact)
            {
                var hit = clips.FirstOrDefault(c =>
                    string.Equals(c.name, exact, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(StripPrefix(c.name), StripPrefix(exact), System.StringComparison.OrdinalIgnoreCase) ||
                    c.name.EndsWith("|" + exact, System.StringComparison.OrdinalIgnoreCase));
                if (hit != null) return hit;
            }

            bool IsExcluded(string name) =>
                exclude.Any(ex => name.IndexOf(ex, System.StringComparison.OrdinalIgnoreCase) >= 0);

            return clips
                .Where(c => c.name.IndexOf(mustContain, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(c => !IsExcluded(c.name))
                .OrderBy(c => c.name.Length)
                .ThenBy(c => c.name)
                .FirstOrDefault();
        }

        static Camera SetupCamera(Transform target)
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            var cam = camGo.GetComponent<Camera>();
            cam.farClipPlane = 200f;
            cam.allowHDR = false;
            cam.allowMSAA = false;

            var follow = camGo.GetComponent<SimpleFollowCamera>() ?? camGo.AddComponent<SimpleFollowCamera>();
            follow.ApplyOverTheShoulderDefaults();
            follow.SetTarget(target);
            follow.SetFacingTransform(target);

            if (camGo.GetComponent<SimpleFpsOverlay>() == null)
                camGo.AddComponent<SimpleFpsOverlay>();
            if (camGo.GetComponent<RuntimePerformanceTweaks>() == null)
                camGo.AddComponent<RuntimePerformanceTweaks>();

            // Seed locked OTS framing (script takes over in LateUpdate).
            var yaw = target.eulerAngles.y;
            var rot = Quaternion.Euler(10f, yaw, 0f);
            camGo.transform.position = target.position + Vector3.up * 1.55f + rot * new Vector3(0.48f, 0f, -2.75f);
            camGo.transform.rotation = Quaternion.LookRotation(
                (target.position + Vector3.up * 1.45f + rot * Vector3.forward * 2.2f) - camGo.transform.position,
                Vector3.up);
            return cam;
        }

        static void SetupMapCompanion(GameObject player)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            var net = new GameObject("MapSync");
            var config = net.AddComponent<NullBornApiConfig>();
            config.Configure(NullBornApiConfig.DefaultBaseUrl, NullBornApiConfig.DefaultZoneId, NullBornApiConfig.DefaultPlayerId);
            config.SetRates(2f, 1f);
            // Off by default — avoids Play-mode hitches when API isn't running.
            // Enable on MapSync when companion API is up (dotnet run).
            config.SetAutoConnect(false);
            var sync = net.AddComponent<GameSyncClient>();
            var pose = net.AddComponent<PlayerPosePublisher>();
            var visual = player.transform.Find("Visual");
            pose.SetPoseSource(visual != null ? visual : player.transform);
            net.AddComponent<WorldMarkerView>();

            EnsureEventSystem();
            BuildMinimapUi(player.transform, sync);
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        static void BuildMinimapUi(Transform player, GameSyncClient sync)
        {
            var poseSource = player.Find("Visual") != null ? player.Find("Visual") : player;

            var circleMask = MapUiSprites.Circle(256, Color.white);
            var circleFrame = MapUiSprites.Circle(256, new Color(0.07f, 0.1f, 0.08f, 0.92f), new Color(0.76f, 0.47f, 0.23f, 0.85f), 0.06f);
            var pinSprite = MapUiSprites.Diamond(64, Color.white);
            var playerSprite = MapUiSprites.Triangle(64, new Color(0.49f, 0.81f, 0.51f, 1f));
            var needleSprite = MapUiSprites.Triangle(96, new Color(0.94f, 0.55f, 0.22f, 1f));

            var canvasGo = new GameObject("MapHudCanvas", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            // --- Corner minimap (circular) ---
            var miniRoot = CreateUiPanel(canvasGo.transform, "MinimapRoot", new Vector2(-28f, -28f), new Vector2(200f, 200f), TextAnchor.UpperRight);
            var miniFrame = CreateUiPanel(miniRoot, "Frame", Vector2.zero, new Vector2(200f, 200f), TextAnchor.MiddleCenter);
            var frameImg = miniFrame.gameObject.AddComponent<Image>();
            frameImg.sprite = circleFrame;
            frameImg.type = Image.Type.Simple;
            frameImg.preserveAspect = true;

            var miniMaskRt = CreateUiPanel(miniRoot, "Mask", Vector2.zero, new Vector2(176f, 176f), TextAnchor.MiddleCenter);
            var maskImg = miniMaskRt.gameObject.AddComponent<Image>();
            maskImg.sprite = circleMask;
            maskImg.color = Color.white;
            var mask = miniMaskRt.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var mapBg = CreateUiPanel(miniMaskRt, "MapBg", Vector2.zero, new Vector2(176f, 176f), TextAnchor.MiddleCenter);
            var mapImg = mapBg.gameObject.AddComponent<RawImage>();
            mapImg.texture = ArtTestMapTexture.Build(256);
            mapImg.color = Color.white;

            var markersRoot = CreateUiPanel(miniMaskRt, "Markers", Vector2.zero, new Vector2(176f, 176f), TextAnchor.MiddleCenter);
            var playerIcon = CreateUiPanel(miniMaskRt, "PlayerIcon", Vector2.zero, new Vector2(18f, 22f), TextAnchor.MiddleCenter);
            var playerImg = playerIcon.gameObject.AddComponent<Image>();
            playerImg.sprite = playerSprite;
            playerImg.raycastTarget = false;

            var hint = CreateUiPanel(miniRoot, "Hint", new Vector2(0f, 14f), new Vector2(180f, 22f), TextAnchor.LowerCenter);
            var hintText = hint.gameObject.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (hintText.font == null)
                hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            hintText.text = "M · mapa";
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.fontSize = 14;
            hintText.color = new Color(0.85f, 0.9f, 0.82f, 0.75f);

            var minimap = canvasGo.AddComponent<MinimapController>();
            minimap.Wire(sync, mapImg, playerIcon, markersRoot, poseSource);
            minimap.SetPinSprite(pinSprite);

            // --- Full map (M) — reserved header/footer so text is never clipped by the map ---
            var fullRoot = CreateUiPanel(canvasGo.transform, "FullMapRoot", Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            StretchFull(fullRoot);
            var fullGroup = fullRoot.gameObject.AddComponent<CanvasGroup>();
            var dim = fullRoot.gameObject.AddComponent<Image>();
            dim.color = new Color(0.02f, 0.04f, 0.03f, 0.72f);
            dim.raycastTarget = true;

            var fullPanel = CreateUiPanel(fullRoot, "FullPanel", Vector2.zero, new Vector2(700f, 780f), TextAnchor.MiddleCenter);
            var fullPanelImg = fullPanel.gameObject.AddComponent<Image>();
            fullPanelImg.color = new Color(0.08f, 0.11f, 0.09f, 0.96f);

            var fullTitle = CreateUiPanel(fullPanel, "Title", new Vector2(0f, -22f), new Vector2(520f, 40f), TextAnchor.UpperCenter);
            var titleText = fullTitle.gameObject.AddComponent<Text>();
            titleText.font = hintText.font;
            titleText.text = "Mapa táctico · ArtTest";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 22;
            titleText.color = new Color(0.9f, 0.93f, 0.86f);
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;

            // Map sits in the middle band (panel 780 → ~80 header + ~80 footer).
            var fullMapBg = CreateUiPanel(fullPanel, "FullMapBg", new Vector2(0f, 0f), new Vector2(560f, 560f), TextAnchor.MiddleCenter);
            var fullMapImg = fullMapBg.gameObject.AddComponent<RawImage>();
            fullMapImg.texture = ArtTestMapTexture.Build(256);
            fullMapImg.color = Color.white;
            var fullMarkers = CreateUiPanel(fullPanel, "FullMarkers", Vector2.zero, new Vector2(560f, 560f), TextAnchor.MiddleCenter);
            var fullPlayer = CreateUiPanel(fullPanel, "FullPlayer", Vector2.zero, new Vector2(28f, 34f), TextAnchor.MiddleCenter);
            var fullPlayerImg = fullPlayer.gameObject.AddComponent<Image>();
            fullPlayerImg.sprite = playerSprite;

            var fullMinimap = fullPanel.gameObject.AddComponent<MinimapController>();
            fullMinimap.Wire(sync, fullMapImg, fullPlayer, fullMarkers, poseSource);
            fullMinimap.SetPinSprite(pinSprite);

            var fullHint = CreateUiPanel(fullPanel, "FullHint", new Vector2(0f, 22f), new Vector2(560f, 36f), TextAnchor.LowerCenter);
            var fullHintText = fullHint.gameObject.AddComponent<Text>();
            fullHintText.font = hintText.font;
            fullHintText.text = "M / Esc cerrar · pins desde el companion móvil";
            fullHintText.alignment = TextAnchor.MiddleCenter;
            fullHintText.fontSize = 15;
            fullHintText.color = new Color(0.75f, 0.82f, 0.72f, 0.85f);
            fullHintText.horizontalOverflow = HorizontalWrapMode.Overflow;
            fullHintText.verticalOverflow = VerticalWrapMode.Overflow;

            // Draw order: map first, then chrome on top (sibling index).
            fullTitle.SetAsLastSibling();
            fullHint.SetAsLastSibling();

            var hud = canvasGo.AddComponent<MapHudController>();
            hud.Wire(miniRoot, fullRoot, fullGroup);

            // --- Compass (circular + needle + distance) ---
            var compassRoot = CreateUiPanel(canvasGo.transform, "Compass", new Vector2(0f, -28f), new Vector2(120f, 140f), TextAnchor.UpperCenter);
            var cg = compassRoot.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            var compassDisc = CreateUiPanel(compassRoot, "Disc", new Vector2(0f, -10f), new Vector2(110f, 110f), TextAnchor.UpperCenter);
            var discImg = compassDisc.gameObject.AddComponent<Image>();
            discImg.sprite = MapUiSprites.Circle(256, new Color(0.06f, 0.09f, 0.07f, 0.88f), new Color(0.76f, 0.47f, 0.23f, 0.7f), 0.07f);

            var north = CreateUiPanel(compassDisc, "N", new Vector2(0f, -8f), new Vector2(24f, 20f), TextAnchor.UpperCenter);
            var nText = north.gameObject.AddComponent<Text>();
            nText.font = hintText.font;
            nText.text = "N";
            nText.alignment = TextAnchor.MiddleCenter;
            nText.fontSize = 14;
            nText.fontStyle = FontStyle.Bold;
            nText.color = new Color(0.94f, 0.55f, 0.22f);

            var needle = CreateUiPanel(compassDisc, "Needle", Vector2.zero, new Vector2(34f, 48f), TextAnchor.MiddleCenter);
            var needleImg = needle.gameObject.AddComponent<Image>();
            needleImg.sprite = needleSprite;
            needleImg.raycastTarget = false;

            var distRt = CreateUiPanel(compassRoot, "Distance", new Vector2(0f, 8f), new Vector2(200f, 24f), TextAnchor.LowerCenter);
            var distText = distRt.gameObject.AddComponent<Text>();
            distText.font = hintText.font;
            distText.alignment = TextAnchor.MiddleCenter;
            distText.fontSize = 14;
            distText.color = new Color(0.9f, 0.93f, 0.86f);
            distText.text = "";

            var compass = canvasGo.AddComponent<NavigationCompass>();
            compass.SetPlayer(poseSource);
            compass.SetNeedle(needle);
            compass.SetDistanceLabel(distText);
            compass.SetCanvasGroup(cg);
            compass.SetSync(sync);

            BuildPauseMenu(canvasGo.transform, hintText.font);
        }

        static void BuildPauseMenu(Transform canvasRoot, Font font)
        {
            // Dedicated canvas (high sort order) so clicks always hit pause UI.
            var pauseCanvasGo = new GameObject("PauseCanvas");
            var pauseCanvas = pauseCanvasGo.AddComponent<Canvas>();
            pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            pauseCanvas.sortingOrder = 200;
            var scaler = pauseCanvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            pauseCanvasGo.AddComponent<GraphicRaycaster>();

            var pauseRoot = CreateUiPanel(pauseCanvasGo.transform, "PauseRoot", Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            StretchFull(pauseRoot);
            var dim = pauseRoot.gameObject.AddComponent<Image>();
            dim.color = new Color(0.02f, 0.04f, 0.03f, 0.82f);
            dim.raycastTarget = true;

            var card = CreateUiPanel(pauseRoot, "PauseCard", Vector2.zero, new Vector2(520f, 460f), TextAnchor.MiddleCenter);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = new Color(0.09f, 0.12f, 0.1f, 0.98f);
            cardImg.raycastTarget = true;

            float y = -32f;
            var title = CreateUiPanel(card, "Title", new Vector2(0f, y), new Vector2(460f, 40f), TextAnchor.UpperCenter);
            var titleText = title.gameObject.AddComponent<Text>();
            titleText.font = font;
            titleText.text = "Pausa";
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.92f, 0.95f, 0.88f);
            titleText.raycastTarget = false;

            y -= 50f;
            var schemeTitle = CreateUiPanel(card, "SchemeTitle", new Vector2(0f, y), new Vector2(460f, 24f), TextAnchor.UpperCenter);
            var schemeTitleText = schemeTitle.gameObject.AddComponent<Text>();
            schemeTitleText.font = font;
            schemeTitleText.text = "Estilo de control";
            schemeTitleText.fontSize = 15;
            schemeTitleText.alignment = TextAnchor.MiddleLeft;
            schemeTitleText.color = new Color(0.8f, 0.86f, 0.78f);
            schemeTitleText.raycastTarget = false;

            y -= 40f;
            var schemeRow = CreateUiPanel(card, "SchemeRow", new Vector2(0f, y), new Vector2(460f, 44f), TextAnchor.UpperCenter);
            var mouseBtn = CreatePauseButton(schemeRow, "WithMouse", new Vector2(-115f, 0f), new Vector2(210f, 40f), "Con mouse", font, 16);
            var keyBtn = CreatePauseButton(schemeRow, "KeyboardOnly", new Vector2(115f, 0f), new Vector2(210f, 40f), "Solo teclado", font, 16);

            y -= 36f;
            var schemeInfo = CreateUiPanel(card, "SchemeInfo", new Vector2(0f, y), new Vector2(460f, 28f), TextAnchor.UpperCenter);
            var schemeInfoText = schemeInfo.gameObject.AddComponent<Text>();
            schemeInfoText.font = font;
            schemeInfoText.fontSize = 13;
            schemeInfoText.alignment = TextAnchor.MiddleCenter;
            schemeInfoText.color = new Color(0.94f, 0.55f, 0.22f);
            schemeInfoText.raycastTarget = false;
            schemeInfoText.text = "Estilo: Mouse (A/D giran el personaje)";

            y -= 44f;
            var sensLabel = CreateUiPanel(card, "SensLabel", new Vector2(0f, y), new Vector2(460f, 24f), TextAnchor.UpperCenter);
            var sensLabelText = sensLabel.gameObject.AddComponent<Text>();
            sensLabelText.font = font;
            sensLabelText.text = "Sensibilidad del mouse";
            sensLabelText.fontSize = 15;
            sensLabelText.alignment = TextAnchor.MiddleLeft;
            sensLabelText.color = new Color(0.8f, 0.86f, 0.78f);
            sensLabelText.raycastTarget = false;

            var sensValueRt = CreateUiPanel(card, "SensValue", new Vector2(0f, y), new Vector2(460f, 24f), TextAnchor.UpperCenter);
            var sensValue = sensValueRt.gameObject.AddComponent<Text>();
            sensValue.font = font;
            sensValue.fontSize = 15;
            sensValue.alignment = TextAnchor.MiddleRight;
            sensValue.color = new Color(0.94f, 0.55f, 0.22f);
            sensValue.text = "1.00x";
            sensValue.raycastTarget = false;

            y -= 40f;
            var sliderRow = CreateUiPanel(card, "SensRow", new Vector2(0f, y), new Vector2(460f, 40f), TextAnchor.UpperCenter);
            var minusBtn = CreatePauseButton(sliderRow, "Minus", new Vector2(-200f, 0f), new Vector2(40f, 36f), "-", font, 22);
            var plusBtn = CreatePauseButton(sliderRow, "Plus", new Vector2(200f, 0f), new Vector2(40f, 36f), "+", font, 22);
            var slider = CreateSensitivitySlider(sliderRow, Vector2.zero, new Vector2(300f, 28f));

            y -= 48f;
            var hint = CreateUiPanel(card, "Hint", new Vector2(0f, y), new Vector2(460f, 44f), TextAnchor.UpperCenter);
            var hintText = hint.gameObject.AddComponent<Text>();
            hintText.font = font;
            hintText.text = "Con mouse: A/D giran cuerpo · W hacia donde miras\nSolo teclado: A/D = girar camara";
            hintText.fontSize = 13;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.color = new Color(0.7f, 0.76f, 0.68f);
            hintText.raycastTarget = false;

            y -= 64f;
            var resumeRt = CreateUiPanel(card, "ResumeBtn", new Vector2(0f, y), new Vector2(240f, 48f), TextAnchor.UpperCenter);
            var resumeImg = resumeRt.gameObject.AddComponent<Image>();
            resumeImg.color = new Color(0.76f, 0.47f, 0.23f, 1f);
            var resumeBtn = resumeRt.gameObject.AddComponent<Button>();
            resumeBtn.targetGraphic = resumeImg;
            var resumeLabel = CreateUiPanel(resumeRt, "Label", Vector2.zero, new Vector2(220f, 44f), TextAnchor.MiddleCenter);
            var resumeText = resumeLabel.gameObject.AddComponent<Text>();
            resumeText.font = font;
            resumeText.text = "Continuar";
            resumeText.fontSize = 18;
            resumeText.fontStyle = FontStyle.Bold;
            resumeText.alignment = TextAnchor.MiddleCenter;
            resumeText.color = new Color(0.12f, 0.08f, 0.04f);
            resumeText.raycastTarget = false;

            var pause = pauseCanvasGo.AddComponent<PauseMenuController>();
            pause.Wire(
                pauseRoot.gameObject,
                slider,
                sensValue,
                schemeInfoText,
                mouseBtn.targetGraphic as Image,
                keyBtn.targetGraphic as Image,
                mouseBtn,
                keyBtn,
                minusBtn,
                plusBtn,
                resumeBtn);
            pauseRoot.gameObject.SetActive(false);
        }

        static Button CreatePauseButton(RectTransform parent, string name, Vector2 pos, Vector2 size, string label, Font font, int fontSize = 22)
        {
            var rt = CreateUiPanel(parent, name, pos, size, TextAnchor.MiddleCenter);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.18f, 1f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var textRt = CreateUiPanel(rt, "Label", Vector2.zero, size, TextAnchor.MiddleCenter);
            var text = textRt.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = label;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.92f, 0.95f, 0.88f);
            text.raycastTarget = false;
            return btn;
        }

        static Slider CreateSensitivitySlider(RectTransform parent, Vector2 pos, Vector2 size)
        {
            var root = CreateUiPanel(parent, "SensSlider", pos, size, TextAnchor.MiddleCenter);
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.07f, 0.06f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
            fillArea.SetParent(root, false);
            StretchFull(fillArea);
            fillArea.offsetMin = new Vector2(8f, 8f);
            fillArea.offsetMax = new Vector2(-8f, -8f);

            var fill = new GameObject("Fill", typeof(RectTransform)).GetComponent<RectTransform>();
            fill.SetParent(fillArea, false);
            StretchFull(fill);
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = new Color(0.76f, 0.47f, 0.23f, 0.95f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.SetParent(root, false);
            StretchFull(handleArea);
            handleArea.offsetMin = new Vector2(10f, 0f);
            handleArea.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform)).GetComponent<RectTransform>();
            handle.SetParent(handleArea, false);
            handle.sizeDelta = new Vector2(24f, 24f);
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.color = new Color(0.94f, 0.78f, 0.55f, 1f);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.targetGraphic = handleImg;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0.1f;
            slider.maxValue = 2f;
            slider.wholeNumbers = false;
            slider.value = GameSettings.MouseSensitivity;
            return slider;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static RectTransform CreateUiPanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(
                anchor is TextAnchor.UpperRight or TextAnchor.MiddleRight or TextAnchor.LowerRight ? 1f :
                anchor is TextAnchor.UpperCenter or TextAnchor.MiddleCenter or TextAnchor.LowerCenter ? 0.5f : 0f,
                anchor is TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight ? 1f :
                anchor is TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight ? 0.5f : 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            if (anchor == TextAnchor.UpperRight) rt.pivot = new Vector2(1f, 1f);
            if (anchor == TextAnchor.UpperCenter) rt.pivot = new Vector2(0.5f, 1f);
            if (anchor == TextAnchor.LowerCenter) rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        static GameObject SpawnProp(Transform parent, string assetPath, Vector3 localPos, Quaternion localRot, Vector3? localScale = null, bool castShadows = true)
        {
            var go = SpawnFbx(parent, assetPath, localPos, localRot, localScale);
            if (go == null) return null;

            StripColliders(go);
            UpgradeMaterialsToUrp(go);
            go.isStatic = true;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);

            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                // Foliage: no cast (overdraw + shadow maps) but still receive for grounding.
                r.shadowCastingMode = castShadows
                    ? UnityEngine.Rendering.ShadowCastingMode.On
                    : UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = true;
            }

            return go;
        }

        static GameObject SpawnFbx(Transform parent, string assetPath, Vector3 localPos, Quaternion localRot, Vector3? localScale = null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning("[NullBorn] Missing FBX: " + assetPath);
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            if (localScale.HasValue)
                go.transform.localScale = localScale.Value;
            return go;
        }

        static void StripColliders(GameObject root)
        {
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent!, name!);
        }
    }
}
#endif
