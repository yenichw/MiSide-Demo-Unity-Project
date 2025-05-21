/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class Waila
    {
        public const string StyleID = "sv_label_4";

        public static Action OnClose;
        public static Action<SceneViewItem> OnDrawModeExternal;
        public static Func<GameObject, string, string> OnPrepareTooltip;
        public static Func<SceneViewItem, bool> OnUpdateTooltipsExternal;

        private static GUIStyle _labelStyle;
        private static GUIStyle tooltipStyle;

        private static Dictionary<int, SceneViewItem> sceneViewItems = new Dictionary<int, SceneViewItem>();
        private static SceneViewItem restoreItem;


        public static GUIStyle labelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(StyleID);
                    _labelStyle.fontSize = 10;
                    _labelStyle.normal.background = null;
                    _labelStyle.padding = new RectOffset(0, 4, 2, 0);
                    _labelStyle.margin = new RectOffset(0, 0, 0, 2);
                    _labelStyle.alignment = TextAnchor.MiddleLeft;
                    _labelStyle.wordWrap = false;
                }

                return _labelStyle;
            }
        }

        static Waila()
        {
            SceneViewManager.AddListener(OnSceneGUI, SceneViewOrder.Waila, true);
        }

        public static void Close()
        {
            foreach (KeyValuePair<int, SceneViewItem> pair in sceneViewItems)
            {
                pair.Value.Close();
            }

            if (OnClose != null) OnClose();
        }

        private static void DrawTooltip(SceneViewItem sceneViewItem)
        {
            if (tooltipStyle == null)
            {
                tooltipStyle = new GUIStyle(StyleID);
                tooltipStyle.fontSize = 10;
                tooltipStyle.stretchHeight = true;
                tooltipStyle.fixedHeight = 0;
                tooltipStyle.border = new RectOffset(8, 8, 8, 8);
                tooltipStyle.margin = new RectOffset(0, 0, 0, 0);
                tooltipStyle.padding = new RectOffset(8, 8, 8, 8);
                tooltipStyle.alignment = TextAnchor.MiddleLeft;
            }
            Vector2 size = tooltipStyle.CalcSize(sceneViewItem.tooltip);
            Vector2 position = Event.current.mousePosition - new Vector2(size.x / 2, size.y + 10);
            Rect rect = new Rect(position, size + new Vector2(4, 0));

            Handles.BeginGUI();
            GUI.Label(rect, sceneViewItem.tooltip, tooltipStyle);
            Handles.EndGUI();
        }

        public static SceneViewItem GetSceneViewItem(SceneView view)
        {
            if (view == null) return null;
            
            SceneViewItem r;
            return sceneViewItems.TryGetValue(view.GetInstanceID(), out r) ? r : null;
        }

        public static void Highlight(GameObject go)
        {
            if (!Prefs.highlight || !Prefs.highlightOnWaila) return;

            EditorWindow wnd = WindowsHelper.mouseOverWindow;
            if (wnd == null || !(wnd is SceneView)) return;

            if (Highlighter.Highlight(go))
            {
                Highlighter.RepaintAllHierarchies();
            }
        }

        private static void InsertTerrainHeight(TerrainCollider collider, ref string tooltipText)
        {
            if (collider == null) return;

            RaycastHit hit;
            if (collider.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit, float.PositiveInfinity))
            {
                tooltipText += "\nWorld Position: " + hit.point.ToString("F2");
            }
        }

        private static void OnSceneGUI(SceneView view)
        {
            SceneViewItem sceneViewItem = null;
            sceneViewItems.TryGetValue(view.GetInstanceID(), out sceneViewItem);

            if (sceneViewItem != null && WindowsHelper.mouseOverWindow != view)
            {
                if (sceneViewItem.mode == 0)
                {
                    if (sceneViewItem.tooltip != null)
                    {
                        sceneViewItem.tooltip = null;
                        Highlight(null);
                    }
                    return;
                }
            }
            if (!Prefs.waila) return;

            if (sceneViewItem == null)
            {
                sceneViewItem = new SceneViewItem();
                sceneViewItems.Add(view.GetInstanceID(), sceneViewItem);
            }

            Event e = Event.current;

            if (Preview.isActive || InputManager.GetAnyMouseButton() && sceneViewItem.tooltip == null)
            {
                if (sceneViewItem.mode == 0)
                {
                    if (sceneViewItem.tooltip != null)
                    {
                        sceneViewItem.tooltip = null;
                        sceneViewItem.targets.Clear();
                        Highlight(null);
                    }
                    return;
                }
            }

            if (sceneViewItem.mode == 0)
            {
                if (e.type == EventType.MouseMove || e.type == EventType.KeyUp || e.type == EventType.KeyDown)
                {
                    UpdateTarget(sceneViewItem);
                }

                if (sceneViewItem.tooltip != null)
                {
                    DrawTooltip(sceneViewItem);

                    if (e.type == EventType.MouseDown && e.modifiers == Prefs.wailaShowNameUnderCursorModifiers && GUIUtility.hotControl != 0)
                    {
                        Selection.activeGameObject = sceneViewItem.targets[0];
                        restoreItem = sceneViewItem;
                        SceneViewManager.AddListener(RestoreSelection);
                        e.Use();
                    }
                }
            }
            else if (OnDrawModeExternal != null) OnDrawModeExternal(sceneViewItem);
        }

        private static void RestoreSelection(SceneView view)
        {
            if (restoreItem == null) return;

            if (restoreItem.targets == null || restoreItem.targets.Count == 0)
            {
                SceneViewManager.RemoveListener(RestoreSelection);
                return;
            }

            if (Selection.gameObjects.Length != 1 || Selection.gameObjects[0] != restoreItem.targets[0])
            {
                SceneViewManager.RemoveListener(RestoreSelection);
                Selection.activeGameObject = restoreItem.targets[0];
            }
        }

        private static void UpdateTarget(SceneViewItem sceneViewItem)
        {
            if (OnUpdateTooltipsExternal != null && OnUpdateTooltipsExternal(sceneViewItem))
            {
            }
            else if (Prefs.wailaShowNameUnderCursor && Event.current.modifiers == Prefs.wailaShowNameUnderCursorModifiers)
            {
                UpdateTooltip(sceneViewItem);
            }
            else
            {
                sceneViewItem.tooltip = null;
                sceneViewItem.targets.Clear();
                Highlight(null);
            }
        }

        private static void UpdateTooltip(SceneViewItem sceneViewItem)
        {
            Vector2 mousePosition = Event.current.mousePosition;

            GameObject go = HandleUtility.PickGameObject(mousePosition, false, null);
            if (go == null)
            {
                Highlight(null);
                sceneViewItem.tooltip = null;
                sceneViewItem.targets.Clear();
                return;
            }

            if (go.GetComponent<Canvas>() != null || go.GetComponent<CanvasRenderer>() != null)
            {
                List<GameObject> found = new List<GameObject>(20) {go};

                while (found.Count < 20)
                {
                    go = HandleUtility.PickGameObject(mousePosition, false, found.ToArray());
                    if (go == null) break;
                    bool isInsert = false;
                    
                    Transform t = go.transform;

                    for (int i = 0; i < found.Count; i++)
                    {
                        Transform ft = found[i].transform;

                        if (ft.parent == t)
                        {
                            found.Insert(i, go);
                            isInsert = true;
                            break;
                        }
                        
                        if (ft.parent == t.parent)
                        {
                            if (ft.GetSiblingIndex() > t.GetSiblingIndex())
                            {
                                found.Insert(i, go);
                                isInsert = true;
                                break;
                            }
                        }
                    }
                    if (!isInsert) found.Add(go);
                }

                go = found[found.Count - 1];
            }

            if (PrefabUtility.IsPartOfAnyPrefab(go)) go = PrefabUtility.GetNearestPrefabInstanceRoot(go);

            Highlight(go);

            if (sceneViewItem.targets.Count != 1 || sceneViewItem.targets[0] != go)
            {
                sceneViewItem.targets.Clear();
                sceneViewItem.targets.Add(go);
            }

            string tooltipText = go.name;
            TerrainCollider terrainCollider = sceneViewItem.targets[0].GetComponent<TerrainCollider>();

            if (terrainCollider != null) InsertTerrainHeight(terrainCollider, ref tooltipText);

            if (OnPrepareTooltip != null)
            {
                tooltipText = OnPrepareTooltip(go, tooltipText);
            }

            sceneViewItem.tooltip = new GUIContent(tooltipText);
        }

        public class SceneViewItem
        {
            public List<GameObject> targets;
            public int mode; // 0 - tooltip, 1 - smart selection
            public GUIContent tooltip;

            public SceneViewItem()
            {
                targets = new List<GameObject>();
            }

            public void Close()
            {
                mode = 0;
                tooltip = null;
            }
        }
    }
}