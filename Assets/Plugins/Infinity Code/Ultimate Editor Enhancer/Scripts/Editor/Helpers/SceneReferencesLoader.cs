/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class SceneReferencesLoader
    {
        static SceneReferencesLoader()
        {
            EditorApplication.delayCall += SceneReferences.UpdateInstances;
        }
    }
}