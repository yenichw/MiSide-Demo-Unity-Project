/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.TransformEditorTools
{
    public class AlignTool: TransformEditorTool
    {
        private static void Align(Transform[] transforms, int side, float xMul, float yMul, float zMul)
        {
            GameObjectUtils.Align(transforms.Select(t => t.gameObject).ToArray(), side, xMul, yMul, zMul);
        }

        private static void Distribute(Transform[] transforms, float xMul, float yMul, float zMul)
        {
            GameObjectUtils.Distribute(transforms.Select(t => t.gameObject).ToArray(), xMul, yMul, zMul);
        }

        public override void Draw()
        {
            Transform[] transforms = TransformEditorWindow.GetTransforms();
            if (transforms.Length <= 1) return;

            Color color = GUI.color;
            Color targetColor = Color.white;
            float f = 0.8f;
            GUI.color = Color.Lerp(Color.red, targetColor, f);
            
            Texture leftAlignImage = Styles.isProSkin ? EditorIconContents.alignHorizontallyLeftActive.image : EditorIconContents.alignHorizontallyLeft.image;
            Texture centerAlignImage = Styles.isProSkin ? EditorIconContents.alignHorizontallyCenterActive.image : EditorIconContents.alignHorizontallyCenter.image;
            Texture rightAlignImage = Styles.isProSkin ? EditorIconContents.alignHorizontallyRightActive.image : EditorIconContents.alignHorizontallyRight.image;
            GUILayoutOption widthOption = GUILayout.Width(30);

            GUILayout.BeginHorizontal();

            
            if (GUILayout.Button(TempContent.Get(leftAlignImage, "Align X Min"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 0, 1, 0, 0);
            }

            if (GUILayout.Button(TempContent.Get(centerAlignImage, "Align X Center"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 1, 1, 0, 0);
            }

            if (GUILayout.Button(TempContent.Get(rightAlignImage, "Align X Right"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 2, 1, 0, 0);
            }

            if (GUILayout.Button(TempContent.Get("X", "Distribute X"), EditorStyles.toolbarButton, widthOption))
            {
                Distribute(transforms, 1, 0, 0);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUI.color = Color.Lerp(Color.green, targetColor, f);
            if (GUILayout.Button(TempContent.Get(leftAlignImage, "Align Y Min"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 0, 0, 1, 0);
            }

            if (GUILayout.Button(TempContent.Get(centerAlignImage, "Align Y Center"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 1, 0, 1, 0);
            }

            if (GUILayout.Button(TempContent.Get(rightAlignImage, "Align Y Right"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 2, 0, 1, 0);
            }

            if (GUILayout.Button(TempContent.Get("Y", "Distribute Y"), EditorStyles.toolbarButton, widthOption))
            {
                Distribute(transforms, 0, 1, 0);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUI.color = Color.Lerp(Color.blue, targetColor, f);
            if (GUILayout.Button(TempContent.Get(leftAlignImage, "Align Z Min"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 0, 0, 0, 1);
            }

            if (GUILayout.Button(TempContent.Get(centerAlignImage, "Align Z Center"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 1, 0, 0, 1);
            }

            if (GUILayout.Button(TempContent.Get(rightAlignImage, "Align Z Right"), EditorStyles.toolbarButton, widthOption))
            {
                Align(transforms, 2, 0, 0, 1);
            }

            if (GUILayout.Button(TempContent.Get("Z", "Distribute Z"), EditorStyles.toolbarButton, widthOption))
            {
                Distribute(transforms, 0, 0, 1);
            }
            GUILayout.EndHorizontal();

            GUI.color = color;
        }

        public override void Init()
        {
            _content = new GUIContent(Styles.isProSkin? Icons.align: Icons.alignDark, "Align & Distribute");
        }

        public override bool Validate()
        {
            if (Selection.gameObjects.Count(g => g.scene.name != null) < 2) return false;
            if (!Interceptor.isAppleM && Prefs.transformAlignDistribute) return false;
            return true;
        }
    }
}