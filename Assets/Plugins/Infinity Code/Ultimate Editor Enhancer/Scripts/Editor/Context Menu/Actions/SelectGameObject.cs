/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public class SelectGameObject : ActionItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -950; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.hierarchy, "Select GameObject");
        }

        public override void Invoke()
        {
            List<GameObject> items = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            GenericMenuEx menu = GenericMenuEx.Start();

            for (int i = 0; i < items.Count; i++) GetChildren(items[i].transform, menu, "");

            menu.Show();
        }

        private void GetChildren(Transform t, GenericMenuEx menu, string prefix)
        {
            string title = prefix + t.name;

            if (t.childCount > 0)
            {
                menu.Add(title + "/Select", Selection.activeGameObject == t.gameObject, () =>
                {
                    Selection.activeGameObject = t.gameObject;
                    SceneViewManager.OnNextGUI += EditorMenu.ShowInLastPosition;
                });
                menu.AddSeparator(title + "/");
                for (int i = 0; i < t.childCount; i++) GetChildren(t.GetChild(i), menu, title + "/");
            }
            else
            {
                menu.Add(title, Selection.activeGameObject == t.gameObject, () =>
                {
                    Selection.activeGameObject = t.gameObject;
                    SceneViewManager.OnNextGUI += EditorMenu.ShowInLastPosition;
                });
            }
        }
    }
}