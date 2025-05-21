/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool moveToPoint = true;
        public static KeyCode moveToPointKeyCode = KeyCode.C;
        public static EventModifiers moveToPointModifiers = EventModifiers.None;
        public static bool selectCollectionAsOne = true;
        public static bool selectLODGroupAsOne = true;

        public class SceneViewManager : StandalonePrefManager<SceneViewManager>, IHasShortcutPref, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return ObjectToolbarManager.GetKeywords()
                        .Concat(SwitchCustomToolManager.GetKeywords())
                        .Concat(DistanceToolManager.GetKeywords())
                        .Concat(TerrainBrushSizeManager.GetKeywords())
                        .Concat(ToolValuesManager.GetKeywords()).Concat(new []
                        {
                            "Move To Point",
                            "Select Collection as One",
                            "Select LOD Group as One",
                        });
                }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Move To Point", ref moveToPoint, ref moveToPointKeyCode, ref moveToPointModifiers, EditorStyles.label, 17);
                ObjectToolbarManager.Draw(null);
                selectCollectionAsOne = EditorGUILayout.ToggleLeft("Select Collection as One", selectCollectionAsOne);
                selectLODGroupAsOne = EditorGUILayout.ToggleLeft("Select LOD Group as One", selectLODGroupAsOne);
                SwitchCustomToolManager.Draw(null);
                DistanceToolManager.Draw(null);
                ToolValuesManager.Draw(null);
                TerrainBrushSizeManager.Draw(null);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                return new[]
                {
                    new Shortcut("Move To Point", "Scene View", moveToPointModifiers, moveToPointKeyCode),
                };
            }

            public string GetMenuName()
            {
                return "Scene View";
            }

            public override void SetState(bool state)
            {
                moveToPoint = state;
                selectCollectionAsOne = state;
                selectLODGroupAsOne = state;
                
                GetManager<DistanceToolManager>().SetState(state);
                GetManager<HighlightManager>().SetState(state);
                GetManager<NavigationManager>().SetState(state);
                GetManager<ObjectToolbarManager>().SetState(state);
                GetManager<QuickAccessBarManager>().SetState(state);
                GetManager<SwitchCustomToolManager>().SetState(state);
                GetManager<TerrainBrushSizeManager>().SetState(state);
                GetManager<ToolValuesManager>().SetState(state);
                GetManager<WailaManager>().SetState(state);
            }
        }
    }
}