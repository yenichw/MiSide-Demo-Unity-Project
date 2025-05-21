/*           INFINITY CODE          */
/*     https://infinity-code.com    */

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class DragAndDropBehavior
    {
        private static string tooltip;
        private static bool needCleanUp;

        static DragAndDropBehavior()
        {
            SceneViewManager.AddListener(OnSceneGUI);
            SceneViewManager.AddListener(OnSceneGUILate, 0, true);
            SceneViewManager.AddListener(OnDragComponent);
        }

        private static void DrawTooltip(SceneView sceneView)
        {
            GUIContent content = TempContent.Get(tooltip);
            float pixelPerPoint = EditorGUIUtility.pixelsPerPoint;
            Vector2 size = Styles.tooltip.CalcSize(content);

            Handles.BeginGUI();
            Vector3 screenPoint = new Vector3(Event.current.mousePosition.x, sceneView.position.height - Event.current.mousePosition.y);
            if (screenPoint.y > 100 / pixelPerPoint)
            {
                screenPoint.y -= size.y + 0 / pixelPerPoint;
            }
            else
            {
                screenPoint.y += size.y + 20 / pixelPerPoint;
            }

            Rect rect = new Rect(screenPoint.x - size.x / 2, Screen.height / pixelPerPoint - screenPoint.y - size.y / 2, size.x, size.y);
            GUI.Label(rect, content, Styles.tooltip);

            Handles.EndGUI();
        }

        private static void OnDragComponent(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.modifiers != EventModifiers.Control && e.modifiers != EventModifiers.Command) return;
            if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;

            EditorWindow wnd = WindowsHelper.mouseOverWindow;

            if (wnd == null) return;
            if (!(wnd is SceneView)) return;
            if (DragAndDrop.objectReferences.Length != 1) return;
            if (!(DragAndDrop.objectReferences[0] is Component)) return;

            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                ComponentWindow.Show(DragAndDrop.objectReferences[0] as Component, false);

                e.Use();
            }
        }

        private static void OnDragGameObjectPerform()
        {
            GameObject go = DragAndDrop.objectReferences[0] as GameObject;
            if (go.scene.name != null || go.GetComponent<RectTransform>() == null) return;

            Event e = Event.current;
            GameObject target = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (target == null) return;
            RectTransform parent = target.GetComponent<RectTransform>();
            if (parent == null) return;

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                if (prefabStage.assetPath == DragAndDrop.paths[0]) return;
                if (e.alt)
                {
                    RectTransform p = prefabStage.prefabContentsRoot.transform as RectTransform;
                    parent = p != null ? p : GameObjectUtils.GetRoot(parent);
                }
            }
            else if (e.alt) parent = GameObjectUtils.GetRoot(parent);

            DragAndDrop.AcceptDrag();
            GameObject instance = PrefabUtility.InstantiatePrefab(go) as GameObject;
            Undo.RegisterCreatedObjectUndo(instance, "Drag Instance");
            instance.transform.SetParent(parent, false);
            Vector3 pos;
            SceneViewManager.GetWorldPosition(out pos);
            instance.transform.position = pos;
            e.Use();

        }

        private static void OnDragGameObjectUpdated()
        {
            GameObject go = DragAndDrop.objectReferences[0] as GameObject;
            if (go.GetComponent<RectTransform>() == null) return;

            Event e = Event.current;
            GameObject target = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (target == null || target.GetComponent<RectTransform>() == null) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            e.Use();
        }

        private static void OnDragPerform(SceneView view)
        {
            if (DragAndDrop.objectReferences.Length != 1) return;

            Object obj = DragAndDrop.objectReferences[0];
            if (obj is Texture) OnDragTexturePerform(view);
            else if (obj is Sprite) OnDragSpritePerform(view);
            else if (obj is GameObject) OnDragGameObjectPerform();
        }

        private static void OnDragSpritePerform(SceneView view)
        {
            Event e = Event.current;
            GameObject go = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (go == null) return;

            Image image = go.GetComponent<Image>();
            RectTransform rt = go.GetComponent<RectTransform>();
            if (image != null && e.modifiers == EventModifiers.None)
            {
                DragAndDrop.AcceptDrag();
                SetReferenceValue(image, "m_Sprite");
                e.Use();
            }
            else if (rt != null)
            {
                DragAndDrop.AcceptDrag();
                EditorApplication.ExecuteMenuItem("GameObject/UI/Image");
                GameObject newGO = Selection.activeGameObject;
                if (e.alt) rt = GameObjectUtils.GetRoot(rt);
                SetPosition(view, newGO, rt);

                SetReferenceValue(newGO.GetComponent<Image>(), "m_Sprite");
                e.Use();
            }
        }

        private static void OnDragSpriteUpdated(Object obj)
        {
            Event e = Event.current;
            GameObject go = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (go == null) return;

            if (go.GetComponent<Image>() != null && e.modifiers == EventModifiers.None)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                tooltip = "Set " + obj.name + " to " + go.name + "/Image.sprite";
                needCleanUp = true;
                e.Use();
            }
            else if (go.GetComponent<RectTransform>() != null)
            {
                tooltip = "Create " + obj.name + " Image in " + go.name;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                needCleanUp = true;
                e.Use();
            }
        }

        private static void OnDragTexturePerform(SceneView view)
        {
            Event e = Event.current;
            GameObject go = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (go == null) return;

            RectTransform rt = go.GetComponent<RectTransform>();

            if (e.modifiers == EventModifiers.None)
            {
                RawImage rawImage = go.GetComponent<RawImage>();
                if (rawImage != null)
                {
                    DragAndDrop.AcceptDrag();
                    SetReferenceValue(rawImage, "m_Texture");
                    e.Use();
                    return;
                }

                Image image = go.GetComponent<Image>();
                if (image != null)
                {
                    TextureImporter importer = AssetImporter.GetAtPath(DragAndDrop.paths[0]) as TextureImporter;
                    if (importer != null && importer.textureType == TextureImporterType.Sprite)
                    {
                        DragAndDrop.AcceptDrag();
                        SetReferenceValue(image, "m_Sprite", AssetDatabase.LoadAssetAtPath<Sprite>(DragAndDrop.paths[0]));
                        e.Use();
                        return;
                    }
                }
            }

            if (rt != null)
            {
                DragAndDrop.AcceptDrag();
                EditorApplication.ExecuteMenuItem("GameObject/UI/Raw Image");
                GameObject newGO = Selection.activeGameObject;
                if (e.alt) rt = GameObjectUtils.GetRoot(rt);
                SetPosition(view, newGO, rt);
                SetReferenceValue(newGO.GetComponent<RawImage>(), "m_Texture");
                e.Use();
            }
        }

        private static void OnDragTextureUpdated(Object obj)
        {
            Event e = Event.current;
            GameObject go = HandleUtility.PickGameObject(e.mousePosition, false, null);
            if (go == null) return;

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null && DragAndDrop.paths.Length > 0)
            {
                TextureImporter importer = AssetImporter.GetAtPath(DragAndDrop.paths[0]) as TextureImporter;
                if (importer != null && importer.textureType == TextureImporterType.Sprite) needCleanUp = true;
            }

            if (e.modifiers == EventModifiers.None)
            {
                if (go.GetComponent<RawImage>() != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    tooltip = "Set " + obj.name + " to " + go.name + "/RawImage.texture";

                    e.Use();
                    return;
                }

                if (go.GetComponent<Image>())
                {
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        tooltip = "Set " + obj.name + " to " + go.name + "/Image.sprite";
                        e.Use();
                        return;
                    }
                }
            }

            if (rectTransform != null)
            {
                tooltip = "Create " + obj.name + " RawImage in " + go.name;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }
        }

        private static void OnDragUpdated()
        {
            if (DragAndDrop.objectReferences.Length != 1) return;

            Object obj = DragAndDrop.objectReferences[0];
            if (obj is Texture) OnDragTextureUpdated(obj);
            else if (obj is Sprite) OnDragSpriteUpdated(obj);
            else if (obj is GameObject) OnDragGameObjectUpdated();
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!Prefs.improveDragAndDropBehaviour) return;

            needCleanUp = false;

            Event e = Event.current;
            if (e.type == EventType.DragPerform)
            {
                tooltip = null;
                OnDragPerform(view);
            }
            else if (e.type == EventType.DragUpdated)
            {
                tooltip = null;
                OnDragUpdated();
            }
        }

        private static void OnSceneGUILate(SceneView view)
        {
            if (needCleanUp)
            {
                SpriteUtilityRef.CleanUp(true);
                needCleanUp = false;
            }

            if (Event.current.type == EventType.Repaint && !string.IsNullOrEmpty(tooltip))
            {
                if (DragAndDrop.objectReferences.Length == 0) tooltip = null;
                else DrawTooltip(view);
            }
        }

        private static void SetPosition(SceneView view, GameObject newGO, RectTransform rt)
        {
            RectTransform rectTransform = newGO.GetComponent<RectTransform>();
            rectTransform.SetParent(rt);
            Vector3 screenPos = Event.current.mousePosition;
            screenPos.y = view.position.height - screenPos.y;
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, view.camera, out pos);
            rectTransform.anchoredPosition = pos;
        }

        private static void SetReferenceValue(Object obj, string field)
        {
            Object value = DragAndDrop.objectReferences[0];
            SetReferenceValue(obj, field, value);
        }

        private static void SetReferenceValue(Object obj, string field, Object value)
        {
            SerializedObject so = new SerializedObject(obj);
            so.Update();
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}