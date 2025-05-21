/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    [HideInIntegrity]
    public static class Compatibility
    {
        public static IList GetGameViews()
        {
            return PlayModeViewRef.GetPlayModeViews();
        }

        public static VisualElement GetVisualTree(ScriptableObject scriptableObject)
        {
            object backend = GUIViewRef.windowBackendProp.GetValue(scriptableObject, null);
            return (VisualElement)IWindowBackendRef.visualTreeProp.GetValue(backend, null);
        }
    }
}