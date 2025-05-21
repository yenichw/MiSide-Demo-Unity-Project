/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class NumberFieldInterceptor: StatedInterceptor<NumberFieldInterceptor>
    {
        private static string recycledText;

        protected override MethodInfo originalMethod
        {
            get => EditorGUIRef.doNumberFieldMethod;
        }

        public override bool state
        {
            get => Prefs.changeNumberFieldValueByArrow;
        }

        protected override string prefixMethodName
        {
            get => nameof(DoNumberFieldPrefix);
        }

        protected override InitType initType
        {
            get => InitType.gui;
        }

        private static void DoNumberFieldPrefix(
            object editor,
            Rect position,
            Rect dragHotZone,
            int id,
#if !UNITY_2021_2_OR_NEWER
            bool isDouble,
            ref double doubleVal,
            ref long longVal,
#else
            ref EditorGUIRef.NumberFieldValue value,
#endif
            string formatString,
            GUIStyle style,
            bool draggable,
            double dragSensitivity)
        {
            if (GUIUtility.keyboardControl != id) return;
            Event e = Event.current;
            int v = 0;
            float d = 1;
            
            if (e.type == EventType.KeyDown) ProcessKeyDown(ref v, ref d);
            else if (e.type == EventType.ScrollWheel && position.Contains(e.mousePosition)) ProcessScrollWheel(ref v, ref d);
            else return;

            if (v == 0) return;
            
#if !UNITY_2021_2_OR_NEWER
            if (isDouble)
            {
                if (!double.IsInfinity(doubleVal) && !double.IsNaN(doubleVal))
                {
                    doubleVal += v / d;
                    recycledText = doubleVal.ToString(Culture.numberFormat);
                    GUI.changed = true;
                }
            }
            else
            {
                longVal += v;
                recycledText = longVal.ToString();
                GUI.changed = true;
            }
#else 
            if (value.isDouble)
            {
                if (!double.IsInfinity(value.doubleVal) && !double.IsNaN(value.doubleVal))
                {
                    value.doubleVal += v / d;
                    value.success = true;
                    recycledText = value.doubleVal.ToString(Culture.numberFormat);
                    GUI.changed = true;
                }
            }
            else
            {
                value.longVal += v;
                value.success = true;
                recycledText = value.longVal.ToString();
                GUI.changed = true;
            }
#endif

            TextEditor textEditor = editor as TextEditor;
            if (textEditor != null)
            {
                textEditor.text = recycledText;
                textEditor.SelectAll();
            }
            
        }

        private static void ProcessScrollWheel(ref int v, ref float d)
        {
            if (!Prefs.changeNumberFieldValueByMouseWheel) return;
            Event e = Event.current;
            
            if (e.modifiers != Prefs.changeNumberFieldValueByWheelModifiers) return;
            v = e.delta.y > 0? 1: -1;
            e.Use();
        }

        private static void ProcessKeyDown(ref int v, ref float d)
        {
            Event e = Event.current;
            if (e.keyCode == KeyCode.UpArrow)
            {
                if (e.control || e.command)
                {
                    v = 1;
                    d = 10;
                }
                else if (e.shift) v = 10;
                else v = 1;

                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                if (e.control || e.command)
                {
                    v = -1;
                    d = 10;
                }
                else if (e.shift) v = -10;
                else v = -1;

                e.Use();
            }
        }
    }
}