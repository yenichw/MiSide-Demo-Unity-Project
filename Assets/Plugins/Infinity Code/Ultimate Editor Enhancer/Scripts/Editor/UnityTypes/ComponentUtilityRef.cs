/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditorInternal;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ComponentUtilityRef
    {
        private static Type type => typeof(ComponentUtility);

        private static MethodInfo _moveComponentRelativeToComponentMethod;

        private static MethodInfo moveComponentRelativeToComponentMethod
        {
            get
            {
                if (_moveComponentRelativeToComponentMethod == null)
                {
                    _moveComponentRelativeToComponentMethod = type.GetMethod("MoveComponentRelativeToComponent", 
                        Reflection.StaticLookup,
                        null, 
                        new[]
                        {
                            typeof(UnityEngine.Component), 
                            typeof(UnityEngine.Component), 
                            typeof(bool)
                        }, null);
                }
                return _moveComponentRelativeToComponentMethod;
            }
        }
        
        public static void MoveComponentRelativeToComponent(UnityEngine.Component component, UnityEngine.Component componentRelativeTo, bool aboveTarget)
        {
            moveComponentRelativeToComponentMethod.Invoke(null, new object[] { component, componentRelativeTo, aboveTarget });
        }
    }
}