/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [Serializable] 
    public abstract class AutoSizePopupWindow : PopupWindow
    {
        public Action<AutoSizePopupWindow> OnClose;
        public Action<Rect> OnPositionChanged;

        protected Action OnPin;

        [NonSerialized]
        public AutoSize adjustHeight = AutoSize.ignore;

        [SerializeField]
        public bool closeOnLossFocus = true;

        [SerializeField]
        public bool closeOnCompileOrPlay = true;

        [SerializeField]
        public bool drawTitle = false;

        [NonSerialized]
        public Vector2 scrollPosition;

        [NonSerialized]
        public bool wasMoved;

        public float maxHeight = 400;

        private bool isDragging;
        private bool isTooBig;
        private GUIContent labelContent;
        private Vector2 lastMousePosition;
        private float prevBottom = 0;

        [NonSerialized]
        private GUIStyle _contentAreaStyle;

        protected GUIStyle contentAreaStyle
        {
            get
            {
                if (_contentAreaStyle == null)
                {
                    _contentAreaStyle = new GUIStyle
                    {
                        margin =
                        {
                            left = 12
                        }
                    };
                }

                return _contentAreaStyle;
            }
        }

        private void AdjustHeight(float bottom)
        {
            Rect pos = position;
            float currentHeight = pos.height;

            if (bottom > maxHeight)
            {
                if (isTooBig) return;

                if (pos.y < 40) pos.y = 40;

                if (adjustHeight == AutoSize.bottom)
                {
                    pos.y += currentHeight - maxHeight;
                }
                else if (adjustHeight == AutoSize.center)
                {
                    pos.y -= (currentHeight - maxHeight) / 2;
                }

                pos.height = maxHeight;
                position = pos;

                isTooBig = true;
                return;
            }

            isTooBig = false;

            if (!(Mathf.Abs(bottom - currentHeight) > 1)) return;

            if (pos.y < 40) pos.y = 40;

            if (adjustHeight == AutoSize.bottom)
            {
                pos.y += currentHeight - bottom;
            }
            else if (adjustHeight == AutoSize.center)
            {
                pos.y -= (currentHeight - bottom) / 2;
            }

            pos.height = bottom;
            position = pos;
        }

        private void DrawLabel()
        {
            if (labelContent == null) labelContent = new GUIContent(titleContent.text);
            float width = position.width - 35;
            if (OnPin != null) width -= 20;

            GUILayout.Label(labelContent, EditorStyles.whiteLabel, GUILayout.MaxWidth(width), GUILayout.Height(20));
            EditorGUILayout.Space();

            ProcessLabelEvents(GUILayoutUtility.GetLastRect());
        }

        private void DrawTitle()
        {
            GUI.DrawTexture(new Rect(0, 0, position.width, 20), background, ScaleMode.StretchToFill);

            EditorGUILayout.BeginHorizontal();

            DrawLabel();

            if (OnPin != null && GUILayout.Button(PinAndClose.tabContent, Styles.transparentButton, GUILayout.Width(12), GUILayout.Height(12)))
            {
                OnPin();
            }
            if (GUILayout.Button(PinAndClose.closeContent, Styles.transparentButton, GUILayout.Width(12), GUILayout.Height(12)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void InvokePin()
        {
            
        }

        protected abstract void OnContentGUI();

        protected virtual void OnDestroy()
        {
            if (OnClose != null) OnClose(this);
        }

        protected override void OnGUI()
        {
            if (closeOnLossFocus && focusedWindow != this && focusedWindow != null)
            {
                if (ValidateCloseOnLossFocus())
                {
                    Close();
                    return;
                }
            }

            if (drawTitle) DrawTitle();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayoutUtils.nestedEditorMargin = 14;
            try
            {
                OnContentGUI();
            }
            catch (ExitGUIException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
            finally
            {
                GUILayoutUtils.nestedEditorMargin = 0;
            }

            if (adjustHeight != AutoSize.ignore)
            {
                float b = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(0)).yMin;
                Event e = Event.current;
                if (e.type == EventType.Repaint)
                {
                    float bottom = b + 5;
                    if (drawTitle) bottom += 20;

                    if (Mathf.Abs(bottom - position.height) > 1 && Math.Abs(prevBottom - bottom) > float.Epsilon)
                    {
                        AdjustHeight(bottom);
                        prevBottom = bottom;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void ProcessLabelEvents(Rect labelRect)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                Rect r = new Rect(0, 0, labelRect.xMax, 20);
                if (e.button == 0 && r.Contains(e.mousePosition) && GUIUtility.hotControl == 0)
                {
                    isDragging = true;
                    Focus();
                    lastMousePosition = GUIUtility.GUIToScreenPoint(e.mousePosition);
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    e.Use();
                }
            }
            else if (e.rawType == EventType.MouseUp)
            {
                if (isDragging && e.button == 0)
                {
                    isDragging = false;
                    wasMoved = true;

                    e.Use();
                    GUIUtility.hotControl = 0;
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (isDragging)
                {
                    Vector2 mousePosition = GUIUtility.GUIToScreenPoint(e.mousePosition);
                    Vector2 delta = mousePosition - lastMousePosition;

                    Rect rect = position;
                    rect.position += delta;
                    position = rect;

                    lastMousePosition = mousePosition;

                    e.Use();
                }
            }
        }

        public void SetRect(Rect rect)
        {
            position = rect;
            if (OnPositionChanged != null) OnPositionChanged(rect);
        }

        protected virtual bool ValidateCloseOnLossFocus()
        {
#if UNITY_2023_2_OR_NEWER
            if (focusedWindow != null && focusedWindow.GetType().Name == "ContextMenu") return false;
#endif
            return true;
        }
    }
}