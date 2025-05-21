/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.EditorMenus;
using InfinityCode.UltimateEditorEnhancer.Tools;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class SmartSelection
    {
        private static GUIStyle _areaStyle;
        private static GameObject highlightGO;
        private static GameObject lastHighlightGO;
        private static Rect screenRect;
        private static KeyManager.KeyBinding binding;

        private static GUIStyle areaStyle
        {
            get
            {
                if (_areaStyle == null)
                {
                    _areaStyle = new GUIStyle(Waila.StyleID);
                    _areaStyle.fontSize = 10;
                    _areaStyle.stretchHeight = true;
                    _areaStyle.fixedHeight = 0;
                    _areaStyle.border = new RectOffset(8, 8, 8, 8);
                    _areaStyle.margin = new RectOffset(4, 4, 4, 4);
                }

                return _areaStyle;
            }
        }

        static SmartSelection()
        {
            var b = KeyManager.AddBinding();
            b.OnValidate += () => Prefs.wailaSmartSelection && Event.current.keyCode == Prefs.wailaSmartSelectionKeyCode && Event.current.modifiers == Prefs.wailaSmartSelectionModifiers;
            b.OnPress += OnInvoke;

            Waila.OnClose += OnClose;
            Waila.OnDrawModeExternal += OnDrawModeExternal;
            Waila.OnUpdateTooltipsExternal += OnUpdateTooltipsExternal;
        }

        private static void Close()
        {
            KeyManager.RemoveBinding(binding);
            binding = null;

            Waila.Close();
            UnityEditor.Tools.hidden = false;
        }

        private static void DrawButton(ref Rect r, Transform t, bool addSlash, ref bool state)
        {
            if (t.parent != null)
            {
                DrawButton(ref r, t.parent, true, ref state);
            }

            Rect r2 = new Rect(r);
            GUIContent content = new GUIContent(t.gameObject.name);
            GUIStyle style = Waila.labelStyle;
            r2.width = style.CalcSize(content).x + style.margin.horizontal;

            r.xMin += r2.width;

            ButtonEvent buttonEvent = GUILayoutUtils.Button(r2, content, style);
            Event e = Event.current;

            if (buttonEvent == ButtonEvent.click)
            {
                EditorMenu.OnValidateOpen -= OnValidateOpenEditorMenu;
                
                if (e.button == 0)
                {
                    if (e.control || e.shift) SelectionRef.Add(t.gameObject);
                    else Selection.activeGameObject = t.gameObject;
                    state = true;
                }
                else if (e.button == 1)
                {
                    GameObjectUtils.ShowContextMenu(false, t.gameObject);
                    e.Use();
                }
                else if (e.button == 2)
                {
                    Undo.RecordObject(t.gameObject, "Toggle Active");
                    t.gameObject.SetActive(!t.gameObject.activeSelf);
                    e.Use();
                }
            }
            else if (buttonEvent == ButtonEvent.press)
            {
                if (e.button == 1)
                {
                    EditorMenu.OnValidateOpen -= OnValidateOpenEditorMenu;
                    EditorMenu.OnValidateOpen += OnValidateOpenEditorMenu;
                }
            }
            else if (buttonEvent == ButtonEvent.drag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { t.gameObject };
                DragAndDrop.StartDrag("Drag " + t.gameObject.name);
                e.Use();
                state = true;
            }

            if (r2.Contains(e.mousePosition))
            {
                highlightGO = t.gameObject;
            }

            if (addSlash)
            {
                r2.xMin = r2.xMax;
                content.text = "/";
                r2.width = style.CalcSize(content).x + style.margin.horizontal;
                GUI.Label(r2, content, style);
                r.xMin += r2.width;
            }
        }

        private static void DrawBubbleSmartSelection(Waila.SceneViewItem sceneViewItem)
        {
            if (!UnityEditor.Tools.hidden) UnityEditor.Tools.hidden = true;

            Event e = Event.current;
            EventType type = e.type;

            highlightGO = null;

            try
            {
                Handles.BeginGUI();

                if (e.type == EventType.Repaint) areaStyle.Draw(screenRect, GUIContent.none, -1);

                GUIStyle style = Waila.labelStyle;
                RectOffset margin = style.margin;
                RectOffset padding = style.padding;

                Rect r = new Rect(screenRect.x + 5, screenRect.y + margin.top + padding.top, screenRect.width - 10, style.lineHeight + margin.vertical + padding.vertical);

                GUI.Label(r, "Select GameObject:", style);
                r.y += r.height + margin.bottom;
                r.height = 1;
                EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 1));

                r.y += 2;
                r.height = style.lineHeight + margin.vertical + padding.vertical;

                try
                {
                    bool state = false;

                    for (int i = 0; i < sceneViewItem.targets.Count; i++)
                    {
                        Rect r2 = new Rect(r);
                        r2.y += i * (style.lineHeight + margin.vertical + padding.vertical);
                        Transform t = sceneViewItem.targets[i].transform;
                        try
                        {
                            DrawButton(ref r2, t, false, ref state);
                        }
                        catch
                        {
                        }
                    }

                    if (state) Close();
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }

                if (lastHighlightGO != highlightGO)
                {
                    lastHighlightGO = highlightGO;
                    Highlighter.Highlight(highlightGO);
                }

                if (type == EventType.MouseUp)
                {
                    if (e.button == 0)
                    {
                        sceneViewItem.mode = 0;
                        UnityEditor.Tools.hidden = false;    
                    }
                }
                else if (type == EventType.KeyDown)
                {
                    if (e.keyCode != KeyCode.None && !KeyManager.IsModifier(e.keyCode) && e.keyCode != Prefs.wailaSmartSelectionKeyCode)
                    {
                        sceneViewItem.mode = 0;
                        UnityEditor.Tools.hidden = false;
                    }
                }

                Handles.EndGUI();
            }
            catch
            {
            }
        }

        private static void DrawSmartSelection(Waila.SceneViewItem sceneViewItem)
        {
            if (Prefs.wailaSmartSelectionStyle == SmartSelectionStyle.bubble) DrawBubbleSmartSelection(sceneViewItem);
        }

        private static void OnClose()
        {
            EditorMenu.OnValidateOpen -= OnValidateOpenEditorMenu;
            
            Waila.Highlight(null);
            if (binding != null)
            {
                KeyManager.RemoveBinding(binding);
                binding = null;
            }
        }

        private static void OnDrawModeExternal(Waila.SceneViewItem sceneViewItem)
        {
            if (sceneViewItem.mode != 1) return;

            DrawSmartSelection(sceneViewItem);
        }

        private static void OnInvoke()
        {
            SceneView view = WindowsHelper.mouseOverWindow as SceneView;
            if (view == null) return;

            Waila.SceneViewItem sceneViewItem = Waila.GetSceneViewItem(view);
            if (sceneViewItem == null || sceneViewItem.mode != 0 || sceneViewItem.targets.Count == 0) return;

            Event e = Event.current;

            if (e.keyCode == Prefs.wailaSmartSelectionKeyCode &&
                e.modifiers == Prefs.wailaSmartSelectionModifiers)
            {
                SceneViewManager.OnNextGUI += () =>
                {
                    ShowSmartSelection(sceneViewItem);
                };
                e.Use();
            }
        }

        private static bool OnUpdateTooltipsExternal(Waila.SceneViewItem sceneViewItem)
        {
            if (Prefs.wailaShowAllNamesUnderCursor && Event.current.modifiers == Prefs.wailaShowAllNamesUnderCursorModifiers)
            {
                UpdateAllTooltips(sceneViewItem);
                return true;
            }

            return false;
        }

        private static bool OnValidateOpenEditorMenu()
        {
            return false;
        }

        private static bool PrepareBubble(List<GameObject> targets, GUIStyle style, RectOffset margin, float height, RectOffset padding, Vector2 slashSize, int rightMargin, float width)
        {
            int count = 0;

            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject go = targets[i];
                    if (go == null) break;

                    float w = 0;
                    Transform t = go.transform;
                    Vector2 contentSize = style.CalcSize(new GUIContent(t.gameObject.name));
                    w += contentSize.x + margin.horizontal;
                    height += contentSize.y + margin.bottom + padding.bottom;

                    while (t.parent != null)
                    {
                        t = t.parent;
                        w += slashSize.x + rightMargin;
                        contentSize = style.CalcSize(new GUIContent(t.gameObject.name));
                        w += contentSize.x + rightMargin;
                    }

                    w += 5;
                    if (w > width) width = w;

                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            if (count == 0) return false;

            Vector2 size = new Vector2(width + 12, height + 32);
            Vector2 position = Event.current.mousePosition - new Vector2(size.x / 2, size.y * 1.5f);
            SceneView view = SceneView.lastActiveSceneView;

            if (position.x < 5) position.x = 5;
            else if (position.x + size.x > view.position.width - 5) position.x = view.position.width - size.x - 5;

            if (position.y < 5) position.y = 5;
            else if (position.y + size.y > view.position.height - 5) position.y = view.position.height - size.y - 5;

            screenRect = new Rect(position, size);
            return true;
        }

        public static void ShowSmartSelection(Waila.SceneViewItem sceneViewItem)
        {
            if (WindowsHelper.mouseOverWindow != null && !(WindowsHelper.mouseOverWindow is SceneView)) return;

            List<GameObject> targets = sceneViewItem.targets;

            if (!Prefs.wailaShowAllNamesUnderCursor || targets == null || targets.Count == 0)
            {
                UpdateAllTooltips(sceneViewItem);
            }
            if (targets == null || targets.Count == 0) return;

            if (Prefs.wailaSmartSelectionStyle != SmartSelectionStyle.bubble) FlatSmartSelectionWindow.Show(sceneViewItem);
            else
            {
                GUIStyle style = Waila.labelStyle;
                RectOffset margin = style.margin;
                RectOffset padding = style.padding;

                float width = style.CalcSize(new GUIContent("Select GameObject")).x + margin.horizontal + 10;

                int rightMargin = margin.right;
                Vector2 slashSize = style.CalcSize(new GUIContent("/"));

                float height = margin.top;

                try
                {
                    if (!PrepareBubble(targets, style, margin, height, padding, slashSize, rightMargin, width))
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw e;
                }
            }

            sceneViewItem.mode = 1;
            sceneViewItem.tooltip = null;

            binding = KeyManager.AddBinding(KeyCode.Escape);
            binding.OnPress += Close;
        }

        private static void UpdateAllTooltips(Waila.SceneViewItem sceneViewItem)
        {
            sceneViewItem.tooltip = null;

            int count = 0;

            StringBuilder builder = StaticStringBuilder.Start();

            sceneViewItem.targets.Clear();

            while (count < 20)
            {
                GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false, sceneViewItem.targets.ToArray());
                if (go == null) break;

                sceneViewItem.targets.Add(go);
                if (count > 0) builder.Append("\n");
                int length = builder.Length;
                Transform t = go.transform;
                builder.Append(t.gameObject.name);
                while (t.parent != null)
                {
                    t = t.parent;
                    builder.Insert(length, " / ");
                    builder.Insert(length, t.gameObject.name);
                }

                count++;
            }

            if (sceneViewItem.targets.Count > 0) Waila.Highlight(sceneViewItem.targets[0]);
            else Waila.Highlight(null);

            if (count > 0) sceneViewItem.tooltip = new GUIContent(builder.ToString());
        }
    }
}