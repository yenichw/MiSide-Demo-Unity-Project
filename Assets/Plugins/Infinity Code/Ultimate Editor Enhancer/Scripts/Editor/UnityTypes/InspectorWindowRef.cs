/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class InspectorWindowRef
    {
        private static PropertyInfo _trackerProp;
        private static Type _type;

        private static PropertyInfo trackerProp
        {
            get
            {
                if (_trackerProp == null) _trackerProp = type.GetProperty("tracker", Reflection.InstanceLookup);
                return _trackerProp;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("InspectorWindow");
                return _type;
            }
        }

        public static ActiveEditorTracker GetTracker(object instance)
        {
            return trackerProp.GetValue(instance) as ActiveEditorTracker;
        }
    }
}