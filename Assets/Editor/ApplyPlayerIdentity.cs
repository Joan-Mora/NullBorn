#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NullBorn.Editor
{
    public static class ApplyPlayerIdentity
    {
        const string IconPath = "Assets/Art/UI/AppIcon.png";

        [MenuItem("NullBorn/Apply Product Name And Icon")]
        public static void ApplyFromMenu()
        {
            Apply();
            EditorUtility.DisplayDialog(
                "NullBorn",
                "Nombre: NullBorn.\nIcono asignado al .exe y a la ventana.\nVuelve a generar el Windows Player.",
                "OK");
        }

        public static void Apply()
        {
            PlayerSettings.productName = "NullBorn";
            PlayerSettings.companyName = "NullBorn";

            var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.isReadable = true;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Standalone, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Application);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[NullBorn] Product name + icon applied.");
        }
    }
}
#endif
