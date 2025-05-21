/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ActiveEditorTrackerRef
    {
        private static MethodInfo _setObjectsLockedByThisTrackerMethod;

        private static MethodInfo setObjectsLockedByThisTrackerMethod
        {
            get
            {
                if (_setObjectsLockedByThisTrackerMethod == null) _setObjectsLockedByThisTrackerMethod = type.GetMethod("SetObjectsLockedByThisTracker", Reflection.InstanceLookup, null, new[] { typeof(List<UnityEngine.Object>) }, null);
                return _setObjectsLockedByThisTrackerMethod;
            }
        }
        
        private static Type type => typeof(ActiveEditorTracker);
        
        public static void SetObjectsLockedByThisTracker(ActiveEditorTracker tracker, List<UnityEngine.Object> objects)
        {
            setObjectsLockedByThisTrackerMethod.Invoke(tracker, new object[] { objects });
        }
    }
}