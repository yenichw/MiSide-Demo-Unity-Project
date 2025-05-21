/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    [RequireSelected]
    public class AddComponent : ActionItem, IValidatableLayoutItem
    {
        public override float order
        {
            get { return -990; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.addComponent, "Add Component");
        }

        public override void Invoke()
        {
            Vector2 s = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - s / 2, s);

            ShowAddComponent(rect);
        }

        public static void ShowAddComponent(Rect rect)
        {
            AddComponentWindowRef.Show(rect, Selection.gameObjects);

            EditorWindow wnd = EditorWindow.GetWindow(AddComponentWindowRef.type);
            wnd.position = rect;

            PinAndClose.Show(wnd, rect, wnd.Close, "Add Component");
        }

        public bool Validate()
        {
            return Prefs.actionsAddComponent || Selection.gameObjects.Length > 1;
        }
    }
}