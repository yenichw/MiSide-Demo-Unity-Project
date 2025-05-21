/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    [InitializeOnLoad]
    [EditorTool("Duplicate Tool")]
    public class DuplicateTool : EditorTool
    {
        public static int phase = 0;
        private static Vector3 position;
        private static Quaternion rotation;

        private static GUIContent labelContent;
        private static GUIContent passiveContent;
        private static GUIContent activeContent;
        private static GUIStyle style;
        private static Transform container;

        private static List<GameObject> tempItems;
        private static Vector3 startPosition;
        private static GameObject[] sourceGameObjects;
        private static Bounds sourceBounds;
        private static Vector3Int count = Vector3Int.zero;
        private static Vector3 sourceSize;
        private static int lastPositionIDX;
        private static Transform parent;
        private static Vector3[] eightPoints = new Vector3[8];
        private static Vector3[] fourCorners = new Vector3[4];

        public override GUIContent toolbarIcon
        {
            get
            {
#if UNITY_2020_2_OR_NEWER
                if (ToolManager.IsActiveTool(this))
#else
                if (EditorTools.IsActiveTool(this))
#endif
                {
                    if (activeContent == null) activeContent = new GUIContent(Icons.duplicateToolActive, "Duplicate Tool");
                    return activeContent;
                }

                if (passiveContent == null) passiveContent = new GUIContent(Icons.duplicateTool, "Duplicate Tool");
                return passiveContent;
            }
        }

        static DuplicateTool()
        {
            SceneViewManager.AddListener(OnSceneGUI, SceneViewOrder.Late, true);
            tempItems = new List<GameObject>();
        }

        private Bounds GetBounds()
        {
            Bounds bounds = new Bounds();

            bool isFirst = true;
            Quaternion ir = Quaternion.Inverse(rotation);

            for (int i = 0; i < sourceGameObjects.Length; i++)
            {
                GameObject go = sourceGameObjects[i];
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0)
                {
                    foreach (Renderer r in renderers)
                    {
                        Transform t = r.transform;
                        Bounds b = r.bounds;
#if UNITY_2021_2_OR_NEWER
                        Bounds lb = r.localBounds;
#else
                        Quaternion tRotation = t.rotation;
                        t.rotation = Quaternion.identity;
                        Bounds lb = r.bounds;
                        t.rotation = tRotation;
                        Vector3 extents = lb.extents;
                        Vector3 ls = t.lossyScale;
                        lb.extents = new Vector3(extents.x / ls.x, extents.y / ls.y, extents.z / ls.z);
#endif
                        Matrix4x4 matrix = t.localToWorldMatrix;
                        GetBoundPoints(lb, eightPoints, matrix.lossyScale);
                        RotateBoundPoints(eightPoints, matrix.rotation);
                        RotateBoundPoints(eightPoints, ir);

                        if (isFirst)
                        {
                            bounds = new Bounds(ir * b.center + eightPoints[0], Vector3.zero);
                            isFirst = false;
                        }

                        for (int j = 0; j < 8; j++)
                        {
                            bounds.Encapsulate(ir * b.center + eightPoints[j]);
                        }
                    }
                }

                RectTransform[] rectTransforms = go.GetComponentsInChildren<RectTransform>();

                if (rectTransforms.Length > 0)
                {
                    foreach (RectTransform rt in rectTransforms)
                    {
                        rt.GetWorldCorners(fourCorners);

                        if (isFirst)
                        {
                            bounds = new Bounds(ir * fourCorners[0], Vector3.zero);
                            isFirst = false;
                        }

                        for (int j = 0; j < 4; j++)
                        {
                            bounds.Encapsulate(ir * fourCorners[j]);
                        }
                    }
                }
            }

            return bounds;
        }

        private void GetBoundPoints(Bounds bounds, Vector3[] points, Vector3 scale)
        {
            Vector3 extents = bounds.extents;
            extents.Scale(scale);
            points[0] = new Vector3(extents.x, extents.y, extents.z);
            points[1] = new Vector3(extents.x, extents.y, -extents.z);
            points[2] = new Vector3(extents.x, -extents.y, extents.z);
            points[3] = new Vector3(extents.x, -extents.y, -extents.z);
            points[4] = new Vector3(-extents.x, extents.y, extents.z);
            points[5] = new Vector3(-extents.x, extents.y, -extents.z);
            points[6] = new Vector3(-extents.x, -extents.y, extents.z);
            points[7] = new Vector3(-extents.x, -extents.y, -extents.z);
        }

        private void Finish()
        {
            if (tempItems.Count > 0)
            {
                Undo.SetCurrentGroupName("Duplicate GameObjects");
                int group = Undo.GetCurrentGroup();

                List<GameObject> gameObjects = Selection.gameObjects.ToList();

                foreach (GameObject item in tempItems)
                {
                    Transform t = item.transform;
                    while (t.childCount > 0)
                    {
                        Transform child = t.GetChild(0);
                        Vector3 pos = child.position;
                        Quaternion rot = child.rotation;
                        child.SetParent(parent != null? parent: t.parent.parent, false);
                        child.position = pos;
                        child.rotation = rot;
                        child.hideFlags = HideFlags.None;
                        gameObjects.Add(child.gameObject);
                        Undo.RegisterCreatedObjectUndo(child.gameObject, "Duplicate GameObject");
                    }

                    DestroyImmediate(item);
                }

                tempItems.Clear();

                Selection.objects = gameObjects.ToArray();
                Undo.CollapseUndoOperations(group);
            }

            Reset();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (labelContent == null) return;

            Handles.BeginGUI();

            if (style == null)
            {
                style = new GUIStyle(ToolValues.StyleID)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    fixedHeight = 0,
                    border = new RectOffset(8, 8, 8, 8)
                };
            }

            Vector3 screenPoint = sceneView.camera.WorldToScreenPoint(position);
            Vector2 size = style.CalcSize(labelContent);
            if (screenPoint.y > size.y + 150) screenPoint.y -= size.y + 50;
            else screenPoint.y += size.y + 150;

            Rect rect = new Rect(screenPoint.x - size.x / 2, Screen.height - screenPoint.y - size.y / 2, size.x, size.y);

            GUI.Label(rect, labelContent, style);
            Handles.EndGUI();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!SelectionBoundsManager.hasBounds)
            {
                Reset();
                return;
            }

            if (phase == 0)
            {
                EditorGUI.BeginChangeCheck();

                position = UnityEditor.Tools.handlePosition;
                rotation = UnityEditor.Tools.handleRotation;
                PositionHandle.Ids ids = PositionHandle.Ids.@default;
                Vector3 newPosition = PositionHandle.DoPositionHandle(ids, position, rotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Waila.Close();
                    Vector3 delta = newPosition - position;
                    if (Math.Abs(delta.sqrMagnitude) < float.Epsilon) return;

                    sourceGameObjects = Selection.gameObjects;
                    Transform firstTransform = sourceGameObjects[0].transform;
                    sourceBounds = new Bounds(firstTransform.position, Vector3.zero);
                    sourceSize = GetBounds().size;

                    parent = firstTransform.parent;

                    for (int i = 1; i < sourceGameObjects.Length; i++)
                    {
                        Transform transform = sourceGameObjects[i].transform;
                        sourceBounds.Encapsulate(transform.position);
                        if (transform.parent != parent) parent = null;
                    }

                    startPosition = Quaternion.Inverse(rotation) * newPosition;
                    phase = 1;
                }

                position = newPosition;
            }
            else if (phase == 1)
            {
                EditorGUI.BeginChangeCheck();

                PositionHandle.Ids ids = PositionHandle.Ids.@default;
                if (lastPositionIDX == 0)
                {
                    lastPositionIDX = ids.x;
                }
                else if (ids.x != lastPositionIDX)
                {
                    int delta = ids.x - lastPositionIDX;
                    lastPositionIDX = ids.x;
                    GUIUtility.hotControl += delta;
                }
                position = PositionHandle.DoPositionHandle(ids, position, rotation);
                var p1 = Quaternion.Inverse(rotation) * position;

                Vector3 center = (startPosition + p1) / 2;
                Vector3 size = startPosition - p1;

                if (EditorGUI.EndChangeCheck() && GUIUtility.hotControl != 0) UpdatePreviews(size);

                if (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    Reset();
                    return;
                }

                if (GUIUtility.hotControl == 0)
                {
                    Finish();
                }
                else
                {
                    Handles.color = Color.red;
                    Matrix4x4 matrix = Handles.matrix;
                    Handles.matrix = Matrix4x4.TRS(matrix.GetColumn(3), rotation, matrix.lossyScale);
                    Handles.DrawWireCube(center, size);
                    Handles.matrix = matrix;
                }
            }

            Vector3 handlePos = position + rotation * new Vector3(-.1f, -.1f, -.1f) * HandleUtility.GetHandleSize(position);
            Handles.Label(handlePos, Icons.duplicate);
        }

        private void Reset()
        {
            foreach (GameObject item in tempItems) DestroyImmediate(item);
            tempItems.Clear();
            
            if (container != null)
            {
                DestroyImmediate(container.gameObject);
                container = null;
            }
            
            sourceGameObjects = null;
            labelContent = null;
            phase = 0;
            lastPositionIDX = 0;
        }

        private void RotateBoundPoints(Vector3[] points, Quaternion _rotation)
        {
            for (int i = 0; i < 8; i++) points[i] = _rotation * points[i];
        }

        private void UpdatePreviews(Vector3 size)
        {
            bool snapEnabled = SnapHelper.enabled;
            if (snapEnabled)
            {
                float snapValue = SnapHelper.value;

                if (sourceSize.x < snapValue) sourceSize.x = snapValue;
                else sourceSize.x = Mathf.RoundToInt(sourceSize.x / snapValue) * snapValue;

                if (sourceSize.y < snapValue) sourceSize.y = snapValue;
                else sourceSize.y = Mathf.RoundToInt(sourceSize.y / snapValue) * snapValue;

                if (sourceSize.z < snapValue) sourceSize.z = snapValue;
                else sourceSize.z = Mathf.RoundToInt(sourceSize.z / snapValue) * snapValue;
            }

            bool isRectTransform = SelectionBoundsManager.isRectTransform;
            if (container == null)
            {
                GameObject containerGO = new GameObject("Duplicate Tool Preview");
                container = containerGO.transform;
                if (isRectTransform)
                {
                    container.SetParent(SelectionBoundsManager.firstGameObject.transform.parent, false);
                    container = containerGO.AddComponent<RectTransform>();
                }
            }

            count.x = sourceSize.x > 0? Mathf.RoundToInt(Mathf.Abs(size.x / sourceSize.x)): 0;
            count.y = sourceSize.y > 0? Mathf.RoundToInt(Mathf.Abs(size.y / sourceSize.y)): 0;
            count.z = !isRectTransform && sourceSize.z > 0? Mathf.RoundToInt(Mathf.Abs(size.z / sourceSize.z)): 0;

            int countItems = (count.x + 1) * (count.y + 1) * (count.z + 1) - 1;
            if (countItems > Prefs.duplicateToolMaxCopies) return;
            if (tempItems.Count == countItems) return;

            StringBuilder builder = StaticStringBuilder.Start();
            builder.Append(count.x + 1).Append("x").Append(count.y + 1).Append("x").Append(count.z + 1);
            if (labelContent == null) labelContent = new GUIContent(builder.ToString());
            else labelContent.text = builder.ToString();

            int i = 0;

            float dx = Mathf.Sign(-size.x);
            float dy = Mathf.Sign(-size.y);
            float dz = Mathf.Sign(-size.z);

            while (tempItems.Count > countItems)
            {
                int removeIndex = tempItems.Count - 1;
                GameObject item = tempItems[removeIndex];
                DestroyImmediate(item);
                tempItems.RemoveAt(removeIndex);
            }

            Vector3 sourcePos = sourceBounds.min;
            Vector3 axis = ((Vector3)count).normalized;

            for (int x = 0; x <= count.x; x++)
            {
                for (int y = 0; y <= count.y; y++)
                {
                    for (int z = 0; z <= count.z; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) continue;

                        GameObject item;
                        if (i < tempItems.Count) item = tempItems[i];
                        else
                        {
                            item = new GameObject("Duplicate Tool Preview");
                            
                            item.transform.SetParent(container, false);
                            if (isRectTransform)
                            {
                                item.AddComponent<RectTransform>();
                            }

                            item.transform.position = sourcePos;

                            foreach (GameObject so in sourceGameObjects)
                            {
                                GameObject dup;
                                if (PrefabUtility.IsPartOfAnyPrefab(so))
                                {
                                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(so);
                                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                    dup = PrefabUtility.InstantiatePrefab(prefab, item.transform) as GameObject;
                                }
                                else
                                {
                                    dup = Instantiate(so, item.transform, false);
                                }
                                dup.name = so.name;
                                dup.transform.position = so.transform.position;
                                dup.transform.rotation = so.transform.rotation;
                                dup.transform.localScale = so.transform.localScale;
                            }
                            tempItems.Add(item);
                        }

                        item.transform.position = sourcePos + rotation * new Vector3(sourceSize.x * x * dx, sourceSize.y * y * dy, sourceSize.z * z * dz);
                        if (snapEnabled)
                        {
                            Vector3 pos = item.transform.position;
                            SnapHelper.Snap(item.transform);
                            Vector3 offset = item.transform.position - pos;
                            offset.Scale(axis);
                            item.transform.position = pos + offset;
                        }
                        
                        i++;
                    }
                }
            }
        }
    }
}