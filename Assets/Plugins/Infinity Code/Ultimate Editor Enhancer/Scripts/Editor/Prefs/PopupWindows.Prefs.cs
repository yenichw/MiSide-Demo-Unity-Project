/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool popupWindows = true;
        public static bool popupWindowPopup = true;
        public static EventModifiers popupWindowPopupModifiers = EventModifiers.None;
        public static bool popupWindowUtility = true;
        public static EventModifiers popupWindowUtilityModifiers = EventModifiers.Shift;
#if !UNITY_EDITOR_OSX
        public static EventModifiers popupWindowTabModifiers = EventModifiers.Control;
#else
        public static EventModifiers popupWindowTabModifiers = EventModifiers.Command;
#endif
        public static bool popupWindowTab = true;

        public static bool componentsInPopupWindow = true;
        public static bool componentsInPopupWindowAddComponent = true;

        private class PopupWindowManager : StandalonePrefManager<PopupWindowManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Windows From Context Menu",
                        "Popup Windows",
                        "Utility Windows",
                        "Components Section",
                        "Add Component"
                    };
                }
            }

            public override float order
            {
                get { return -90; }
            }

            public override void Draw()
            {
                popupWindows = EditorGUILayout.ToggleLeft("Windows Section", popupWindows, EditorStyles.label);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!popupWindows);

                DrawFieldWithModifiers("Popup Windows", ref popupWindowPopup, ref popupWindowPopupModifiers);
                DrawFieldWithModifiers("Utility Windows", ref popupWindowUtility, ref popupWindowUtilityModifiers);

                if (popupWindowPopup && popupWindowUtility && popupWindowPopupModifiers == popupWindowUtilityModifiers)
                {
                    EditorGUILayout.HelpBox("The modifiers for Popup and Utility must be different.", MessageType.Error);
                }

                DrawFieldWithModifiers("Tab Windows", ref popupWindowTab, ref popupWindowTabModifiers);

                if (popupWindowPopup && popupWindowTab && popupWindowPopupModifiers == popupWindowTabModifiers)
                {
                    EditorGUILayout.HelpBox("The modifiers for Popup and Tab must be different.", MessageType.Error);
                }
                if (popupWindowTab && popupWindowUtility && popupWindowTabModifiers == popupWindowUtilityModifiers)
                {
                    EditorGUILayout.HelpBox("The modifiers for Utility and Tab must be different.", MessageType.Error);
                }

                componentsInPopupWindow = EditorGUILayout.ToggleLeft("Components Section", componentsInPopupWindow);

                EditorGUI.BeginDisabledGroup(!componentsInPopupWindow);
                EditorGUI.indentLevel++;
                componentsInPopupWindow = EditorGUILayout.ToggleLeft("Add Component", componentsInPopupWindow);
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!popupWindows) return new Shortcut[0];

                List<Shortcut> shortcuts = new List<Shortcut>();

                if (popupWindowPopup)
                {
                    shortcuts.Add(new Shortcut("Show Window As Popup", "Context Menu/Windows", popupWindowPopupModifiers, "LMB"));
                }

                if (popupWindowUtility)
                {
                    shortcuts.Add(new Shortcut("Show Window As Utility", "Context Menu/Windows", popupWindowUtilityModifiers, "LMB"));
                }

                if (popupWindowTab)
                {
                    shortcuts.Add(new Shortcut("Show Window As Tab", "Context Menu/Windows", popupWindowTabModifiers, "LMB"));
                }

                shortcuts.Add(new Shortcut("Show Context Menu For Component", "Context Menu/Windows", "RMB"));
                shortcuts.Add(new Shortcut("Start Drag Component", "Context Menu/Components", "Drag Item"));

                return shortcuts;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                popupWindows = state;
            }
        }
    }
}