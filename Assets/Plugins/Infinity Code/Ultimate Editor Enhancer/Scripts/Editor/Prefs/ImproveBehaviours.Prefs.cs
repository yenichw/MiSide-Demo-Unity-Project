/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool dragAndDropToTab = true;
        public static bool improveAddComponentBehaviour = true;
        public static bool improveDragAndDropBehaviour = true;
        public static bool improveMaximizeGameViewBehaviour = true;
        

        private class ImproveBehavioursManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Improve Behaviours",
                        "Add Component By Shortcut",
                        "Drag And Drop To Canvas",
                        "Drag And Drop To Tab",
                        "Maximize Game View By Shortcut (SHIFT + Space)",
                        "Number Fields",
                        "Curve Editor",
                    };
                }
            }

            public override float order => Order.ImproveBehaviors;

            public override void Draw()
            {
                EditorGUILayout.LabelField("Improve Behaviors");

                EditorGUI.indentLevel++;

                improveAddComponentBehaviour = EditorGUILayout.ToggleLeft("Add Component By Shortcut", improveAddComponentBehaviour);
                improveDragAndDropBehaviour = EditorGUILayout.ToggleLeft("Drag And Drop To Canvas", improveDragAndDropBehaviour);
                dragAndDropToTab = EditorGUILayout.ToggleLeft("Drag And Drop To Tab", dragAndDropToTab);
                _improveCurveEditor = EditorGUILayout.ToggleLeft("Improve Curve Editor", _improveCurveEditor);
                improveMaximizeGameViewBehaviour = EditorGUILayout.ToggleLeft("Maximize Game View By Shortcut (SHIFT + Space)", improveMaximizeGameViewBehaviour);

                EditorGUI.indentLevel--;
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>();
                if (improveAddComponentBehaviour)
                {
                    shortcuts.Add(new Shortcut("Add Component To Selected GameObject", "Everywhere",
#if !UNITY_EDITOR_OSX
                    "CTRL + SHIFT + A"
#else
                        "CMD + SHIFT + A"
#endif
                    ));
                }

                if (improveMaximizeGameViewBehaviour)
                {
                    shortcuts.Add(new Shortcut("Maximize GameView", "Game View", "SHIFT + SPACE"));
                }

                if (changeNumberFieldValueByArrow)
                {
                    EventModifiers m1 = EventModifiers.Shift;
#if !UNITY_EDITOR_OSX
                    EventModifiers m2 = EventModifiers.Control;
#else
                    EventModifiers m2 = EventModifiers.Command;
#endif

                    shortcuts.Add(new Shortcut("Increase Value By 1", "Number Field", EventModifiers.None, KeyCode.UpArrow));
                    shortcuts.Add(new Shortcut("Increase Value By 10", "Number Field", m1, KeyCode.UpArrow));
                    shortcuts.Add(new Shortcut("Increase Value By 100", "Number Field", m2, KeyCode.UpArrow));
                    shortcuts.Add(new Shortcut("Decrease Value By 1", "Number Field", EventModifiers.None, KeyCode.DownArrow));
                    shortcuts.Add(new Shortcut("Decrease Value By 10", "Number Field", m1, KeyCode.DownArrow));
                    shortcuts.Add(new Shortcut("Decrease Value By 100", "Number Field", m2, KeyCode.DownArrow));
                }

                if (changeNumberFieldValueByMouseWheel)
                {
                    shortcuts.Add(new Shortcut("Change Value By 1", "Number Field", "Mouse Wheel " + ModifierToString(changeNumberFieldValueByWheelModifiers)));
                }

                return shortcuts;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                dragAndDropToTab = state;
                improveAddComponentBehaviour = state;
                improveDragAndDropBehaviour = state;
                improveMaximizeGameViewBehaviour = state;
            }
        }
    }
}