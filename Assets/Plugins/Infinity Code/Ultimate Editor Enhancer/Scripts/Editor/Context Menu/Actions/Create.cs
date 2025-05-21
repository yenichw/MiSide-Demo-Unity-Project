/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public class Create : ActionItem
    {
        private int type; // 0 - Root, 1 - Child, 2 - Sibling, 3 - Parent, 4 - Temporary

        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -1000; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.createObject, "Create Object");
        }

        private void ApplyType()
        {
            GameObject active = Selection.activeGameObject;

            CheckDuplicateAudioListeners(active);

            if (targets == null || targets.Length == 0) return;
            
            GameObject go = targets[0];
            if (go == null || active == go) return;

            Transform t = go.transform;
            Transform at = active.transform;

            if (type == 1)
            {
                Undo.SetTransformParent(at, t, "Set Parent");
                Undo.RecordObject(at, "Before Change Position");
                at.localPosition = Vector3.zero;
                at.localRotation = Quaternion.identity;
                at.localScale = Vector3.one;
            }
            else if (type == 2)
            {
                Undo.SetTransformParent(at, t.parent, "Set Parent");
                Undo.RecordObject(at, "Before Change Position");
                at.SetParent(t.parent, false);
                at.SetSiblingIndex(t.GetSiblingIndex() + 1);
                at.localPosition = Vector3.zero;
                at.localRotation = Quaternion.identity;
                at.localScale = Vector3.one;
            }
            else if (type == 3)
            {
                Transform parent = t.parent;
                if (parent != null)
                {
                    Undo.SetTransformParent(at, parent, "Set Parent");
                }

                Undo.RecordObject(at, "Before Change Position");
                at.position = t.position;
                Undo.SetTransformParent(t, at, "Set Parent");
            }
            else if (type == 4)
            {
                GameObject parent = TemporaryContainer.GetContainer();
                if (parent == null || active == null || active == parent) return;

                Undo.SetTransformParent(at, parent.transform, "Set Parent");
                active.tag = "EditorOnly";
            }
        }

        private static void CheckDuplicateAudioListeners(GameObject target)
        {
            if (target == null) return;
            if (target.GetComponent<Camera>() == null) return;
            if (ObjectHelper.FindObjectsOfType<AudioListener>().Length <= 1) return;

            AudioListener audioListener = target.GetComponent<AudioListener>();
            if (audioListener != null) Object.DestroyImmediate(audioListener);
        }

        public override void Invoke()
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Root", () => ShowCreateBrowser(0));
            if (targets.Length == 1 && targets[0] != null)
            {
                menu.Add("Child", () => ShowCreateBrowser(1));
                if (targets[0].transform.parent != null) menu.Add("Sibling", () => ShowCreateBrowser(2));
                menu.Add("Parent", () => ShowCreateBrowser(3));
            }
            menu.Add("Temporary", () => ShowCreateBrowser(4));
            menu.Show();
        }

        private void OnBrowserPrefab(string assetPath)
        {
            Undo.SetCurrentGroupName("Instantiate Prefab");
            int group = Undo.GetCurrentGroup();

            Selection.activeGameObject = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)) as GameObject;
            Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, "Instantiate");
            ApplyType();
            Undo.CollapseUndoOperations(group);
        }

        private void OnBrowserCreate(string menuItem)
        {
            Undo.SetCurrentGroupName("Create " + menuItem.Substring(11));
            int group = Undo.GetCurrentGroup();
            bool addRectTransform = false;
            if (type > 0 && type < 4 && menuItem == "GameObject/Create Empty")
            {
                addRectTransform = targets.All(t => t.GetComponent<RectTransform>() != null);
            }
            EditorApplication.ExecuteMenuItem(menuItem);
            ApplyType();

            if (addRectTransform)
            {
                Undo.AddComponent<RectTransform>(Selection.activeGameObject);
            }

            Undo.CollapseUndoOperations(group);
        }

        private void OnBrowserClose(CreateBrowser browser)
        {
            browser.OnClose -= OnBrowserClose;
            browser.OnSelectCreate -= OnBrowserCreate;
            browser.OnSelectPrefab -= OnBrowserPrefab;
        }

        private void ShowCreateBrowser(int type)
        {
            EditorMenu.Close();

            this.type = type;
            CreateBrowser browser = CreateBrowser.OpenWindow();
            browser.OnClose += OnBrowserClose;
            browser.OnSelectCreate += OnBrowserCreate;
            browser.OnSelectPrefab += OnBrowserPrefab;

            if (type == 1)
            {
                browser.createLabel = "Create Child Item";
                browser.prefabsLabel = "Instantiate Child Prefab";
            }
            else if (type == 2)
            {
                browser.createLabel = "Create Sibling Item";
                browser.prefabsLabel = "Instantiate Sibling Prefab";
            }
            else if (type == 3)
            {
                browser.createLabel = "Create Parent Item";
                browser.prefabsLabel = "Instantiate Parent Prefab";
            }
            else if (type == 4)
            {
                browser.createLabel = "Create Temporary Item";
                browser.prefabsLabel = "Instantiate Temporary Prefab";
            }
        }
    }
}