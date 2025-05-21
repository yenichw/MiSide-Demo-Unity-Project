/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public abstract class StandalonePrefManager : PrefManager
        {

        }


        public abstract class StandalonePrefManager<T>: StandalonePrefManager where T: StandalonePrefManager<T>
        {
            private EditorWindow _settingsWindow;
            private static StandalonePrefManager<T> _instance;

            private static StandalonePrefManager<T> instance
            {
                get
                {
                    if (_instance == null) _instance = (T)Activator.CreateInstance(typeof(T));
                    return _instance;
                }
            }

            protected EditorWindow settingsWindow
            {
                get
                {
                    if (_settingsWindow == null) _settingsWindow = EditorWindow.GetWindow(ProjectSettingsWindowRef.type);
                    return _settingsWindow;
                }
            }

            public static void Draw(string filter)
            {
                try
                {
                    EditorGUI.BeginChangeCheck();
                    instance.Draw();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Save();
                    }
                }
                catch (ExitGUIException e)
                {
                    throw e;
                }
                catch (Exception e) 
                {
                    Log.Add(e);
                }
            }

            public static void DrawWithToolbar(string filter)
            {
                DrawToolbar();
                Draw(filter);
            }

            public static IEnumerable<string> GetKeywords()
            {
                return instance.keywords;
            }
        }
    }
}