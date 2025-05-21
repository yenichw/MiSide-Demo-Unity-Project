/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class CurveEditorWindowRef
    {
        private static PropertyInfo _curveProp;
        private static MethodInfo _frameClipMethod;
        private static MethodInfo _getCurveEditorRectMethod;
        private static MethodInfo _onGUIMethod;
        private static FieldInfo _onCurveChangedField;
        private static MethodInfo _refreshShownCurvesMethod;
        private static MethodInfo _sendEventMethod;
        private static Type _type;

        private static PropertyInfo curveProp
        {
            get
            {
                if (_curveProp == null) _curveProp = type.GetProperty("curve", Reflection.StaticLookup);
                return _curveProp;
            }
        }

        public static MethodInfo frameClipMethod
        {
            get
            {
                if (_frameClipMethod == null) _frameClipMethod = type.GetMethod("FrameClip", Reflection.InstanceLookup);
                return _frameClipMethod;
            }
        }

        public static MethodInfo getCurveEditorRectMethod
        {
            get
            {
                if (_getCurveEditorRectMethod == null) _getCurveEditorRectMethod = type.GetMethod("GetCurveEditorRect", Reflection.InstanceLookup);
                return _getCurveEditorRectMethod;
            }
        }

        public static MethodInfo onGUIMethod
        {
            get
            {
                if (_onGUIMethod == null) _onGUIMethod = type.GetMethod("OnGUI", Reflection.InstanceLookup);
                return _onGUIMethod;
            }
        }

        public static FieldInfo onCurveChangedField
        {
            get
            {
                if (_onCurveChangedField == null) _onCurveChangedField = type.GetField("m_OnCurveChanged", Reflection.InstanceLookup);
                return _onCurveChangedField;
            }
        }

        public static MethodInfo refreshShownCurvesMethod
        {
            get
            {
                if (_refreshShownCurvesMethod == null) _refreshShownCurvesMethod = type.GetMethod("RefreshShownCurves", Reflection.InstanceLookup);
                return _refreshShownCurvesMethod;
            }
        }

        public static MethodInfo sendEventMethod
        {
            get
            {
                if (_sendEventMethod == null) _sendEventMethod = type.GetMethod("SendEvent", Reflection.InstanceLookup, null, new []{typeof(string), typeof(bool)}, null);
                return _sendEventMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("CurveEditorWindow");
                return _type;
            }
        }

        public static void FrameClip(EditorWindow window)
        {
            frameClipMethod.Invoke(window, null);
        }

        public static AnimationCurve GetCurve()
        {
            return curveProp.GetValue(null) as AnimationCurve;
        }

        public static void InvokeCurveChanged(EditorWindow window, AnimationCurve curve)
        {
            Action<AnimationCurve> action = onCurveChangedField.GetValue(window) as Action<AnimationCurve>;
            if (action != null) action(curve);
        }

        public static void RefreshShownCurves(EditorWindow window)
        {
            refreshShownCurvesMethod.Invoke(window, null);
        }

        public static void SendEvent(EditorWindow window, string eventName, bool exitGUI)
        {
            sendEventMethod.Invoke(window, new object[] {eventName, exitGUI});
        }

        public static void SetCurve(AnimationCurve curve)
        {
            curveProp.SetValue(null, curve);
        }
    }
}