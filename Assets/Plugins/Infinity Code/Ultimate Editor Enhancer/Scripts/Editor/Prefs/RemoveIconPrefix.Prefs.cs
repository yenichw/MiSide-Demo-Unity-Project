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
        public static string removeIconPrefix = "Online Maps;Real World Terrain";

        private static string[] _removeIconPrefix;

        public static string RemoveIconPrefix(string text)
        {
            if (_removeIconPrefix == null) _removeIconPrefix = removeIconPrefix.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string p in _removeIconPrefix)
            {
                if (text.Length > p.Length && text.StartsWith(p)) return text.Substring(p.Length);
            }

            return text;
        }

        private class RemoveIconPrefixManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Remove Prefix"
                    };
                }
            }

            public override float order
            {
                get { return Order.RemoveComponentIconPrefix; }
            }

            public override void Draw()
            {
                if (_removeIconPrefix == null) _removeIconPrefix = removeIconPrefix.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                EditorGUILayout.LabelField("Remove Component Icon Prefix");

                EditorGUI.BeginChangeCheck();

                EditorGUI.indentLevel++;

                int removeIndex = -1;
                for (int i = 0; i < _removeIconPrefix.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _removeIconPrefix[i] = EditorGUILayout.TextField(_removeIconPrefix[i]);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                if (GUILayout.Button("Add"))
                {
                    ArrayUtility.Add(ref _removeIconPrefix, "");
                }

                EditorGUILayout.EndHorizontal();

                if (removeIndex != -1)
                {
                    ArrayUtility.RemoveAt(ref _removeIconPrefix, removeIndex);
                    UpdateRemoveIconPrefix();
                    GUI.changed = true;
                }

                if (EditorGUI.EndChangeCheck()) UpdateRemoveIconPrefix();
            }

            private static void UpdateRemoveIconPrefix()
            {
                removeIconPrefix = string.Join(";", _removeIconPrefix.Where(s => !string.IsNullOrEmpty(s)).ToArray());
            }

        }
    }
}