using UnityEditor;
using UnityEngine;

namespace NullBorn.EditorTools
{
    /// <summary>
    /// Capturas rápidas para la tesis (Game View).
    /// Menú: NullBorn → Capturar screenshot tesis
    /// </summary>
    public static class ThesisScreenshot
    {
        [MenuItem("NullBorn/Capturar screenshot tesis")]
        public static void Capture()
        {
            string folder = "Captures";
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            string path = $"{folder}/NullBorn_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"Screenshot pedido: {path} (aparece tras un frame en Play Mode)");
            EditorUtility.DisplayDialog("NullBorn",
                "Screenshot solicitado.\nEntra en Play, espera 1 segundo y revisa la carpeta Captures/.",
                "OK");
        }
    }
}
