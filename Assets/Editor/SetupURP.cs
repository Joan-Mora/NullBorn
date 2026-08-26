using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullBorn.EditorTools
{
    /// <summary>
    /// Migra Built-in (obsoleto en Unity 6) → URP.
    /// Usa reflexión para poder compilar aunque el paquete aún se esté instalando.
    /// Menú: NullBorn → Configurar URP
    /// </summary>
    public static class SetupURP
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string RendererPath = "Assets/Settings/URP_Renderer.asset";
        private const string PipelinePath = "Assets/Settings/URP_Pipeline.asset";
        private const string PackageId = "com.unity.render-pipelines.universal";

        private static AddRequest addRequest;
        private static bool showDialog;

        [MenuItem("NullBorn/Configurar URP")]
        public static void SetupFromMenu()
        {
            showDialog = true;
            BeginSetup();
        }

        public static void SetupBatch()
        {
            showDialog = false;
            BeginSetup();
        }

        private static void BeginSetup()
        {
            if (FindUrpType("UniversalRenderPipelineAsset") != null)
            {
                ConfigureUrpAssets();
                return;
            }

            Debug.Log("Instalando paquete URP...");
            addRequest = Client.Add(PackageId);
            EditorApplication.update += WaitForPackage;
        }

        private static void WaitForPackage()
        {
            if (addRequest == null || !addRequest.IsCompleted) return;
            EditorApplication.update -= WaitForPackage;

            if (addRequest.Status == StatusCode.Failure)
            {
                Debug.LogError("No se pudo instalar URP: " + addRequest.Error.message);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            Debug.Log("URP instalado. Esperando recompilación...");
            EditorApplication.delayCall += ConfigureUrpAssets;
        }

        private static void ConfigureUrpAssets()
        {
            Type rendererType = FindUrpType("UniversalRendererData");
            Type pipelineType = FindUrpType("UniversalRenderPipelineAsset");

            if (rendererType == null || pipelineType == null)
            {
                Debug.LogError("URP aún no está disponible. Abre el proyecto una vez y vuelve a ejecutar NullBorn → Configurar URP.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath(RendererPath, rendererType) as ScriptableObject;
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererType);
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            ScriptableObject pipelineAsset = AssetDatabase.LoadAssetAtPath(PipelinePath, pipelineType) as ScriptableObject;
            if (pipelineAsset == null)
            {
                MethodInfo createMethod = pipelineType.GetMethod(
                    "Create",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { rendererType },
                    null);

                if (createMethod != null)
                {
                    pipelineAsset = createMethod.Invoke(null, new object[] { rendererData }) as ScriptableObject;
                }
                else
                {
                    pipelineAsset = ScriptableObject.CreateInstance(pipelineType);
                    // Asigna el renderer por campo serializado si Create no existe.
                    TryAssignRenderer(pipelineAsset, rendererData);
                }

                AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset as RenderPipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset as RenderPipelineAsset;

            EditorUtility.SetDirty(pipelineAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("URP configurado correctamente.");

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "NullBorn — URP",
                    "Universal Render Pipeline listo.\n\nSi Unity Hub aún muestra el aviso, cierra el Hub/proyecto y vuelve a abrirlo.",
                    "OK");
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        private static void TryAssignRenderer(ScriptableObject pipelineAsset, ScriptableObject rendererData)
        {
            SerializedObject so = new SerializedObject(pipelineAsset);
            SerializedProperty list = so.FindProperty("m_RendererDataList");
            if (list != null)
            {
                list.arraySize = 1;
                list.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Type FindUrpType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("UnityEngine.Rendering.Universal." + typeName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
