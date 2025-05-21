/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class Shortcuts: EditorWindow
    {
        private static Prefs.Shortcut[] _shortcuts;
        private static string[] _contexts;
        private Vector2 scrollPosition;
        private string filter = "";
        private int contextIndex = 0;
        private IEnumerable<Prefs.Shortcut> filteredShortcuts;

        private static Prefs.Shortcut[] shortcuts
        {
            get
            {
                if (_shortcuts == null)
                {
                    _shortcuts = Prefs.managers
                        .Select(m => m as IHasShortcutPref)
                        .Where(m => m != null)
                        .SelectMany(m => m.GetShortcuts()).ToArray();
                }

                return _shortcuts;
            }
        }

        private static string[] contexts
        {
            get
            {
                if (_contexts == null) _contexts = new[] { "-" }.Concat(shortcuts.Select(s => s.context).Distinct().OrderBy(s => s)).ToArray();
                return _contexts;
            }
        }

        private void OnDestroy()
        {
            _shortcuts = null;
            _contexts = null;
        }

        private void OnGUI()
        {
            float w = position.width;
            float actionWidth = w * 0.58f - 10;
            float contextWidth = w * 0.22f - 10;
            float shortcutWidth = w * 0.2f - 10;

            bool adjusted = false;
            if (contextWidth < 190)
            {
                contextWidth = 190;
                adjusted = true;
            }

            if (shortcutWidth < 150)
            {
                shortcutWidth = 150;
                adjusted = true;
            }

            if (adjusted) actionWidth = w - contextWidth - shortcutWidth - 30;

            if (actionWidth < 260) actionWidth = 260;

            if (filteredShortcuts == null) filteredShortcuts = shortcuts;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Action", Styles.centeredLabel, GUILayout.Width(actionWidth));
            GUILayout.Label("Context", Styles.centeredLabel, GUILayout.Width(contextWidth));
            GUILayout.Label("Shortcut", Styles.centeredLabel, GUILayout.Width(shortcutWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            filter = EditorGUILayout.TextField(filter, GUILayout.Width(actionWidth));
            contextIndex = EditorGUILayout.Popup(contextIndex, contexts, GUILayout.Width(contextWidth));
            if (EditorGUI.EndChangeCheck())
            {
                filteredShortcuts = shortcuts;
                if (!string.IsNullOrEmpty(filter)) filteredShortcuts = filteredShortcuts.Where(s => s.action.ToLowerInvariant().Contains(filter.ToLowerInvariant()));
                if (contextIndex != 0)
                {
                    string context = contexts[contextIndex].ToLowerInvariant();
                    filteredShortcuts = filteredShortcuts.Where(s => s.context.ToLowerInvariant().Contains(context));
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (Prefs.Shortcut shortcut in filteredShortcuts)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(shortcut.action, GUILayout.Width(actionWidth));
                GUILayout.Label(shortcut.context, GUILayout.Width(contextWidth));
                GUILayout.Label(shortcut.shortcut, GUILayout.Width(shortcutWidth));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        [MenuItem(WindowsHelper.MenuPath + "Shortcuts", false, MenuItemOrder.Shortcuts)]
        public static void OpenWindow()
        {
            GetWindow<Shortcuts>("Shortcuts");
        }
    }
}