/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class UnityEventDrawerRef
    {
        private static MethodInfo _buildPopupListMethod;
        private static MethodInfo _generatePopUpForTypeMethod;
        private static MethodInfo _getDummyEventMethod;

        private static MethodInfo buildPopupListMethod
        {
            get
            {
                if (_buildPopupListMethod == null)
                {
                    _buildPopupListMethod = Reflection.GetMethod(
                        type, 
                        "BuildPopupList", 
                        new[] { typeof(Object), typeof(UnityEventBase), typeof(SerializedProperty) }, 
                        Reflection.StaticLookup);
                }

                return _buildPopupListMethod;
            }
        }
        
        private static MethodInfo generatePopUpForTypeMethod
        {
            get
            {
                if (_generatePopUpForTypeMethod == null)
                {
#if UNITY_2021_3_OR_NEWER || UNITY_2020_3
                    Type[] types = new[] { typeof(GenericMenu), typeof(Object), typeof(string), typeof(SerializedProperty), typeof(Type[]) };
#else
                    Type[] types = new[] { typeof(GenericMenu), typeof(Object), typeof(bool), typeof(SerializedProperty), typeof(Type[]) };
#endif

                    _generatePopUpForTypeMethod = Reflection.GetMethod(
                        type, 
                        "GeneratePopUpForType", 
                        types, 
                        Reflection.StaticLookup);
                }

                return _generatePopUpForTypeMethod;
            }
        }

        private static MethodInfo getDummyEventMethod
        {
            get
            {
                if (_getDummyEventMethod == null)
                {
                    _getDummyEventMethod = Reflection.GetMethod(
                        type, 
                        "GetDummyEvent", 
                        new[] { typeof(SerializedProperty) }, 
                        Reflection.StaticLookup);
                }
                
                return _getDummyEventMethod;
            }
        }

        public static Type type
        {
            get { return typeof(UnityEventDrawer); }
        }

        public static GenericMenu BuildPopupList(Object target, UnityEventBase dummyEvent, SerializedProperty listener)
        {
            return (GenericMenu) buildPopupListMethod.Invoke(
                null, 
                new object[]
                {
                    target, 
                    dummyEvent, 
                    listener
                }); 
        }

        public static void GeneratePopUpForType(
            GenericMenu menu,
            Object target,
            string targetName,
            SerializedProperty listener,
            Type[] delegateArgumentsTypes)
        {
            object[] parameters = new object[]
            {
                menu, 
                target, 
#if UNITY_2021_2_OR_NEWER || UNITY_2020_3
                targetName,
#else
                true,
#endif
                listener, 
                delegateArgumentsTypes
            };
            generatePopUpForTypeMethod.Invoke(
                null, 
                parameters);
        }

        public static UnityEventBase GetDummyEvent(SerializedProperty prop)
        {
            return getDummyEventMethod.Invoke(null, new object[] { prop }) as UnityEventBase;
        }
    }
}