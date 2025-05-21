/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        private static PrefManager[] _generalManagers;
        private static string[] _keywords;
        
        internal static PrefManager[] generalManagers
        {
            get
            {
                if (_generalManagers == null)
                {
                    _generalManagers = managers.Where(i => !i.GetType().IsSubclassOf(typeof(StandalonePrefManager))).ToArray();
                }

                return _generalManagers;
            }
        }
        
        public static void DrawGeneralManagers(string searchContext)
        {
            DrawToolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (PrefManager manager in generalManagers)
            {
                try
                {
                    EditorGUI.BeginChangeCheck();
                    manager.Draw();
                    EditorGUILayout.Space();
                    if (EditorGUI.EndChangeCheck() || forceSave)
                    {
                        Save();
                        forceSave = false;
                    }
                }
                catch (ExitGUIException)
                {
                    throw;
                }
                catch
                {
                }
            }

            EditorGUILayout.EndScrollView();
        }
        
        public static IEnumerable<string> GetGeneralManagersKeywords()
        {
            if (_keywords == null) _keywords = generalManagers.SelectMany(m => m.keywords).ToArray();
            return _keywords;
        }

        private static void SetGeneralManagersState(bool state)
        {
            foreach (PrefManager manager in generalManagers)
            {
                manager.SetState(state);
            }
        }
    }
}