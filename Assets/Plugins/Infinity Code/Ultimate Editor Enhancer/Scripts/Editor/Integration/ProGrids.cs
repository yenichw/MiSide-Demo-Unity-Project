/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Integration
{
    [InitializeOnLoad]
    public static class ProGrids
    {
        private static MethodInfo isEnabledMethod;
        private static FieldInfo instanceField;
        private static FieldInfo menuOpenField;
        private static MethodInfo snapEnabledMethod;
        private static MethodInfo snapToGridMethod;
        private static PropertyInfo snapValueInGridUnitsProp;
        private static PropertyInfo snapValueInUnityUnitsProp;
        private static PropertyInfo SnapModifierProp;

        public static bool isEnabled
        {
            get { return isPresent && (bool)isEnabledMethod.Invoke(null, null); }
        }

        public static bool isMenuOpen
        {
            get
            {
                if (!isPresent) return false;
                object instance = instanceField.GetValue(null);
                if (instance == null) return false;
                return (bool)menuOpenField.GetValue(instance);
            }
        }

        public static bool isPresent { get; }

        public static bool snapEnabled
        {
            get
            {
                if (!isPresent) return false;
                return (bool)snapEnabledMethod.Invoke(null, null);
            }
        }

        public static float snapValueInUnityUnits
        {
            get
            {
                if (!isEnabled) return 0;
                object instance = instanceField.GetValue(null);
                if (instance == null) return 0;
                return (float)snapValueInUnityUnitsProp.GetValue(instance);
            }
            set
            {
                if (!isEnabled) return;

                object instance = instanceField.GetValue(null);
                if (instance == null) return;

                snapValueInGridUnitsProp.SetValue(instance, value);
                SnapModifierProp.SetValue(instance, 2048);
            }
        }

        static ProGrids()
        {
            Assembly assembly = Reflection.GetAssembly("Unity.ProGrids.Editor");
            if (assembly == null) return;

            Type editorType = assembly.GetType("UnityEditor.ProGrids.ProGridsEditor");
            if (editorType == null) return;

            isEnabledMethod = Reflection.GetMethod(editorType, "IsEnabled", Reflection.StaticLookup);
            snapEnabledMethod = Reflection.GetMethod(editorType, "SnapEnabled", Reflection.StaticLookup);
            snapToGridMethod = Reflection.GetMethod(editorType, "SnapToGrid", new []{typeof(Transform[])}, Reflection.InstanceLookup);
            instanceField = editorType.GetField("s_Instance", Reflection.StaticLookup);
            menuOpenField = editorType.GetField("menuOpen", Reflection.InstanceLookup);
            snapValueInUnityUnitsProp = editorType.GetProperty("SnapValueInUnityUnits", Reflection.InstanceLookup);
            snapValueInGridUnitsProp = editorType.GetProperty("SnapValueInGridUnits", Reflection.InstanceLookup);
            SnapModifierProp = editorType.GetProperty("SnapModifier", Reflection.InstanceLookup);

            if (isEnabledMethod == null ||
                snapEnabledMethod == null ||
                instanceField == null ||
                menuOpenField == null ||
                snapValueInGridUnitsProp == null ||
                snapValueInUnityUnitsProp == null ||
                SnapModifierProp == null)
            {
                return;
            }

            isPresent = true;
        }

        private static float Snap(float val, float round)
        {
            return round * Mathf.Round(val / round);
        }

        public static Vector3 SnapToGrid(Vector3 position)
        {
            if (!snapEnabled) return position;

            float snapValue = snapValueInUnityUnits;

            return new Vector3(
                Snap(position.x, snapValue),
                Snap(position.y, snapValue),
                Snap(position.z, snapValue));
        }

        public static void SnapToGrid(Transform transform)
        {
            SnapToGrid(new []{ transform });
        }

        public static void SnapToGrid(Transform[] transforms)
        {
            if (!snapEnabled) return;
            object instance = instanceField.GetValue(null);
            snapToGridMethod.Invoke(instance, new object[]{ transforms });
        }
    }
}