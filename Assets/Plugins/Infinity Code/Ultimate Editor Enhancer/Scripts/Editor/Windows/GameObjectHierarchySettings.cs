/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.HierarchyTools;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class GameObjectHierarchySettings : AutoSizePopupWindow
    {
        public static Color[] colors = {
            Color.gray, Color.blue, Color.cyan, Color.green,
            Color.yellow, new Color32(0xFF, 0xAA, 0, 255), Color.red, new Color32(0xAA, 0x00, 0xFF, 255)
        };

        private static GUIStyle backgroundStyle = "sv_iconselector_back";
        private static GUIContent[] labelIcons;
        private static GUIContent noneButtonContent;
        private static GUIStyle noneButtonStyle = "sv_iconselector_button";
        private static bool recursive = false;
        private static Color selectedColor = Color.black;
        private static GUIStyle selectionLabelStyle = "sv_iconselector_labelselection";
        private static GUIStyle selectionStyle = "sv_iconselector_selection";
        private static GUIStyle seperatorStyle = "sv_iconselector_sep";
        private static GUIContent[] smallIcons;
        private static Object[] targets;

        private void DrawBackgrounds()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Background");

            if (GUILayout.Button(noneButtonContent, noneButtonStyle))
            {
                RemoveBackground();
                Close();
                return;
            }

            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);

            GUILayout.Label("", seperatorStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            for (int i = 0; i < labelIcons.Length / 2; i++)
            {
                GUIContent icon = labelIcons[i];
                Color color = colors[i];
                DrawIconButton(icon, null, true, () => SetBackground(color));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            for (int i = labelIcons.Length / 2; i < labelIcons.Length; i++)
            {
                GUIContent icon = labelIcons[i];
                Color color = colors[i];
                DrawIconButton(icon, null, true, () => SetBackground(color));
            }

            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;
            try
            {
                selectedColor = EditorGUILayout.ColorField("Color", selectedColor);
            }
            catch (ExitGUIException e)
            {
                throw e;
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetBackground(selectedColor);
            }

            EditorGUI.BeginChangeCheck();
            recursive = EditorGUILayout.ToggleLeft("Recursive", recursive);
            if (EditorGUI.EndChangeCheck()) SetRecursive();

            EditorGUIUtility.labelWidth = labelWidth;
            
        }

        private void DrawIconButton(GUIContent content, Texture2D selectedIcon, bool labelIcon, Action OnSelected)
        {
            if (content.image == selectedIcon && Event.current.type == EventType.Repaint)
            {
                object topLevel = GUILayoutUtilityRef.GetTopLevel();
                Rect rect = GUILayoutGroupRef.PeekNext(topLevel);

                float offset = 2;
                rect.x -= offset;
                rect.y -= offset;
                rect.width = selectedIcon.width + offset * 2;
                rect.height = selectedIcon.height + offset * 2;
                GUIStyle style = labelIcon ? selectionLabelStyle : selectionStyle;
                style.Draw(rect, GUIContent.none, false, false, false, false);
            }

            if (GUILayout.Button(content, GUIStyle.none)) OnSelected();
            if (Event.current.clickCount == 2) Close();
        }

        private void DrawIcons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon");

            if (GUILayout.Button(noneButtonContent, noneButtonStyle))
            {
                SetIcon(null);
                Close();
                return;
            }

            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();

            Texture2D texture2D = AssetPreview.GetMiniThumbnail(targets[0]);

            GUILayout.Space(4);
            GUILayout.Label("", seperatorStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            for (int i = 0; i < labelIcons.Length / 2; i++)
            {
                GUIContent icon = labelIcons[i];
                DrawIconButton(icon, texture2D, true, () => SetIcon((Texture2D)icon.image));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            for (int i = labelIcons.Length / 2; i < labelIcons.Length; i++)
            {
                GUIContent icon = labelIcons[i];
                DrawIconButton(icon, texture2D, true, () => SetIcon((Texture2D)icon.image));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
            
            GUILayout.Label("", seperatorStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(9f);
            for (int i = 0; i < smallIcons.Length / 2; i++)
            {
                GUIContent icon = smallIcons[i];
                DrawIconButton(icon, texture2D, false, () => SetIcon((Texture2D)icon.image));
            }
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(9f);
            for (int i = smallIcons.Length / 2; i < smallIcons.Length; ++i)
            {
                GUIContent icon = smallIcons[i];
                DrawIconButton(icon, texture2D, false, () => SetIcon((Texture2D)icon.image));
            }
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Other"))
            {
                int objectPickerID = GUIUtility.GetControlID("Other_Button".GetHashCode(), FocusType.Keyboard) + 1000;
                EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", objectPickerID);
            }
            GUILayout.Space(6f);
            GUILayout.Label("", seperatorStyle);

            Event e = Event.current;
            if (e.type == EventType.ExecuteCommand)
            {
                if (e.commandName == "ObjectSelectorUpdated")
                {
                    SetIcon(EditorGUIUtility.GetObjectPickerObject() as Texture2D);
                    GUI.changed = true;
                    e.Use();
                }
            }
        }

        private GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] guiContentArray = new GUIContent[count];
            for (int index = 0; index < count; ++index)
            {
                guiContentArray[index] = EditorGUIUtility.IconContent(baseName + (startIndex + index) + postFix);
            }
            return guiContentArray;
        }

        protected override void OnContentGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.Repaint) backgroundStyle.Draw(new Rect(0, 0, position.width, position.height), false, false, false, false);

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                e.Use();
                Close();
                return;
            }

            DrawIcons();
            DrawBackgrounds();
        }

        protected override void OnDestroy()
        {
            targets = null;
        }

        private void OnEnable()
        {
            labelIcons = GetTextures("sv_icon_name", "", 0, 8);
            smallIcons = GetTextures("sv_icon_dot", "_sml", 0, 16);
            noneButtonContent = EditorGUIUtility.IconContent("sv_icon_none");
            noneButtonContent.text = "None";
        }

        private void RemoveBackground()
        {
            selectedColor = Color.black;

            SceneReferences sceneReferences = null;

            for (int i = 0; i < targets.Length; i++)
            {
                GameObject target = targets[i] as GameObject;
                if (target == null) continue;

                if (sceneReferences == null || sceneReferences.gameObject.scene != target.scene)
                {
                    sceneReferences = SceneReferences.Get(target.scene, false);
                    if (sceneReferences == null) continue;

                    EditorUtility.SetDirty(sceneReferences);
                }

                sceneReferences.hierarchyBackgrounds.RemoveAll(b => b.gameObject == target);
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        private void SetBackground(Color color)
        {
            selectedColor = color;

            SceneReferences sceneReferences = null;

            for (int i = 0; i < targets.Length; i++)
            {
                GameObject target = targets[i] as GameObject;
                if (target == null) continue;

                if (sceneReferences == null || sceneReferences.gameObject.scene != target.scene)
                {
                    sceneReferences = SceneReferences.Get(target.scene);
                    EditorUtility.SetDirty(sceneReferences);
                }

                SceneReferences.HierarchyBackground background = sceneReferences.hierarchyBackgrounds.FirstOrDefault(b => b.gameObject == target);
                if (background == null)
                {
                    background = new SceneReferences.HierarchyBackground
                    {
                        color = color,
                        gameObject = target,
                        recursive = recursive
                    };
                    sceneReferences.hierarchyBackgrounds.Add(background);
                }
                else
                {
                    background.color = color;
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        private void SetIcon(Texture2D icon)
        {
            Undo.RecordObjects(targets, "Set Icon On GameObject");
            
            foreach (Object target in targets)
            {
                EditorGUIUtilityRef.SetIconForObject(target, icon);
            }
        }

        private void SetRecursive()
        {
            SceneReferences sceneReferences = null;
            for (int i = 0; i < targets.Length; i++)
            {
                GameObject target = targets[0] as GameObject;
                if (target == null) continue;
                if (sceneReferences == null || sceneReferences.gameObject.scene != target.scene)
                {
                    sceneReferences = SceneReferences.Get(target.scene, false);
                    if (sceneReferences == null) continue;

                    EditorUtility.SetDirty(sceneReferences);
                }

                SceneReferences.HierarchyBackground b = sceneReferences.GetBackground(target);
                b.recursive = recursive;
            }
        }

        public static GameObjectHierarchySettings ShowAtPosition(Object target, Rect rect)
        {
            return ShowAtPosition(new []{ target }, rect);
        }

        public static GameObjectHierarchySettings ShowAtPosition(Object[] targets, Rect rect)
        {
            if (targets == null || targets.Length == 0) return null;

            GameObject first = targets[0] as GameObject;
            if (first == null) return null;

            GameObjectHierarchySettings wnd = CreateInstance<GameObjectHierarchySettings>();
            GameObjectHierarchySettings.targets = targets;
            wnd.minSize = new Vector2(10, 10);
            rect = GUIUtility.GUIToScreenRect(rect);
            rect.width = 140;
            rect.height = 205;
            rect.y += 16;
            wnd.position = rect;
            wnd.adjustHeight = AutoSize.ignore;
            wnd.closeOnCompileOrPlay = true;
            wnd.ShowPopup();
            wnd.Focus();

            SceneReferences sceneReferences = SceneReferences.Get(first.scene, false);
            if (sceneReferences != null)
            {
                SceneReferences.HierarchyBackground b = sceneReferences.GetBackground(first);
                if (b != null)
                {
                    selectedColor = b.color;
                    recursive = b.recursive;
                }
            }

            return wnd;
        }

        protected override bool ValidateCloseOnLossFocus()
        {
            if (!base.ValidateCloseOnLossFocus()) return false;
            if (focusedWindow == null) return true;

            Type type = focusedWindow.GetType();
            if (type.Name == "ColorPicker" || type.Name == "ObjectSelector") return false;

            return base.ValidateCloseOnLossFocus();
        }
    }
}