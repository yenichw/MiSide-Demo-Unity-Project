/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool contextMenuOnRightClick = true;
        public static EventModifiers rightClickModifiers = EventModifiers.None;
        public static bool pickGameObject = true;

#if !UNITY_EDITOR_OSX
        public static EventModifiers pickGameObjectModifiers = EventModifiers.Control;
        public static KeyCode contextMenuHotKey = KeyCode.Space;
        public static EventModifiers contextMenuHotKeyModifiers = EventModifiers.Control;
#else
        public static EventModifiers pickGameObjectModifiers = EventModifiers.Command;
        public static KeyCode contextMenuHotKey = KeyCode.Space;
        public static EventModifiers contextMenuHotKeyModifiers = EventModifiers.Command;
#endif

        public static bool contextMenuOnHotKey = true;
        public static bool contextMenuDisableInPlayMode = false;
        public static bool contextMenuPauseInPlayMode = false;

        public class ContextMenuMainManager : StandalonePrefManager<ContextMenuMainManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Context Menu",
                        "Show Context Menu On Hot Key",
                        "Disable Context Menu In Play Mode",
                        "Pause In Play Mode",
                        "Favorite Windows In Context Menu",
                        "Show Context Menu On Right Click",
                        "Window Size",
                    };
                }
            }

            public override float order
            {
                get { return -100; }
            }

            public override void Draw()
            {
                DrawRightClickContent();
                DrawFieldWithHotKey("Show Context Menu By Hot Key", ref contextMenuOnHotKey, ref contextMenuHotKey, ref contextMenuHotKeyModifiers);

                contextMenuDisableInPlayMode = EditorGUILayout.ToggleLeft("Disable Context Menu In Play Mode", contextMenuDisableInPlayMode);
                contextMenuPauseInPlayMode = EditorGUILayout.ToggleLeft("Pause In Play Mode", contextMenuPauseInPlayMode);
            }

            private static void DrawRightClickContent()
            {
                contextMenuOnRightClick = EditorGUILayout.ToggleLeft("Show Context Menu By Right Click", contextMenuOnRightClick);
                EditorGUI.BeginDisabledGroup(!contextMenuOnRightClick);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(SpaceBeforeModifiers);
                GUILayout.Label("Modifiers", GUILayout.Width(ModifierLabelWidth));
                rightClickModifiers = DrawModifiers(rightClickModifiers);
                EditorGUILayout.EndHorizontal();

                pickGameObject = EditorGUILayout.ToggleLeft("Pick GameObject", pickGameObject);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(SpaceBeforeModifiers);
                GUILayout.Label("Modifiers", GUILayout.Width(ModifierLabelWidth));
                pickGameObjectModifiers = DrawModifiers(pickGameObjectModifiers);
                EditorGUILayout.EndHorizontal();

                if (contextMenuOnRightClick && pickGameObject && rightClickModifiers == pickGameObjectModifiers)
                {
                    EditorGUILayout.HelpBox("The modifiers for the right click and the pick GameObject must be different.", MessageType.Error);
                }

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>();

                if (contextMenuOnRightClick)
                {
                    shortcuts.Add(new Shortcut("Show Context Menu", "Scene View", rightClickModifiers, "RMB"));
                }

                shortcuts.Add(new Shortcut("Show Context Menu", "Everywhere", contextMenuHotKeyModifiers, contextMenuHotKey));

                if (pickGameObject)
                {
                    shortcuts.Add(new Shortcut("Pick GameObject and Show Context Menu", "Scene View", pickGameObjectModifiers, "RMB"));
                }

                shortcuts.Add(new Shortcut("Close Context Menu", "Context Menu", "Escape"));
                shortcuts.Add(new Shortcut("Close Context Menu", "Everywhere", "LMB or RMB"));

                return shortcuts;
            }


            public override void SetState(bool state)
            {
                base.SetState(state);
                
                contextMenuOnRightClick = state;
                contextMenuOnHotKey = state;
            }
        }
    }
}