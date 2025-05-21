/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.ProjectTools;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool projectFolderIcons = true;
        public static bool projectFolderIconsDrawColors = true;
        public static bool projectFolderIconsDrawIcons = true;

        public class ProjectFolderIconManager : StandalonePrefManager<ProjectFolderIconManager>
        {
            private ReorderableList reorderableList;
            private Vector2 scrollPosition;

            public static JsonItem json
            {
                get
                { 
                    JsonArray jArr = new JsonArray();
                    for (int i = 0; i < ReferenceManager.projectFolderIcons.Count; i++)
                    {
                        jArr.Add(ReferenceManager.projectFolderIcons[i].json);
                    }

                    return jArr;
                }
                set
                { 
                    if (ReferenceManager.projectFolderIcons.Count > 0)
                    {
                        if (!EditorUtility.DisplayDialog("Import Project Folder Icons", "Project Folder Icons already contain items", "Replace", "Ignore")) return;
                    }

                    List<ProjectFolderRule> items = value.Deserialize<List<ProjectFolderRule>>();
                    ReferenceManager.projectFolderIcons.Clear();
                    ReferenceManager.projectFolderIcons.AddRange(items);
                }
            }

            private void AddItem(ReorderableList list)
            {
                ReferenceManager.projectFolderIcons.Add(new ProjectFolderRule());
                ReferenceManager.Save();
            }

            public override void Draw()
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                projectFolderIcons = EditorGUILayout.ToggleLeft("Override Folder Icons", projectFolderIcons);
                
                EditorGUI.indentLevel++;
                projectFolderIconsDrawColors = EditorGUILayout.ToggleLeft("Draw Folder Colors", projectFolderIconsDrawColors);
                projectFolderIconsDrawIcons = EditorGUILayout.ToggleLeft("Draw Folder Icons", projectFolderIconsDrawIcons);
                EditorGUI.indentLevel--;
                
                if (reorderableList == null)
                {
                    reorderableList = new ReorderableList(ReferenceManager.projectFolderIcons, typeof(ProjectFolderRule), true, true, true, true);
                    reorderableList.drawElementCallback += DrawItem;
                    reorderableList.drawHeaderCallback += DrawHeader;
                    reorderableList.elementHeightCallback += GetItemHeight;
                    reorderableList.onAddCallback += AddItem;
                    reorderableList.onRemoveCallback += RemoveItem;
                    reorderableList.onReorderCallback += Reorder;
                }
                
                EditorGUILayout.HelpBox("Tip: To make a rule for two or more folders use \"|\" as a separator.", MessageType.Info);
                EditorGUILayout.HelpBox(@"I did try to make the colour scheme beautiful and meaningful, but I'm not an artist and I'm not entirely satisfied with the result.
If you can make the colour scheme better, please export it via File / Export / Items / Project Icons and send it to me (support@infinity-code.com).
I would be very grateful for the help.", MessageType.Warning);
                
                reorderableList.DoLayoutList();

                EditorGUILayout.EndScrollView();
            }

            private void DrawHeader(Rect rect)
            {
                GUI.Label(rect, "Items");
            }

            private void DrawItem(Rect rect, int index, bool isactive, bool isfocused)
            {
                if (index >= ReferenceManager.projectFolderIcons.Count) return;
                
                ProjectFolderRule item = ReferenceManager.projectFolderIcons[index];
                
                EditorGUI.BeginChangeCheck();
                
                Rect r = new Rect(rect.position.x + 60, rect.position.y, rect.width - 60, 16);
                EditorGUI.BeginChangeCheck();
                item.folderName = EditorGUI.TextField(r, "Folder Name", item.folderName);
                if (EditorGUI.EndChangeCheck())
                {
                    item.SetDirty();
                    ProjectFolderIconDrawer.SetDirty();
                }
                
                r.y += 18;
                Rect r2 = new Rect(r.x, r.y, r.width - 22, r.height);
                EditorGUI.BeginChangeCheck();
                item.icon = EditorGUI.TextField(r2, "Icon", item.icon);
                if (EditorGUI.EndChangeCheck())
                {
                    item.SetDirty();
                    ProjectFolderIconDrawer.SetDirty();
                }

                r2.x = r.xMax - 20;
                r2.width = 20;
                if (GUI.Button(r2, "..."))
                {
                    EditorIconsBrowser.OpenWindow().OnSelect += (icon) =>
                    {
                        item.icon = icon;
                        item.SetDirty();
                        ReferenceManager.Save();
                    };
                }
                
                EditorGUI.BeginChangeCheck();
                r.y += 18;
                item.color = EditorGUI.ColorField(r, "Color", item.color);
                
                r.y += 18;
                item.rootOnly = EditorGUI.Toggle(r, "Root Only", item.rootOnly);
                
                r.y += 18;
                item.recursive = EditorGUI.Toggle(r, "Recursive", item.recursive);
                
                if (EditorGUI.EndChangeCheck())
                {
                    item.SetDirty();
                    ProjectFolderIconDrawer.SetDirty();
                }
                
                if (EditorGUI.EndChangeCheck()) ReferenceManager.Save();

                DrawPreview(rect, item);
            }

            private void DrawPreview(Rect rect, ProjectFolderRule item)
            {
                rect.y += 2;
                rect.width = rect.height = 48;
                GUI.Box(rect, GUIContent.none);
                
                Color clr = GUI.color;
                GUI.color = item.color;
                GUI.DrawTexture(rect, Icons.folder);
                GUI.color = clr;
                
                Texture icon = item.iconTexture;
                if (icon != null)
                {
                    Rect r2 = new Rect(rect.x + rect.width / 2, rect.y + rect.width / 2.2f, rect.width / 3, rect.width / 3);
                    GUI.DrawTexture(r2, item.iconTexture);
                }
            }

            private float GetItemHeight(int index)
            {
                return 18 * 5;
            }

            private void RemoveItem(ReorderableList list)
            {
                ReferenceManager.projectFolderIcons.RemoveAt(list.index);
                ReferenceManager.Save();
            }

            private void Reorder(ReorderableList list)
            {
                ReferenceManager.Save();
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                projectFolderIcons = state;
            }
        }
    }
}