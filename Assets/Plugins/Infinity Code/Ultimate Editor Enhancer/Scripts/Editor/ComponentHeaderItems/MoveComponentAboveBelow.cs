/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.ComponentHeader
{
    public static class MoveComponentAboveBelow
    {
        [ComponentHeaderButton(ComponentHeaderButtonOrder.MoveComponentUpDown)]
        public static bool DrawButton(Rect rect, Object[] targets)
        {
            if (!Prefs.inspectorMoveAboveBelow) return false;
            if (targets.Length != 1) return false;
            
            Component component = targets[0] as Component;
            if (component == null) return false;
            if (component is Transform) return false;
            
#if UNITY_6000_0_OR_NEWER
            if (component.gameObject.GetComponentCount() < 3) return false;
#else 
            if (component.gameObject.GetComponents<Component>().Length < 3) return false;
#endif

            GUIContent content = TempContent.Get(Icons.moveUpDown, "Move Component Above/Below Than");

            ButtonEvent buttonEvent = GUILayoutUtils.Button(rect, content, GUIStyle.none);
            if (buttonEvent == ButtonEvent.click) MoveComponent(component);
            
            return true;
        }

        private static void MoveComponent(Component component)
        {
            GenericMenuEx menu = GenericMenuEx.Start();

            Component[] components = component.gameObject.GetComponents<Component>();
            Span<Component> aboveComponent = new Span<Component>(components, 1, components.Length - 1);
            Span<Component> belowComponent = new Span<Component>(components, 0, components.Length - 1);

            for (int i = 0; i < aboveComponent.Length; i++)
            {
                Component c = aboveComponent[i];
                if (c == component) continue;
                
                string name = ObjectNames.NicifyVariableName(c.GetType().Name);
                menu.Add(new GUIContent("Move Above Than/" + name), () => SetComponentIndex(component, c, true));
            }

            for (int i = 0; i < belowComponent.Length; i++)
            {
                Component c = belowComponent[i];
                if (c == component) continue;
                
                string name = ObjectNames.NicifyVariableName(c.GetType().Name);
                menu.Add(new GUIContent("Move Below Than/" + name), () => SetComponentIndex(component, c, false));
            }
            
            menu.Show();
        }

        private static void SetComponentIndex(Component component, Component targetComponent, bool above)
        {
            ComponentUtilityRef.MoveComponentRelativeToComponent(component, targetComponent, above);
        }
    }
}