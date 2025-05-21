/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.PopupWindows
{
    [RequireSelected]
    public class Components : PopupWindowItem, IValidatableLayoutItem
    {
        private GUIContent addContent;
        private Component[] components;
        private GUIContent[] contents;
        private GUIContent labelContent;
        private Vector2 labelSize;

        public override float order
        {
            get { return 100; }
        }

        protected override void CalcSize()
        {
            labelSize = EditorStyles.whiteLabel.CalcSize(labelContent);
            _size = labelSize;
            _size.y += GUI.skin.label.margin.bottom;
            _size.x += EditorStyles.whiteLabel.CalcSize(new GUIContent("+")).x;

            GUIStyle style = Styles.buttonWithToggleAlignLeft;
            int marginBottom = style.margin.bottom;

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null) continue;

                Vector2 s = style.CalcSize(contents[i]);
                if (contents[i].image != null) s.x -= contents[i].image.width - 20;
                _size.x = Mathf.Max(_size.x, s.x);
                _size.y += s.y + marginBottom;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            components = null;
            contents = null;
            labelContent = null;
        }

        public override void Draw()
        {
            Event e = Event.current;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Components:", EditorStyles.whiteLabel, GUILayout.Width(labelSize.x));

            if (Prefs.componentsInPopupWindowAddComponent && GUILayout.Button(addContent, EditorStyles.whiteLabel, GUILayout.ExpandWidth(false)))
            {
                Vector2 s = Prefs.defaultWindowSize;
                Rect rect = new Rect(GUIUtility.GUIToScreenPoint(e.mousePosition) - s / 2, s);

                EditorMenu.Close();
                AddComponent.ShowAddComponent(rect);
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null) continue;

                GUIContent content = contents[i];

                ButtonEvent buttonEvent = DrawComponent(component, content);

                if (buttonEvent == ButtonEvent.drag)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { component };
                    DragAndDrop.StartDrag("Drag " + component.name);
                    e.Use();
                    GUI.changed = true;
                }
                else if (buttonEvent == ButtonEvent.click)
                {
                    if (e.button == 0)
                    {
                        if (Prefs.popupWindowTab && Prefs.popupWindowTabModifiers == e.modifiers) ComponentWindow.Show(component);
                        else if (Prefs.popupWindowUtility && Prefs.popupWindowUtilityModifiers == e.modifiers) ComponentWindow.ShowUtility(component);
                        else if (Prefs.popupWindowPopup && Prefs.popupWindowPopupModifiers == e.modifiers)
                        {
                            EditorWindow wnd = ComponentWindow.ShowPopup(component);
                            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += b =>
                            {
                                wnd.Close();
                                b.Remove();
                            };
                        }
                        EditorMenu.Close();
                    }
                    else if (e.button == 1)
                    {
                        ComponentUtils.ShowContextMenu(component);
                        SceneViewManager.OnNextGUI += WaitCloseContextMenu;
                    }

                    e.Use();
                }
            }
        }

        public static ButtonEvent DrawComponent(Component component, GUIContent content)
        {
            Event e = Event.current;

            Rect rect = GUILayoutUtility.GetRect(content, Styles.buttonWithToggleAlignLeft);
            Rect toggleRect = new Rect(rect.x + 4, rect.y + 2, 16, 16);
            int id = GUIUtility.GetControlID(GUILayoutUtils.buttonHash, FocusType.Passive, rect);
            bool isHover = rect.Contains(e.mousePosition) && !toggleRect.Contains(e.mousePosition);
            bool hasMouseControl = GUIUtility.hotControl == id;

            ButtonEvent state = ButtonEvent.none;

            if (e.type == EventType.Repaint) Styles.buttonWithToggleAlignLeft.Draw(rect, content, isHover, hasMouseControl, false, false);
            else if (e.type == EventType.MouseDrag)
            {
                if (hasMouseControl)
                {
                    GUIUtility.hotControl = 0;
                    state = ButtonEvent.drag;
                }
            }
            else if (e.type == EventType.MouseDown)
            {
                if (isHover && GUIUtility.hotControl == 0)
                {
                    GUIUtility.hotControl = id;
                    e.Use();
                    state = ButtonEvent.press;
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (hasMouseControl)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();

                    if (isHover)
                    {
                        GUI.changed = true;
                        state = ButtonEvent.click;
                    }
                }
                else state = ButtonEvent.release;
            }

            if (component.hideFlags == HideFlags.HideAndDontSave || component.hideFlags == HideFlags.HideInInspector)
            {
                if (GUI.Button(toggleRect, EditorIconContents.sceneVisibilityHiddenHover, Styles.transparentButton))
                {
                    Undo.RecordObject(component, "Modified Property in " + component.gameObject.name);
                    component.hideFlags = HideFlags.None;
                    EditorUtility.SetDirty(component);
                }
            }
            else if (ComponentUtils.CanBeDisabled(component))
            {
                EditorGUI.BeginChangeCheck();
                bool value = GUI.Toggle(toggleRect, ComponentUtils.GetEnabled(component), GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(component, "Modified Property in " + component.gameObject.name);
                    ComponentUtils.SetEnabled(component, value);
                    EditorUtility.SetDirty(component);
                }
            }

            return state;
        }

        protected override void Init()
        {
            components = targets[0].GetComponents<Component>();
            contents = new GUIContent[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null) continue;

                Texture2D assetPreview = AssetPreview.GetMiniThumbnail(component);
                GUIContent content = new GUIContent(assetPreview);

                Type type = component.GetType();
                object[] acm = type.GetCustomAttributes(typeof(AddComponentMenu), true);
                if (acm.Length > 0)
                {
                    string componentMenu = (acm[0] as AddComponentMenu).componentMenu;
                    if (!string.IsNullOrEmpty(componentMenu)) content.text = componentMenu.Split('/').Last();
                    else content.text = ObjectNames.NicifyVariableName(type.Name);
                }
                else content.text = ObjectNames.NicifyVariableName(type.Name);
                contents[i] = content;
            }

            labelContent = new GUIContent("Components:");
            addContent = new GUIContent("+", "Add Component");
        }

        public bool Validate()
        {
            if (!Prefs.componentsInPopupWindow) return false;
            if (targets == null || targets.Length != 1) return false;

            return true;
        }

        private void WaitCloseContextMenu()
        {
            EditorMenu.Close();
        }
    }
}