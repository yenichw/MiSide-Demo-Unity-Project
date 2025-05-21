/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class HelpIconButtonInterceptor: StatedInterceptor<HelpIconButtonInterceptor>
    {
        protected override MethodInfo originalMethod
        {
            get => EditorGUIRef.helpIconButtonMethod;
        }

        protected override string prefixMethodName
        {
            get => nameof(HelpIconButtonPrefix);
        }

        protected override InitType initType
        {
            get => InitType.gui;
        }

        public override bool state
        {
            get => Prefs.hideEmptyHelpButton;
        }

        private static Dictionary<Type, bool> typeCache = new Dictionary<Type, bool>();

        private static bool HelpIconButtonPrefix(Rect position, Object[] objs)
        {
            Object obj = objs[0];
            if (!(obj is MonoBehaviour)) return true;

            Type type = obj.GetType();
            if (type.FullName.StartsWith("UnityEngine.")) return true;

            bool v;
            if (typeCache.TryGetValue(type, out v)) return v;

            typeCache[type] = v = type.GetCustomAttribute(typeof(HelpURLAttribute)) != null;

            return v;
        }
    }
}