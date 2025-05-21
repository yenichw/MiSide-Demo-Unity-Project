/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class PropertyHandlerInterceptor : StatedInterceptor<PropertyHandlerInterceptor>
    {
        public static Action<SerializedProperty, GenericMenu> OnAddMenuItems;

        protected override MethodInfo originalMethod
        {
            get => PropertyHandlerRef.addMenuItemsMethod;
        }

        protected override string postfixMethodName
        {
            get => nameof(AddMenuItemsPostfix);
        }

        public override bool state
        {
            get => true;
        }

        private static void AddMenuItemsPostfix(SerializedProperty property, GenericMenu menu)
        {
            if (OnAddMenuItems == null) return;
            Delegate[] invocationList = OnAddMenuItems.GetInvocationList();
            object[] args = { property, menu };

            for (int i = 0; i < invocationList.Length; i++)
            {
                Delegate d = invocationList[i];
                try
                {
                    d.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }
    }
}