/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class ObjectWindow : AutoSizePopupWindow
    {
        public bool drawHeader = true;
        public bool drawInspector = true;

        public Object[] targets;
        private Editor editor;

        protected override void OnContentGUI()
        {
            if (editor == null)
            {
                editor = Editor.CreateEditor(targets);
                if (editor is MaterialEditor) InternalEditorUtility.SetIsInspectorExpanded(editor.target, true);
            }

            if (editor != null)
            {
                if (drawHeader) editor.DrawHeader();
                if (drawInspector)
                {
                    EditorGUILayout.BeginVertical(contentAreaStyle);
                    editor.OnInspectorGUI();
                    
                    if (editor.GetType().IsSubclassOf(AssetImporterEditorRef.type))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        TextureImporterInspectorRef.OnApplyRevertGUI(editor);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (editor != null)
            {
                DestroyImmediate(editor);
                editor = null;
            }

            targets = null;
        }

        public static ObjectWindow Show(Object[] targets, bool autosize = true)
        {
            if (targets == null) return null;
            targets = targets.Where(t => t != null).ToArray();
            if (targets.Length == 0) return null;

            ObjectWindow wnd = CreateInstance<ObjectWindow>();
            wnd.titleContent = new GUIContent(targets[0].name);
            wnd.targets = targets;
            wnd.minSize = Vector2.one;
            if (autosize) wnd.adjustHeight = AutoSize.center;
            wnd.Show();
            wnd.Focus();
            if (Event.current != null)
            {
                Vector2 screenPoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 size = Prefs.defaultWindowSize;
                wnd.position = new Rect(screenPoint - size / 2, size);
            }
            return wnd;
        }

        public static ObjectWindow ShowPopup(Object[] targets, Rect? rect = null, string title = null)
        {
            if (targets == null) return null;
            targets = targets.Where(t => t != null).ToArray();
            if (targets.Length == 0) return null;

            ObjectWindow wnd = CreateInstance<ObjectWindow>();
            wnd.targets = targets;
            wnd.minSize = Vector2.one;

            if (!rect.HasValue)
            {
                Vector2 position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 size = Prefs.defaultWindowSize;
                rect = new Rect(position - size / 2, size);
            }

            Rect r = rect.Value;
            if (r.y < 30) r.y = 30;
            wnd.position = r;
            wnd.ShowPopup();
            wnd.Focus();
            wnd.adjustHeight = AutoSize.top;

            if (title == null) title = targets[0].name;
            wnd.titleContent = new GUIContent(title);
            wnd.drawTitle = true;

            return wnd;
        }

        public static ObjectWindow ShowUtility(Object[] targets, bool autosize = true)
        {
            if (targets == null) return null;
            targets = targets.Where(t => t != null).ToArray();
            if (targets.Length == 0) return null;

            ObjectWindow wnd = CreateInstance<ObjectWindow>();
            wnd.minSize = Vector2.one;
            wnd.titleContent = new GUIContent(targets[0].name);
            wnd.targets = targets;
            wnd.ShowUtility();
            wnd.Focus();
            if (autosize) wnd.adjustHeight = AutoSize.center;
            Vector2 size = Prefs.defaultWindowSize;
            wnd.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - size / 2, size);
            return wnd;
        }
    }
}