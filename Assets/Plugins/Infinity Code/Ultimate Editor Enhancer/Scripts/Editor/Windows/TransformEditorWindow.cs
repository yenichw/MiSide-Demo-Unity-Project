/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.TransformEditorTools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class TransformEditorWindow : AutoSizePopupWindow
    {
        private static TransformEditorWindow _instance;

        [SerializeField]
        private Transform[] transforms;

        private Editor editor;
        private TransformEditorTool[] tools;
        private TransformEditorTool activeTool;
        private GUIContent[] toolContents;
        private int activeToolIndex = -1;
        private GUIStyle leftStyle;
        private GUIStyle midStyle;
        private GUIStyle rightStyle;

        public static TransformEditorWindow instance
        {
            get { return _instance; }
        }

        private void CacheTools()
        {
            tools = Reflection.GetInheritedItems<TransformEditorTool>()
                .Where(t => t.Validate())
                .OrderBy(t => t.order).ToArray();
            List<GUIContent> contents = new List<GUIContent>();
            for (int i = 0; i < tools.Length; i++)
            {
                try
                {
                    TransformEditorTool t = tools[i];
                    t.Init();
                    contents.Add(t.content);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            toolContents = contents.ToArray();

            if (tools.Length > 1)
            {
                leftStyle = GUI.skin.FindStyle("Buttonleft");
                midStyle = GUI.skin.FindStyle("Buttonmid");
                rightStyle = GUI.skin.FindStyle("Buttonright");
            }
            else
            {
                leftStyle = midStyle = rightStyle = GUI.skin.button;
            }
        }

        private void DrawTools()
        {
            if (tools == null) CacheTools();

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < toolContents.Length; i++)
            {
                GUIStyle style = midStyle;
                if (i == 0) style = leftStyle;
                else if (i == toolContents.Length - 1) style = rightStyle;

                bool isActiveTool = i == activeToolIndex;
                bool newSelected = GUILayout.Toggle(isActiveTool, toolContents[i], style, GUILayout.ExpandWidth(false));
                if (newSelected != isActiveTool)
                {
                    if (activeTool != null)
                    {
                        try
                        {
                            activeTool.OnDisable();
                        }
                        catch (Exception e)
                        {
                            Log.Add(e);
                        }
                    }

                    if (isActiveTool)
                    {
                        activeToolIndex = -1;
                        activeTool = null;
                    }
                    else
                    {
                        activeToolIndex = i;
                        activeTool = tools[i];
                        try
                        {
                            activeTool.OnEnable();
                        }
                        catch (Exception e)
                        {
                            Log.Add(e);
                        }
                        
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (activeTool != null)
            {
                try
                {
                    activeTool.Draw();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }

        public static Transform[] GetTransforms()
        {
            if (_instance == null) return null;
            return _instance.transforms;
        }

        protected override void OnContentGUI()
        {
            if (editor == null)
            {
                editor = Editor.CreateEditor(transforms);
                if (editor == null) return;
            }

            editor.OnInspectorGUI();
            DrawTools();

            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape) Close();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _instance = null;
            transforms = null;

            if (activeTool != null)
            {
                try
                {
                    activeTool.OnDisable();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
                activeTool = null;
            }

            if (tools != null)
            {
                for (int i = 0; i < tools.Length; i++)
                {
                    try
                    {
                        tools[i].Dispose();
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }
                }
                tools = null;
            }

            if (editor != null)
            {
                DestroyImmediate(editor);
                editor = null;
            }
        }

        private void OnEnable()
        {
            _instance = this;
        }

        public static TransformEditorWindow ShowPopup(Transform[] transforms, Rect? rect = null)
        {
            if (transforms == null || transforms.Length == 0) return null;

            TransformEditorWindow wnd = CreateInstance<TransformEditorWindow>();
            wnd.transforms = transforms;
            wnd.adjustHeight = AutoSize.top;
            wnd.minSize = new Vector2(10, 10);

            if (!rect.HasValue)
            {
                Vector2 position = ToolValues.lastScreenPosition;
                position = GUIUtility.GUIToScreenPoint(new Vector2(position.x, Screen.height - position.y));
                if (ToolValues.isBelowHandle) position.y += 0;
                else
                {
                    position.y -= 20;
                    wnd.adjustHeight = AutoSize.bottom;
                }
                Vector2 size = new Vector2(Prefs.defaultWindowSize.x, 20);
                rect = new Rect(position - new Vector2(size.x / 2, 0), size);
            }

            Rect r = rect.Value;
            if (r.y < 40) r.y = 40;
            wnd.position = r;
            wnd.ShowPopup();
            wnd.Focus();
            wnd.drawTitle = true;
            wnd.titleContent = new GUIContent("Transform Tools");

            return wnd;
        }
    }
}