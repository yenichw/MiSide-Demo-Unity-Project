/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class DrawEditorHeaderItemsInterceptor: StatedInterceptor<DrawEditorHeaderItemsInterceptor>
    {
        private static bool inited;

        protected override MethodInfo originalMethod
        {
            get { return EditorGUIUtilityRef.drawEditorHeaderItemsMethod; }
        }

        protected override string postfixMethodName
        {
            get { return nameof(DrawEditorHeaderItemsPostfix); }
        }

        public override bool state
        {
            get { return Prefs.componentExtraHeaderButtons; }
        }

        private static void DrawEditorHeaderItemsPostfix(ref Rect __result, Rect rectangle, Object[] targetObjs, float spacing)
        {
            if (!Prefs.componentExtraHeaderButtons) return;
            if (inited) return;

            IList s_EditorHeaderItemsMethods = Reflection.GetStaticFieldValue<IList>(typeof(EditorGUIUtility), "s_EditorHeaderItemsMethods");
            if (s_EditorHeaderItemsMethods == null) return;

            inited = true;
            var methods = TypeCache.GetMethodsWithAttribute<ComponentHeaderButtonAttribute>();
            Type headerItemDelegate = Reflection.GetEditorType("EditorGUIUtility+HeaderItemDelegate");
            
            foreach (MethodInfo method in methods.OrderBy(m => m.GetCustomAttribute<ComponentHeaderButtonAttribute>().order))
            {
                s_EditorHeaderItemsMethods.Add(Delegate.CreateDelegate(headerItemDelegate, method));
            }

            Reflection.SetStaticFieldValue(typeof(EditorGUIUtility), "s_EditorHeaderItemsMethods", s_EditorHeaderItemsMethods);

            GUI.changed = true;
        }
    }
}