/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectCreateMaterialFromShader
    {
        static ProjectCreateMaterialFromShader()
        {
            ProjectItemDrawer.Register("CREATE_MATERIAL_FROM_SHADER", DrawButton, ProjectToolOrder.CreateMaterialFromShader);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectCreateMaterial) return;
            if (!item.hovered) return;
            Object asset = item.asset;
            if (!asset) return;
            if (!(asset is Shader)) return;
        
            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;

            Texture icon = EditorIconContents.material.image;
            string tooltip = "Create Material With Shader";

            ButtonEvent be = GUILayoutUtils.Button(r, TempContent.Get(icon, tooltip), GUIStyle.none);
            if (be == ButtonEvent.click)
            {
                Event e = Event.current;
                if (e.button == 0)
                {
                    CreateMaterial(asset);
                }
            }
        }

        private static void CreateMaterial(Object asset)
        {
            Selection.activeObject = asset;
            Material material = new Material(asset as Shader);
            ProjectWindowUtil.CreateAsset(material, asset.name + ".mat");
        }
    }
}