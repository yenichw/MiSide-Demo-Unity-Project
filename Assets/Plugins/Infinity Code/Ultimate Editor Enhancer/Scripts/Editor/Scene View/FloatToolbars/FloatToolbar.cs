/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    public abstract class FloatToolbar
    {
        private static Texture2D borderTexture;

        private bool _isDirty = true;
        protected ToolbarAlign align;
        protected Vector2 position;
        protected Rect rect;
        protected bool isDragging;
        private bool lastVisible;
        private Vector2 pressMousePosition;

        protected virtual Rect areaRect
        {
            get { return rect; }
        }

        protected abstract string prefKey { get; }
        protected abstract Vector2 size { get; }

        public bool isDirty
        {
            get { return _isDirty; }
            set { _isDirty = _isDirty || value; }
        }

        public bool hasLeftAlign
        {
            get { return align == ToolbarAlign.left || align == ToolbarAlign.topLeft || align == ToolbarAlign.bottomLeft; }
        }

        public bool hasRightAlign
        {
            get { return align == ToolbarAlign.right || align == ToolbarAlign.topRight || align == ToolbarAlign.bottomRight; }
        }

        public bool hasVerticalCenterAlign
        {
            get { return align == ToolbarAlign.top || align == ToolbarAlign.bottom; }
        }

        public bool hasTopAlign
        {
            get { return align == ToolbarAlign.top || align == ToolbarAlign.topLeft || align == ToolbarAlign.topRight; }
        }

        public bool hasBottomAlign
        {
            get { return align == ToolbarAlign.bottom || align == ToolbarAlign.bottomLeft || align == ToolbarAlign.bottomRight; }
        }

        public bool hasHorizontalCenterAlign
        {
            get { return align == ToolbarAlign.left || align == ToolbarAlign.right; }
        }

        public FloatToolbar()
        {
            SceneViewManager.OnValidateOpenContextMenu += OnValidateOpenContextMenu;
            EditorApplication.quitting += OnQuitting;
            Reset();
            Load();
        }

        protected abstract void DrawBackground();
        protected abstract void DrawContent();
        protected abstract Rect DrawHeader();

        protected virtual void DrawToolbar(SceneView sceneView)
        {
            DrawBackground();

            Rect headerRect = DrawHeader();
            ProcessHeaderEvents(SceneViewManager.GetRect(sceneView), headerRect);

            DrawContent();
        }

        private void Load()
        {
            string key = Prefs.Prefix + prefKey;

            string jsonString = EditorPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(jsonString)) return;

            JsonItem json = Json.Parse(jsonString);
            align = (ToolbarAlign)json.V<int>("align");
            position = json.V<Vector2>("position");

            OnLoad(json);
            isDirty = true;
        }

        protected virtual void OnDisable()
        {

        }

        protected virtual void OnHeaderClick()
        {

        }
        
        protected virtual void OnHeaderMiddleClick()
        {
            
        }

        protected virtual void OnLoad(JsonItem json)
        {

        }

        protected virtual void OnQuitting()
        {

        }

        protected virtual void OnPositionChanged(Rect screenRect)
        {

        }

        protected virtual void OnSave(JsonObject json)
        {

        }

        public void OnSceneViewGUI(SceneView sceneView)
        {
            if (Event.current.type == EventType.Layout)
            {
                bool v = Visible();
                if (lastVisible && !v)
                {
                    OnDisable();
                    lastVisible = false;
                }

                lastVisible = v;
            }

            if (!lastVisible) return;

            lastVisible = true;

            if (borderTexture == null) borderTexture = Resources.CreateSinglePixelTexture(0.1f);

            Rect viewRect = SceneViewManager.GetRect(sceneView);

            if (_isDirty)
            {
                UpdateRect(viewRect);
                _isDirty = false;
            }

            try
            {
                Handles.BeginGUI();

                PrepareToolbar(viewRect);

                GUI.DrawTexture(new Rect(areaRect.x - 1, areaRect.y - 1, areaRect.width + 2, areaRect.height + 2), borderTexture, ScaleMode.StretchToFill);

                GUILayout.BeginArea(areaRect);

                DrawToolbar(sceneView);

                GUILayout.EndArea();
                Handles.EndGUI();
            }
            catch
            {

            }
        }

        private bool OnValidateOpenContextMenu()
        {
            return !areaRect.Contains(Event.current.mousePosition);
        }

        protected virtual void PrepareHeaderContextMenu(GenericMenuEx menu)
        {

        }

        protected virtual void PrepareToolbar(Rect sceneRect)
        {

        }

        private void ProcessHeaderDrag(Rect sceneRect, Rect headerRect)
        {
            Event e = Event.current;

            if (isDragging)
            {
                rect.position += e.delta;
                pressMousePosition -= e.delta;
                Vector2 newPosition;
                ToolbarAlign newAlign = SceneViewAlignDrawer.Update(sceneRect, rect, out newPosition);
                if (newAlign != align || newPosition != position)
                {
                    align = newAlign;
                    position = newPosition;
                    UpdateRect(sceneRect);
                }

                e.Use();
            }
            else if (headerRect.Contains(e.mousePosition))
            {
                StartDragTargets();
                e.Use();
            }
        }

        protected void ProcessHeaderEvents(Rect sceneRect, Rect headerRect)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown) ProcessHeaderPress(sceneRect, headerRect);
            else if (e.type == EventType.MouseUp) ProcessHeaderRelease(sceneRect, headerRect);
            else if (e.type == EventType.MouseDrag) ProcessHeaderDrag(sceneRect, headerRect);
            else if (e.type == EventType.MouseMove && isDragging)
            {
                isDragging = false;
                TryFixPosition(sceneRect);
                e.Use();
                GUIUtility.hotControl = 0;
                SceneViewAlignDrawer.Hide();
            }
        }

        private void ProcessHeaderPress(Rect sceneRect, Rect headerRect)
        {
            Event e = Event.current;

            if (!headerRect.Contains(e.mousePosition)) return;

            Vector2 mousePosition = e.mousePosition;
            pressMousePosition = mousePosition;
            
            if (e.button == 0)
            {
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                if (!isDragging && (e.control || e.command) && headerRect.Contains(mousePosition))
                {
                    isDragging = true;
                    Vector2 newPosition;
                    ToolbarAlign newAlign = SceneViewAlignDrawer.Show(sceneRect, rect, out newPosition);
                    if (newAlign != align || newPosition != position)
                    {
                        align = newAlign;
                        position = newPosition;
                        UpdateRect(sceneRect);
                        Save();
                    }
                }

                e.Use();
            }
            else if (e.button == 1)
            {
                if (headerRect.Contains(mousePosition))
                {
                    e.Use();
                    ShowHeaderContextMenu();
                }
            }
            else if (e.button == 2)
            {
                e.Use();
            }
        }

        private void ProcessHeaderRelease(Rect sceneRect, Rect headerRect)
        {
            Event e = Event.current;
            bool isClick = headerRect.Contains(e.mousePosition) && (e.mousePosition - pressMousePosition).sqrMagnitude < 10;
            
            if (e.button == 0)
            {
                if (isDragging)
                {
                    isDragging = false;
                    TryFixPosition(sceneRect);
                    GUIUtility.hotControl = 0;
                    SceneViewAlignDrawer.Hide();
                }

                if (isClick)
                {
                    OnHeaderClick();
                }
                
                e.Use();
            }
            else if (e.button == 1)
            {
                e.Use();
            }
            else if (e.button == 2)
            {
                if (isClick)
                {
                    OnHeaderMiddleClick();
                    e.Use();
                }
            }
        }

        protected virtual void Reset()
        {

        }

        protected void Save()
        {
            object fields = new
            {
                align = (int)align,
                position
            };

            JsonItem json = Json.Serialize(fields);

            OnSave(json as JsonObject);

            EditorPrefs.SetString(Prefs.Prefix + prefKey, json.ToString());
        }

        private void ShowHeaderContextMenu()
        {
            GenericMenuEx menu = GenericMenuEx.Start();

            PrepareHeaderContextMenu(menu);

            if (menu.count > 0) menu.Show();
        }

        protected abstract void StartDragTargets();

        private void TryFixPosition(Rect sceneRect)
        {
            bool changed = false;

            if (hasLeftAlign)
            {
                if (position.x < 3)
                {
                    position.x = 3;
                    changed = true;
                }
            }
            else if (hasRightAlign)
            {
                if (position.x > -5)
                {
                    position.x = -5;
                    changed = true;
                }
            }

            if (hasTopAlign)
            {
                if (position.y < 3)
                {
                    position.y = 3;
                    changed = true;
                }
            }
            else if (hasBottomAlign)
            {
                int bottomY = -5;

                if (position.y > bottomY)
                {
                    position.y = bottomY;
                    changed = true;
                }
            }

            if (changed)
            {
                UpdateRect(sceneRect);
                Save();
            }
        }

        protected virtual void UpdateRect(Rect sceneRect)
        {
            rect = new Rect(position, size);

            Vector2 sceneSize = sceneRect.size;
            float sceneHeight = sceneSize.y;

            Rect r = QuickAccess.GetFreeRect(sceneRect);
            rect.x += r.x;
            sceneSize.x -= r.x;

            if (hasVerticalCenterAlign) rect.x += (sceneSize.x - size.x) / 2;
            else if (hasRightAlign) rect.x += sceneSize.x - size.x;

            if (hasHorizontalCenterAlign) rect.y += (sceneHeight - size.y) / 2;
            else if (hasBottomAlign) rect.y += sceneHeight - size.y;

            OnPositionChanged(sceneRect);
        }

        protected virtual bool Visible()
        {
            return true;
        }
    }
}