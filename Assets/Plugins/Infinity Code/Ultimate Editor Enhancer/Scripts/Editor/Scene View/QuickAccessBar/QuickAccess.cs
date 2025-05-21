/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.EditorMenus;
using InfinityCode.UltimateEditorEnhancer.Integration;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class QuickAccess
    {
        public static Action OnCollapseChanged;
        public static Action OnVisibleChanged;

        public static EditorWindow activeWindow;
        public static int activeWindowIndex = -1;
        public static int invokeItemIndex;
        public static Rect invokeItemRect;
        public static int invokeItemVisibleIndex;

        private static GUIStyle _activeContentStyle;
        private static Texture2D _collapseLineTexture;
        private static GUIStyle _contentStyle;
        private static GUIStyle _tooltipStyle;
        private static bool _visible;
        private static Texture2D background;
        private static Action invokeItemAction;
        private static Rect rect;
        private static Rect tooltipRect;
        private static string tooltip;
        private static bool collapseLineDrawn;

        public static GUIStyle activeContentStyle
        {
            get
            {
                if (_activeContentStyle == null)
                {
                    _activeContentStyle = new GUIStyle(contentStyle);
                    _activeContentStyle.normal.background = _activeContentStyle.active.background;
                }

                return _activeContentStyle;
            }
        }

        public static bool collapsed
        {
            get { return LocalSettings.collapseQuickAccessBar; }
            set
            {
                bool changed = LocalSettings.collapseQuickAccessBar != value;
                LocalSettings.collapseQuickAccessBar = value;
                if (changed && OnCollapseChanged != null) OnCollapseChanged();
            }
        }

        public static GUIStyle contentStyle
        {
            get
            {
                if (_contentStyle == null || _contentStyle.normal.background == null)
                {
                    _contentStyle = new GUIStyle
                    {
                        fontSize = 8,
                        fixedHeight = 32,
                        normal = { background = Resources.CreateSinglePixelTexture(0, 0.1f), textColor = Color.white },
                        hover = { background = Resources.CreateSinglePixelTexture(0, 0.2f), textColor = Color.white },
                        active = { background = Resources.CreateSinglePixelTexture(0, 0.3f), textColor = Color.white },
                        padding = new RectOffset(),
                        alignment = TextAnchor.MiddleCenter,
                        imagePosition = ImagePosition.ImageAbove
                    };
                }

                return _contentStyle;
            }
        }

        public static GUIStyle tooltipStyle
        {
            get
            {
                if (_tooltipStyle == null || _tooltipStyle.normal.background == null)
                {
                    _tooltipStyle = new GUIStyle()
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(0.2f),
                            textColor = Color.white
                        },
                        padding = new RectOffset(5, 5, 5, 5)
                    };
                }

                return _tooltipStyle;
            }
        }

        public static bool visible
        {
            get
            {
                if (!Prefs.quickAccessBar) return false;

                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null) return false;

                bool maximized = WindowsHelper.IsMaximized(sceneView);
                return ReferenceManager.quickAccessItems.Any(i => i.Visible(maximized));
            }
        }

        public static float width
        {
            get { return 32; }
        }

        private static Texture2D collapseLineTexture
        {
            get
            {
                if (_collapseLineTexture == null) _collapseLineTexture = Resources.CreateSinglePixelTexture(0.3f, 0.8f, 0.3f, 0.6f);
                return _collapseLineTexture;
            }
        }

        static QuickAccess()
        {
            SceneViewManager.OnValidateOpenContextMenu += OnValidateOpenContextMenu;
            SceneViewManager.AddListener(OnSceneGUI, SceneViewOrder.QuickAccessBar, true);
        }

        public static void CheckActiveWindow()
        {
            if (activeWindow == null) activeWindowIndex = -1;
        }

        public static void CloseActiveWindow()
        {
            if (activeWindow == null) return;

            if (EditorMenu.allowCloseWindow) activeWindow.Close(); 
            activeWindow = null;
            activeWindowIndex = -1;
        }

        private static void DrawBackground()
        {
            if (background == null)
            {
                background = new Texture2D(1, 1);
                background.SetPixel(0, 0, new Color(0, 0, 0, 0.6f));
                background.Apply();
            }

            float minIntend = GetMinIntend();
            float maxIntend = Prefs.quickAccessBarIndentMax;

            GUI.DrawTexture(new Rect(0, minIntend, rect.width, rect.height - minIntend - maxIntend), background, ScaleMode.StretchToFill);
        }

        private static void DrawCollapseArea(SceneView view, float minIntend, float maxIntend)
        {
            if (!(WindowsHelper.mouseOverWindow is SceneView)) return;

            Event e = Event.current;
            Vector2 p = e.mousePosition;
            if (p.y < minIntend || p.y > rect.height - maxIntend ||
                p.x < rect.width || p.x > rect.width + 3)
            {
                if (collapseLineDrawn)
                {
                    collapseLineDrawn = false;
                    view.Repaint();
                }
                return;
            }

            if (e.type == EventType.Repaint)
            {
                GUI.DrawTexture(new Rect(rect.width, minIntend, 3, rect.height - minIntend - maxIntend), collapseLineTexture, ScaleMode.StretchToFill);
            }
            else if (e.type == EventType.MouseDown)
            {
                collapsed = true;
                e.Use();
                SceneViewManager.BlockMouseUp();
            }
            else if (e.type == EventType.MouseMove && !collapseLineDrawn)
            {
                collapseLineDrawn = true;
                view.Repaint();
            }
        }

        private static void DrawContent(SceneView view)
        {
            CheckActiveWindow();

            Event e = Event.current;
            bool maximized = WindowsHelper.IsMaximized(view);

            int index = -1;
            int visibleIndex = -1;

            float y = GetMinIntend();

            foreach (QuickAccessItem item in ReferenceManager.quickAccessItems)
            {
                index++;
                if (item.type == QuickAccessItemType.flexibleSpace)
                {
                    GUILayout.FlexibleSpace();
                    continue;
                }

                if (item.type == QuickAccessItemType.space)
                {
                    GUILayout.Space(item.intSettings[0]);
                    continue;
                }
                
                if (!item.Visible(maximized)) continue;

                if (item.type == QuickAccessItemType.action)
                {
                    item.DrawAction();
                    continue;
                }

                if (item.content == null) continue;
                visibleIndex++;

                GUIStyle style = activeWindowIndex == index ? activeContentStyle : contentStyle;
                int padding = Mathf.RoundToInt(rect.width / 2 * (1 - item.iconScale));
                style.padding = new RectOffset(padding, padding, padding, padding);

                string contentTooltip = item.content.tooltip;
                GUIContent content = TempContent.Get(item.content);
                content.tooltip = null;

                ButtonEvent buttonEvent = GUILayoutUtils.Button(content, style, GUILayout.Width(width), GUILayout.Height(width));
                if (buttonEvent == ButtonEvent.press)
                {
                    if (e.button == 0)
                    {
                        invokeItemRect = GUILayoutUtils.lastRect;
                        invokeItemRect.y += y;
                        invokeItemIndex = index;
                        invokeItemVisibleIndex = visibleIndex;
                        invokeItemAction = item.Invoke;
                        EditorApplication.update += InvokeItemAction;
                        e.Use();
                    }
                    else if (e.button == 1) ShowContextMenu();
                }
                else if (buttonEvent == ButtonEvent.hover && !string.IsNullOrEmpty(contentTooltip))
                {
                    tooltipRect = GUILayoutUtils.lastRect;
                    tooltip = contentTooltip;
                }
            }

            if (e.type == EventType.MouseDown && e.mousePosition.x < width && e.button == 1) ShowContextMenu();
        }

        private static void DrawTooltip(SceneView view)
        {
            if (Event.current.type != EventType.Repaint || string.IsNullOrEmpty(tooltip)) return;

            GUIContent content = TempContent.Get(tooltip);
            Vector2 size = tooltipStyle.CalcSize(content);

            Rect r = new Rect(tooltipRect.x + width + 3, tooltipRect.center.y - size.y / 2, size.x, size.y);

            if (r.y < 2) r.y = 2;
            else if (r.yMax > view.position.height - 2) r.y = view.position.height - size.y - 2;

            tooltipStyle.Draw(r, content, -1);
        }

        private static float GetMinIntend()
        {
            float v = 0;

            if (ProGrids.isEnabled)
            {
                if (ProGrids.isMenuOpen) v = 205;
                else v = 25;
            }

            return Mathf.Max(v, Prefs.quickAccessBarIndentMin);
        }

        public static Rect GetFreeRect(Rect sceneRect)
        {
            if (visible && !collapsed) sceneRect.xMin = width;
            return sceneRect;
        }

        private static void InvokeItemAction()
        {
            EditorApplication.update -= InvokeItemAction;
            if (invokeItemAction == null) return;

            try
            {
                invokeItemAction();
            }
            catch
            {
                
            }

            invokeItemAction = null;
            invokeItemVisibleIndex = invokeItemIndex = -1;
        }

        private static void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;
            if (e.type == EventType.Layout)
            {
                bool v = visible;
                if (v != _visible)
                {
                    if (OnVisibleChanged != null) OnVisibleChanged();
                    _visible = v;
                }
            }
            if (!_visible) return;

            Rect viewRect = SceneViewManager.GetRect(view);

            rect = new Rect(0, 0, width, viewRect.height);

            try
            {
                Handles.BeginGUI();

                tooltip = null;

                float minIntend = GetMinIntend();
                float maxIntend = Mathf.Max(Prefs.quickAccessBarIndentMax, 0);

                if (collapsed)
                {
                    float mx = e.mousePosition.x;
                    if (mx >= 0 && mx < 5)
                    {
                        if (e.type == EventType.Repaint)
                        {
                            GUI.DrawTexture(new Rect(0, minIntend, 3, rect.height - minIntend - maxIntend), collapseLineTexture, ScaleMode.StretchToFill);
                        }
                        else if (e.type == EventType.MouseDown)
                        {
                            collapsed = false;
                            e.Use();
                            SceneViewManager.BlockMouseUp();
                        }
                        else if (e.type == EventType.MouseMove && !collapseLineDrawn)
                        {
                            collapseLineDrawn = true;
                            view.Repaint();
                        }
                    }
                    else if (collapseLineDrawn)
                    {
                        collapseLineDrawn = false;
                        view.Repaint();
                    }

                    Handles.EndGUI();

                    return;
                }

                DrawBackground();

                Rect areaRect = new Rect(0, minIntend, rect.width, rect.height - minIntend - maxIntend);
                GUILayout.BeginArea(areaRect);
                try
                {
                    DrawContent(view);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
                GUILayout.EndArea();

                DrawCollapseArea(view, minIntend, maxIntend);
                DrawTooltip(view);

                Handles.EndGUI();

                if (e.type == EventType.MouseMove && areaRect.Contains(e.mousePosition)) view.Repaint();
            }
            catch
            {
                
            }
        }

        private static bool OnValidateOpenContextMenu()
        {
            return !visible || Event.current.mousePosition.x > width;
        }

        public static void SetTooltip(string text, Rect buttonRect)
        {
            tooltipRect = buttonRect;
            tooltip = text;
        }

        private static void ShowContextMenu()
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Edit", Settings.OpenQuickAccessSettings);
            menu.Add("Collapse", () => collapsed = true);
            menu.Show();
            Event.current.Use();
        }
    }
}