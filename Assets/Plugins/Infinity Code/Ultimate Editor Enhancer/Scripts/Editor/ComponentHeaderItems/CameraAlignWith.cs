/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ComponentHeader
{
    public static class CameraAlignWith
    {
        private static bool inited;
        private static GUIContent content;

        [ComponentHeaderButton(ComponentHeaderButtonOrder.CameraAlignWith)]
        public static bool DrawHeaderButton(Rect rectangle, Object[] targets)
        {
            Object target = targets[0];
            if (!Validate(target)) return false;
            if (!inited) Init();
            
            if (GUI.Button(rectangle, content, Styles.iconButton))
            {
                ShowMenu(target as Component);
            }

            return true;
        }

        private static void ShowMenu(Component target)
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Frame Selected", () => SceneView.FrameLastActiveSceneView());
            menu.Add("Move To View", SceneView.lastActiveSceneView.MoveToView);
            menu.Add("Align With View", SceneView.lastActiveSceneView.AlignWithView);
            menu.Add("Align View To Selected", () => SceneView.lastActiveSceneView.AlignViewToObject(target.transform));
            menu.Show();
        }

        private static void Init()
        {
            if (content == null)
            {
                content = new GUIContent(EditorGUIUtility.isProSkin? Icons.align : Icons.alignDark, "Align With");
            }
            
            inited = true;
        }

        private static bool Validate(Object target)
        {
            if (!Prefs.componentExtraHeaderButtons || !Prefs.cameraAlignWith) return false;
            if (!(target is Camera)) return false;
            return true;
        }
    }
}