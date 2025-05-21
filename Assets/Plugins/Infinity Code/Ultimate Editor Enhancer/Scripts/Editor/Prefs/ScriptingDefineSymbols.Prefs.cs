/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool addScriptingDefineSymbols = true;

        public class ScriptingDefineSymbolsManager : PrefManager
        {
            public static Action OnAddSymbolsChanged;

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Add Scripting Define Symbols",
                    };
                }
            }

            public override float order
            {
                get { return Order.AddScriptingDefineSymbols; }
            }

            public override void Draw()
            {
                EditorGUI.BeginChangeCheck();
                addScriptingDefineSymbols = EditorGUILayout.ToggleLeft("Add Scripting Define Symbols", addScriptingDefineSymbols);
                if (EditorGUI.EndChangeCheck())
                {
                    if (OnAddSymbolsChanged != null) OnAddSymbolsChanged();
                }
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                addScriptingDefineSymbols = state;
            }
        }
    }
}