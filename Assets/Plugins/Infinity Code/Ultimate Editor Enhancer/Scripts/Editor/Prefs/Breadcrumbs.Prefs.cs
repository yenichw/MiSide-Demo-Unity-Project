/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool breadcrumbs = true;
        public static bool breadcrumbsParent = true;
        public static bool breadcrumbsParentUp = true;
        public static EventModifiers breadcrumbsParentUpModifiers = EventModifiers.None;
        public static bool breadcrumbsParentShowAll = true;
        public static EventModifiers breadcrumbsParentShowAllModifiers = EventModifiers.Control;

        public static bool breadcrumbsNeighbors = true;
        public static bool breadcrumbsChildren = true;

        private class BreadcrumbsManager : StandalonePrefManager<BreadcrumbsManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Breadcrumbs", "Neighbors", "Children", "Parent", "Up", "Show All" }; }
            }

            public override float order
            {
                get { return -70; }
            }

            public override void Draw()
            {
                breadcrumbs = EditorGUILayout.ToggleLeft("Breadcrumbs", breadcrumbs, EditorStyles.label);
                EditorGUI.BeginDisabledGroup(!breadcrumbs);
                EditorGUI.indentLevel++;

                DrawParent();

                breadcrumbsNeighbors = EditorGUILayout.ToggleLeft("Neighbors", breadcrumbsNeighbors);
                breadcrumbsChildren = EditorGUILayout.ToggleLeft("Children", breadcrumbsChildren);

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            private static void DrawParent()
            {
                breadcrumbsParent = EditorGUILayout.ToggleLeft("Parent", breadcrumbsParent);

                EditorGUI.BeginDisabledGroup(!breadcrumbsParent);
                EditorGUI.indentLevel++;

                DrawFieldWithModifiers("Up", ref breadcrumbsParentUp, ref breadcrumbsParentUpModifiers);
                DrawFieldWithModifiers("Show All", ref breadcrumbsParentShowAll, ref breadcrumbsParentShowAllModifiers);

                if (breadcrumbsParentUp && breadcrumbsParentShowAll && breadcrumbsParentUpModifiers == breadcrumbsParentShowAllModifiers)
                {
                    EditorGUILayout.HelpBox("The modifiers for Up and Show All must be different.", MessageType.Error);
                }

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!breadcrumbs) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Show All Parents", "Context Menu/Breadcrumbs", breadcrumbsParentShowAllModifiers),
                    new Shortcut("Show Context Menu For GameObject", "Context Menu/Breadcrumbs", "RMB"),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                breadcrumbs = state;
            }
        }
    }
}