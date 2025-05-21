/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class Group
    {
        public const string CollectionTag = "Collection";
        private const int DialogHeight = 105;

        private static string defaultName = "Group";
        private static GameObject[] _targets;
        private static InputDialog dialog;
        private static bool createCollection = false;
        private static int align = 0;
        private static Transform parent;

        private static GUIContent[] singleAlign = 
        {
            new GUIContent("Original Position"), 
            new GUIContent("Center Position"), 
            new GUIContent("Zero Local Position")
        };
        private static int[] singleAlignValues = {2, 0, 1};
        private static GUIContent[] multipleAlign =
        {
            new GUIContent("Center Position"), 
            new GUIContent("Zero Local Position")
        };
        private static int[] multipleAlignValues = { 0, 1};
        private static GUIContent[] alignContents;
        private static int[] alignValues;

        static Group()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Selection.gameObjects.Length > 0;
            binding.OnPress += OnInvoke;

            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += OnClosePopups;
        }

        [MenuItem("GameObject/Create Empty Collection", priority = 1)]
        public static GameObject Create()
        {
            GameObject go = new GameObject("Collection");
            GameObjectUtils.SetCustomTag(go, CollectionTag);
            GameObject active = Selection.activeGameObject;
            if (active != null)
            {
                go.transform.SetParent(active.transform.parent);
                go.transform.SetSiblingIndex(active.transform.GetSiblingIndex());
            }

            go.AddComponent<FlattenCollection>();
            Undo.RegisterCreatedObjectUndo(go, go.name);
            Selection.activeGameObject = go;
            return go;
        }

        private static Transform FindParent(Transform p1, Transform p2)
        {
            if (p1 == null || p2 == null) return null;
            if (p1 == p2) return p1;

            List<Transform> rp1 = new List<Transform>{p1};
            List<Transform> rp2 = new List<Transform>{p2};

            Transform tp1 = p1;
            Transform tp2 = p2;

            while (tp1.parent != null)
            {
                tp1 = tp1.parent;
                rp1.Insert(0, tp1);
            }

            while (tp2.parent != null)
            {
                tp2 = tp2.parent;
                rp2.Insert(0, tp2);
            }

            Transform p = null;

            for (int i = 0; i < Mathf.Min(rp1.Count, rp2.Count); i++)
            {
                if (rp1[i] == rp2[i]) p = rp1[i];
                else break;
            }

            return p;
        }

        [MenuItem("Edit/Group", false, 120)]
        [MenuItem("GameObject/Group", false, 0)]
        public static void GroupSelection()
        {
            if (dialog == null) GroupTargets(Selection.gameObjects);
        }

        public static void GroupTargets(params GameObject[] targets)
        {
            if (targets.Length == 0) return;

            _targets = targets;

            alignContents = _targets.Length > 1 ? multipleAlign : singleAlign;
            alignValues = _targets.Length > 1 ? multipleAlignValues : singleAlignValues;

            createCollection = false;
            align = alignValues[0];

            parent = _targets[0].transform.parent;
            for (int i = 1; i < _targets.Length; i++) parent = FindParent(parent, _targets[i].transform.parent);

            string name = defaultName;
            if (targets.Length == 1) name = targets[0].name + " Container";

            dialog = InputDialog.Show("Enter name of GameObject", name, OnCreateGroup); 
            dialog.OnClose += OnDialogClose;
            dialog.OnDrawExtra += OnDialogExtra;
            dialog.OnDrawLeftButtons += OnDrawLeftButtons;
            dialog.minSize = new Vector2(dialog.minSize.x, DialogHeight);
        }

        private static void OnClosePopups(EventManager.EventBinding binding)
        {
            if (dialog == null) return;

            dialog.Close();
            dialog = null;
        }

        private static void OnCreateGroup(string name)
        {
            if (_targets == null || _targets.Length == 0) return;

            Undo.SetCurrentGroupName("Group GameObjects");
            int group = Undo.GetCurrentGroup();

            GameObject go = new GameObject(name);
            if (createCollection)
            {
                GameObjectUtils.SetCustomTag(go, CollectionTag);
                go.AddComponent<FlattenCollection>();
            }
            Undo.RegisterCreatedObjectUndo(go, go.name);

            if (parent != null) Undo.SetTransformParent(go.transform, parent, go.name);

            if (align == 0)
            {
                Bounds bounds = new Bounds();

                for (int i = 0; i < _targets.Length; i++)
                {
                    GameObject t = _targets[i];
                    Collider cl = t.GetComponent<Collider>();
                    Bounds b;
                    if (cl != null) b = cl.bounds;
                    else
                    {
                        Renderer r = t.GetComponent<Renderer>();
                        if (r != null) b = r.bounds;
                        else b = new Bounds(t.transform.position, Vector3.zero);
                    }

                    if (i == 0) bounds = b;
                    else bounds.Encapsulate(b);
                }

                go.transform.position = bounds.center;
            }
            else if (align == 1)
            {
                go.transform.localPosition = Vector3.zero;
            }
            else if (align == 2)
            {
                go.transform.position = _targets[0].transform.position;
            }

            bool addRectTransform = true;
            foreach (GameObject g in _targets)
            {
                if (addRectTransform && g.GetComponent<RectTransform>() == null) addRectTransform = false;
                Undo.SetTransformParent(g.transform, go.transform, g.name);
            }

            if (addRectTransform) go.AddComponent<RectTransform>();

            Selection.activeGameObject = go;

            Undo.CollapseUndoOperations(group);

            _targets = null;
        }

        private static void OnDialogClose(InputDialog dlg)
        {
            dialog = null;
        }

        private static void OnDialogExtra(InputDialog dlg)
        {
            try
            {
                parent = EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true) as Transform;
            }
            catch (ExitGUIException e)
            {
                throw e;
            }

            align = EditorGUILayout.IntPopup(align, alignContents, alignValues);

            createCollection = EditorGUILayout.Toggle("Create Collection", createCollection);
        }

        private static void OnDrawLeftButtons(InputDialog dlg)
        {
            if (GUILayout.Button("?", GUILayout.ExpandWidth(false)))
            {
                Links.OpenDocumentation("group-gameobjects");
            }
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (e.keyCode == Prefs.groupKeyCode && e.modifiers == Prefs.groupModifiers) GroupSelection();
        }

        [MenuItem("GameObject/Group", true)]
        [MenuItem("Edit/Group", true)]
        public static bool ValidateGroup()
        {
            return Selection.gameObjects.Length > 0;
        }
    }
}