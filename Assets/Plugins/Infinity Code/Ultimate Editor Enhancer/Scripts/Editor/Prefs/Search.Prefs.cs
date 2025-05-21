/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool search = true;
        public static bool searchScript = true;

        public static bool searchPauseInPlayMode = false;
        public static bool searchByProject = true;
        public static bool searchByComponents = false;
        public static bool searchByWindow = true;
        public static string searchDoNotShowOnWindows = "UnityEditor.ProfilerWindow;UnityEditor.SettingsWindow;InfinityCode.UltimateEditorEnhancer.Windows.BookmarkWindow";

        public static KeyCode searchKeyCode = KeyCode.F;
        public static KeyCode searchScriptKeyCode = KeyCode.T;
#if !UNITY_EDITOR_OSX
        public static EventModifiers searchModifiers = EventModifiers.Control;
        public static EventModifiers searchScriptModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers searchModifiers = EventModifiers.Command;
        public static EventModifiers searchScriptModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        private static string[] _searchDoNotShowOnWindows;
        private static bool searchWaitWindowChanged;

        public static bool SearchDoNotShowOnWindows()
        {
            if (EditorWindow.focusedWindow == null) return false;
            if (string.IsNullOrEmpty(searchDoNotShowOnWindows)) return false;
            if (_searchDoNotShowOnWindows == null)
            {
                _searchDoNotShowOnWindows = searchDoNotShowOnWindows.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < _searchDoNotShowOnWindows.Length; i++) _searchDoNotShowOnWindows[i] = _searchDoNotShowOnWindows[i].Trim();
            }

            if (_searchDoNotShowOnWindows.Length == 0) return false;

            Type windowType = EditorWindow.focusedWindow.GetType();
            return _searchDoNotShowOnWindows.Contains(windowType.FullName);
        }

        public class SearchManager : StandalonePrefManager<SearchManager>, IHasShortcutPref, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Search",
                        "Do Not Show On Windows",
                        "Pause In Play Mode",
                        "Search by Project",
                        "Search by Components",
                        "Search by Window"
                    };
                }
            }

            public override float order
            {
                get { return -45; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Search", ref search, ref searchKeyCode, ref searchModifiers, EditorStyles.label, 0);
                DrawFieldWithHotKey("Search For Scripts", ref searchScript, ref searchScriptKeyCode, ref searchScriptModifiers, EditorStyles.label, 0);

                EditorGUI.BeginDisabledGroup(!search);

                EditorGUI.BeginChangeCheck();
                const int delta = 65;
                EditorGUIUtility.labelWidth += delta;

                if (_searchDoNotShowOnWindows == null)
                {
                    _searchDoNotShowOnWindows = searchDoNotShowOnWindows.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < _searchDoNotShowOnWindows.Length; i++) _searchDoNotShowOnWindows[i] = _searchDoNotShowOnWindows[i].Trim();
                }

                EditorGUILayout.LabelField("Do Not Show On Windows");

                EditorGUI.indentLevel++;
                int removeIndex = -1;
                for (int i = 0; i < _searchDoNotShowOnWindows.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _searchDoNotShowOnWindows[i] = EditorGUILayout.TextField(_searchDoNotShowOnWindows[i]);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal(); 
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16); 
                if (GUILayout.Button("Add"))
                {
                    ArrayUtility.Add(ref _searchDoNotShowOnWindows, ""); 
                }

                if (GUILayout.Button(!searchWaitWindowChanged? "Pick": "Stop Pick", GUILayout.Width(200)))
                {
                    searchWaitWindowChanged = !searchWaitWindowChanged;
                    if (searchWaitWindowChanged)
                    {
                        EditorApplication.update -= WaitWindowChanged;
                        EditorApplication.update += WaitWindowChanged;
                    }
                    else
                    {
                        EditorApplication.update -= WaitWindowChanged;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (searchWaitWindowChanged)
                {
                    EditorGUILayout.HelpBox("Set the focus on the window you want to add to the black list.", MessageType.Info);
                }

                if (removeIndex != -1)
                {
                    ArrayUtility.RemoveAt(ref _searchDoNotShowOnWindows, removeIndex);
                    UpdateSearchDoNotShowOnWindow();
                    GUI.changed = true;
                }

                EditorGUI.indentLevel--;

                EditorGUIUtility.labelWidth -= delta;
                if (EditorGUI.EndChangeCheck()) UpdateSearchDoNotShowOnWindow();

                searchPauseInPlayMode = EditorGUILayout.ToggleLeft("Pause In Play Mode", searchPauseInPlayMode);
                searchByProject = EditorGUILayout.ToggleLeft("Search by Project", searchByProject);
                searchByComponents = EditorGUILayout.ToggleLeft("Search by Components", searchByComponents);
                searchByWindow = EditorGUILayout.ToggleLeft("Search by Window", searchByWindow);

                EditorGUI.EndDisabledGroup();
            }

            public string GetMenuName()
            {
                return "Search";
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!search) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Show Search Window", "Everywhere", searchModifiers, searchKeyCode), 
                    new Shortcut("Switch Search Source", "Search", "Tab"),
                    new Shortcut("Prev Item", "Search", "Up"), 
                    new Shortcut("Next Item", "Search", "Down"),
                    new Shortcut("Perform Default Action", "Search", "Enter or Double Click"),
                    new Shortcut("Perform Alternative Action", "Search",
#if !UNITY_EDITOR_OSX
                        "CTRL + Enter"
#else
                        "CMD + Enter"
#endif
                        ),
                    new Shortcut("Close Window", "Search", "Escape"),
                    new Shortcut("Start Drag Item", "Search", "Drag Item"),
                    new Shortcut("Show Context Menu For Item", "Search", "RMB"),
                };
            }

            public override void SetState(bool state)
            { 
                base.SetState(state);
                
                search = state;
                searchScript = state;
            }

            private static void UpdateSearchDoNotShowOnWindow()
            {
                searchDoNotShowOnWindows = string.Join(";", _searchDoNotShowOnWindows.Where(s => !string.IsNullOrEmpty(s)).ToArray());
            }

            private void WaitWindowChanged()
            {
                EditorWindow wnd = EditorWindow.focusedWindow;

                if (wnd == null) return;
                string wndType = wnd.GetType().ToString();
                if (wndType == "UnityEditor.ProjectSettingsWindow") return;

                EditorApplication.update -= WaitWindowChanged;
                ArrayUtility.Add(ref _searchDoNotShowOnWindows, wndType);
                UpdateSearchDoNotShowOnWindow();
                searchWaitWindowChanged = false;
                forceSave = true;
            }
        }
    }
}