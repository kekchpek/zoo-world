using System.IO;
using UnityEditor;
namespace ZooWorld.Editor
{
    // Opt-in local automation. Request files are ignored by Git and never enter a player build.
    [InitializeOnLoad]
    public static class LocalVerificationRunner
    {
        static LocalVerificationRunner() { EditorApplication.update += Update; }
        private static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists("verify.request"))
            {
                File.Delete("verify.request");
                Verification.Run();
                return;
            }
            if (File.Exists("build.request"))
            {
                File.Delete("build.request");
                ProjectBuilder.Build();
            }
        }
    }
}
