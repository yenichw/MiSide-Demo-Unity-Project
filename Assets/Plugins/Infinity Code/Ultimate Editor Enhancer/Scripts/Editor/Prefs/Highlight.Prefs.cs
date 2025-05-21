/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool highlight = true;
        public static bool highlightNoRenderer = true;
        public static Color highlightColor = new Color(1, 1, 0, 0.2f);
        public static Color highlightNoRendererColor = Color.red;
        public static Color highlightUIColor = new Color(1, 1, 0, 1f);
        public static bool highlightOnWaila = true;
        public static bool highlightOnHierarchy = true;

#if UNITY_EDITOR_OSX
        public static EventModifiers highlightOnHierarchyModifiers = EventModifiers.Command;
#else
        public static EventModifiers highlightOnHierarchyModifiers = EventModifiers.Control;
#endif
        public static bool highlightHierarchyRow = true;
        public static Color highlightHierarchyRowColor = Color.red;

        public class HighlightManager : StandalonePrefManager<HighlightManager>, IHasShortcutPref
        {

            private const string HighlightRowLabel = "Highlight Hierarchy Row";

            public override IEnumerable<string> keywords
            {
                get { return new[]
                {
                    "Highlighter", 
                    "Color", 
                    "Highlight On Waila", 
                    "Highlight On Hierarchy", 
                    "Hierarchy Row Color", 
                    "Highlight Hierarchy Row"
                }; }
            }

            public override float order
            {
                get { return -47; }
            }

            public override void Draw()
            {
                highlight = EditorGUILayout.ToggleLeft("Highlighter", highlight);

                EditorGUI.BeginDisabledGroup(!highlight);
                EditorGUI.indentLevel++;

                highlightColor = EditorGUILayout.ColorField("Renderer Color", highlightColor);
                highlightUIColor = EditorGUILayout.ColorField("UI Color", highlightUIColor);
                highlightHierarchyRowColor = EditorGUILayout.ColorField("Hierarchy Row Color", highlightHierarchyRowColor);
                
                highlightOnWaila = EditorGUILayout.ToggleLeft("Highlight On Waila", highlightOnWaila);
                DrawFieldWithModifiers("Highlight On Hierarchy", ref highlightOnHierarchy, ref highlightOnHierarchyModifiers);
                highlightHierarchyRow = EditorGUILayout.ToggleLeft(HighlightRowLabel, highlightHierarchyRow);

                highlightNoRenderer = EditorGUILayout.ToggleLeft("Highlight Objects Without Renderer", highlightNoRenderer);
                EditorGUI.BeginDisabledGroup(!highlightNoRenderer);
                EditorGUI.indentLevel++;
                highlightNoRendererColor = EditorGUILayout.ColorField("Arrow Color", highlightNoRendererColor);
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!highlight || !highlightOnHierarchy) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Highlight GameObject In Scene View", "Hierarchy", highlightOnHierarchyModifiers)
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                highlight = state;
                highlightOnWaila = state;
                highlightOnHierarchy = state;
                highlightHierarchyRow = state;
            }
        }
    }
}