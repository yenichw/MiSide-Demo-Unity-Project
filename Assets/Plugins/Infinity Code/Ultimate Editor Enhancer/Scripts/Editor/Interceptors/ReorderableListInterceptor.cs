/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.InspectorTools;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class ReorderableListInterceptor: StatedInterceptor<ReorderableListInterceptor>
    {
        private static int indentLevel;

        public static bool insideList
        {
            get { return indentLevel > 0; }
        }

        protected override MethodInfo originalMethod
        {
            get => ReorderableListRef.doListElementsMethod;
        }

        public override bool state
        {
            get => !Prefs.nestedEditorInReorderableList;
        }

        protected override string prefixMethodName
        {
            get => nameof(DoListElementsPrefix);
        }

        protected override string postfixMethodName
        {
            get => nameof(DoListElementsPostfix);
        }

        private static void DoListElementsPrefix()
        {
            indentLevel++;
            if (Prefs.nestedEditorInReorderableList) return;
            NestedEditor.disallowNestedEditors = true;
        }

        private static void DoListElementsPostfix()
        {
            indentLevel--;
            if (Prefs.nestedEditorInReorderableList) return;
            NestedEditor.disallowNestedEditors = indentLevel > 0;
        }

        protected override void Unpatch()
        {
            base.Unpatch();
            NestedEditor.disallowNestedEditors = false;
        }
    }
}