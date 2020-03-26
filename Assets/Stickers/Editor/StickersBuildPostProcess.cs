using UnityEditor;
using UnityEditor.Callbacks;

namespace Agens.StickersEditor
{
    public class StickersBuildPostprocessor
    {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            StickersExport.WriteToProject(pathToBuiltProject);
        }
    }
}
