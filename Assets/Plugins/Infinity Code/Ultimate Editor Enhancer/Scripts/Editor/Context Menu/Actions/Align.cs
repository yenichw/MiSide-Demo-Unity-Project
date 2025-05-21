/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    [RequireMultipleGameObjects]
    public class Align : ActionItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -900; }
        }

        private static void AlignSelection(int side, float x, float y, float z)
        {
            GameObjectUtils.Align(Selection.gameObjects, side, x, y, z);
            EditorMenu.Close();
        }

        private static void Distribute(float x, float y, float z)
        {
            GameObjectUtils.Distribute(Selection.gameObjects, x, y, z);
            EditorMenu.Close();
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.align, "Align & Distribute");
        }

        public override void Invoke()
        {
            GenericMenuEx menu = GenericMenuEx.Start();

            menu.Add("Align/X/Min", () => AlignSelection(0, 1, 0, 0));
            menu.Add("Align/X/Center", () => AlignSelection(1, 1, 0, 0));
            menu.Add("Align/X/Max", () => AlignSelection(2, 1, 0, 0));

            menu.Add("Align/Y/Min", () => AlignSelection(0, 0, 1, 0));
            menu.Add("Align/Y/Center", () => AlignSelection(1, 0, 1, 0));
            menu.Add("Align/Y/Max", () => AlignSelection(2, 0, 1, 0));

            menu.Add("Align/Z/Min", () => AlignSelection(0, 0, 0, 1));
            menu.Add("Align/Z/Center", () => AlignSelection(1, 0, 0, 1));
            menu.Add("Align/Z/Max", () => AlignSelection(2, 0, 0, 1));

            if (Selection.gameObjects.Length > 2)
            {
                menu.Add("Distribute/X", () => Distribute(1, 0, 0));
                menu.Add("Distribute/Y", () => Distribute(0, 1, 0));
                menu.Add("Distribute/Z", () => Distribute(0, 0, 1));
            }

            menu.Show();
        }
    }
}