/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class CompilerDefine
    {
        private const string Key = "UEE";

        static CompilerDefine()
        {
            Prefs.InvokeAfterFirstLoad(TryAddSymbols);
            Prefs.ScriptingDefineSymbolsManager.OnAddSymbolsChanged += TryAddSymbols;
        }

        private static void TryAddSymbols()
        {
            if (!Prefs.addScriptingDefineSymbols) return;

            BuildTargetGroup g = EditorUserBuildSettings.selectedBuildTargetGroup;
            
#if UNITY_2023_1_OR_NEWER
            UnityEditor.Build.NamedBuildTarget buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(g);
            string currentDefinitions = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
#else
            string currentDefinitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(g);
#endif
            
            
            if (!string.IsNullOrEmpty(currentDefinitions))
            {
                string[] keys = currentDefinitions.Split(';');
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i] == Key) return;
                }
            }

            currentDefinitions += ";" + Key;
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, currentDefinitions);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(g, currentDefinitions);
#endif

        }
    }
}