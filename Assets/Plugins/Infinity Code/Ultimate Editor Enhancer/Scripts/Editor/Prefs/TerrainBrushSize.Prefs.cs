/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool terrainBrushSize = true;
        public static bool terrainBrushSizeBoost = true;

#if !UNITY_EDITOR_OSX
        public static EventModifiers terrainBrushSizeModifiers = EventModifiers.Control;
        public static EventModifiers terrainBrushSizeBoostModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers terrainBrushSizeModifiers = EventModifiers.Command;
        public static EventModifiers terrainBrushSizeBoostModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        public class TerrainBrushSizeManager : StandalonePrefManager<TerrainBrushSizeManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Terrain Brush Size",
                        "Boost"
                    };
                }
            }

            public override void Draw()
            {
                terrainBrushSize = EditorGUILayout.ToggleLeft("Terrain Brush Size", terrainBrushSize);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!terrainBrushSize);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Modifiers", GUILayout.Width(LabelWidth));
                terrainBrushSizeModifiers = DrawModifiers(terrainBrushSizeModifiers);
                EditorGUILayout.EndHorizontal();

                DrawFieldWithModifiers("Boost", ref terrainBrushSizeBoost, ref terrainBrushSizeBoostModifiers);

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!terrainBrushSize) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Change size of Terrain Brush", "Scene View", terrainBrushSizeModifiers, "Mouse Wheel"),
                    new Shortcut("Fast Change size of Terrain Brush", "Scene View", terrainBrushSizeBoostModifiers, "Mouse Wheel"),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                terrainBrushSize = state;
                terrainBrushSizeBoost = state;
            }
        }
    }
}