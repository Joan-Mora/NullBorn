#if UNITY_EDITOR
using UnityEditor;

namespace NullBorn.Editor
{
    /// <summary>Batch-mode entry: Unity -batchmode -executeMethod NullBorn.Editor.RebuildLocomotionBatch.Run</summary>
    public static class RebuildLocomotionBatch
    {
        public static void Run()
        {
            BuildArtTestScene.RebuildLocomotionControllerMenu();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.Exit(0);
        }
    }
}
#endif
