/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectCreateMaterial
    {
        static ProjectCreateMaterial()
        {
            ProjectItemDrawer.Register("CREATE_MATERIAL", DrawButton, ProjectToolOrder.CreateMaterial);
        }

        private static void CreateMaterial(ProjectItem item)
        {
            Selection.activeObject = item.asset;
            Material material = new Material(RenderPipelineHelper.GetDefaultShader());
            ProjectWindowUtil.CreateAsset(material, "New Material.mat");
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectCreateMaterial) return;
            if (!item.isFolder) return;
            if (!item.hovered) return;
            if (!item.path.StartsWith("Assets")) return;
            if (!item.path.Contains("Material")) return;

            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;

            if (GUI.Button(r, TempContent.Get(EditorIconContents.material.image, "Create Material"), GUIStyle.none))
            {
                CreateMaterial(item);
            }
        }
    }
}