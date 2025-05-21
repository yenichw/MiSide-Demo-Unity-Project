/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class ComponentUtils
    {
        public static bool CanBeDisabled(Component component)
        {
            return component != null && 
                   (component is Behaviour || component is Renderer || component is Collider);
        }

        public static bool GetEnabled(Component component)
        {
            if (component== null) return false;
            if (component is Behaviour) return (component as Behaviour).enabled;
            if (component is Renderer) return (component as Renderer).enabled;
            if (component is Collider) return (component as Collider).enabled;
            return true;
        }

        public static void SetEnabled(Component component, bool value)
        {
            if (component == null) return;
            if (component is Behaviour) (component as Behaviour).enabled = value;
            else if (component is Renderer) (component as Renderer).enabled = value;
            else if (component is Collider) (component as Collider).enabled = value;
        }

        public static void ShowContextMenu(Object target)
        {
            if (target == null) return;
            EditorUtilityRef.DisplayObjectContextMenu(new Rect(Event.current.mousePosition, Vector2.zero), target, 0);
        }
    }
}