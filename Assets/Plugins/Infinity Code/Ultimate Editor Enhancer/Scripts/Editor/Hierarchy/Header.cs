/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class Header
    {
        static Header()
        {
            HierarchyItemDrawer.Register("Header", OnHierarchyItem, HierarchyToolOrder.Header);
        }

        public static JsonArray json
        {
            get
            {
                JsonArray jArr = new JsonArray();
                for (int i = 0; i < ReferenceManager.headerRules.Count; i++)
                {
                    jArr.Add(ReferenceManager.headerRules[i].json);
                }

                return jArr;
            }
            set
            {
                if (ReferenceManager.headerRules.Count > 0)
                {
                    if (!EditorUtility.DisplayDialog("Import Hierarchy Headers", "Hierarchy Headers already contain items", "Replace", "Ignore")) return;
                }

                List<HeaderRule> items = value.Deserialize<List<HeaderRule>>();
                ReferenceManager.headerRules.Clear();
                ReferenceManager.headerRules.AddRange(items);
            }
        }

        [MenuItem("GameObject/Create Header", priority = 1)]
        public static GameObject Create()
        {
            GameObject go = new GameObject(Prefs.hierarchyHeaderPrefix + "Header");
            go.tag = "EditorOnly";
            GameObject active = Selection.activeGameObject;
            if (active != null)
            {
                go.transform.SetParent(active.transform.parent);
                go.transform.SetSiblingIndex(active.transform.GetSiblingIndex());
            }
            Undo.RegisterCreatedObjectUndo(go, go.name);
            Selection.activeGameObject = go;
            return go;
        }

        private static void OnHierarchyItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyHeaders) return;

            GameObject go = item.gameObject;
            if (go == null) return;

            List<HeaderRule> rules = ReferenceManager.headerRules;
            HeaderRule rule = null;

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].Validate(go))
                {
                    rule = rules[i];
                    break;
                }
            }

            if (rule == null) return;

            if (Event.current.type == EventType.Repaint)
            {
                int textPadding = 8;

                bool hasChildren = item.gameObject.transform.childCount > 0;
                if (hasChildren) textPadding = (int)item.rect.x - 30;

                rule.Draw(item, textPadding);

                if (hasChildren)
                {
                    bool isExpanded = HierarchyHelper.IsExpanded(item.id);
                    Rect r = new Rect(item.rect);
                    r.width = 16;
                    r.x -= 14;
                    EditorStyles.foldout.Draw(r, GUIContent.none, -1, isExpanded);
                }
            }

            HierarchyItemDrawer.StopCurrentRowGUI();
        }
    }
}