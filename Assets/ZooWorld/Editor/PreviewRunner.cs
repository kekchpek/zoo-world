using System.IO;
using UnityEditor;
using UnityEngine;
namespace ZooWorld.Editor
{
    [InitializeOnLoad]
    public static class PreviewRunner
    {
        private const string Active = "ZooWorld.Preview";
        private static double captureAt;
        private static int stage;
        static PreviewRunner() { EditorApplication.update += Update; }
        private static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists("preview.request"))
            {
                File.Delete("preview.request"); SessionState.SetBool(Active,true);
                EditorApplication.isPlaying=true; return;
            }
            if (!SessionState.GetBool(Active,false) || !EditorApplication.isPlaying) return;
            var world = Object.FindFirstObjectByType<ZooSimulation>();
            if (world == null || world.Arena == null) return;
            if (stage==0)
            {
                Random.InitState(2026);
                // Preview-only seed population, normal startup remains empty.
                for (int i=0;i<24;i++) world.Spawn(world.Species[i%4==0?1:0],world.Arena.RandomPoint(.7f));
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
                stage=1; captureAt=EditorApplication.timeSinceStartup+5; return;
            }
            if (EditorApplication.timeSinceStartup<captureAt) return;
            if (stage==1)
            {
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("../ZooWorld-preview.png"));
                stage=2; captureAt=EditorApplication.timeSinceStartup+2; return;
            }
            SessionState.SetBool(Active,false); stage=0; EditorApplication.isPlaying=false;
            Debug.Log("ZOO_PREVIEW_CAPTURED");
        }
    }
}
