/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class Ungroup
    {
        static Ungroup()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Selection.gameObjects.Length > 0;
            binding.OnPress += OnInvoke;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (e.keyCode != Prefs.ungroupKeyCode || e.modifiers != Prefs.ungroupModifiers) return;

            if (EditorUtility.DisplayDialog("Ungroup", "Ungroup selected GameObjects?", "Yes", "Cancel"))
            {
                UngroupSelection();
            }
        }

        [MenuItem("Edit/Ungroup", false, 120)]
        [MenuItem("GameObject/Ungroup", false, 0)]
        public static void UngroupSelection()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;
            UngroupTargets(Selection.gameObjects);
        }

        public static void UngroupTargets(params GameObject[] targets)
        {
            Undo.SetCurrentGroupName("Ungroup GameObjects");
            int group = Undo.GetCurrentGroup();

            List<GameObject> newSelection = new List<GameObject>();

            GameObject[] selections = targets;

            for (int i = 0; i < selections.Length; i++)
            {
                GameObject go = selections[i];
                bool isPartOfPrefab = PrefabUtility.IsPartOfAnyPrefab(go);
                if (isPartOfPrefab)
                {
                    GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                    go = selections[i];
                }

                Transform t = go.transform;
                while (t.childCount != 0)
                {
                    Transform ct = t.GetChild(0);
                    Undo.SetTransformParent(ct, t.parent, "Update Parent");
                    newSelection.Add(ct.gameObject);
                }
                Undo.DestroyObjectImmediate(go);
            }

            Selection.objects = newSelection.ToArray();

            Undo.CollapseUndoOperations(group);
        }

        [MenuItem("GameObject/Ungroup", true)]
        [MenuItem("Edit/Ungroup", true)]
        public static bool ValidateUngroup()
        {
            return Selection.gameObjects.Length > 0;
        }
    }
}