/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public class MiniLayoutsManager : StandalonePrefManager<MiniLayoutsManager>
        {
            private ReorderableList reorderableList;
            private bool waitWindowChanged;

            public static JsonArray json
            {
                get
                {
                    JsonArray jArr = new JsonArray();
                    for (int i = 0; i < ReferenceManager.miniLayouts.Count; i++)
                    {
                        jArr.Add(ReferenceManager.miniLayouts[i].json);
                    }

                    return jArr;
                }
                set
                {
                    if (ReferenceManager.miniLayouts.Count > 0)
                    {
                        if (!EditorUtility.DisplayDialog("Import Mini Layouts", "Mini Layouts already contain items", "Replace", "Ignore")) return;
                    }

                    ReferenceManager.miniLayouts.Clear();

                    foreach (JsonItem ji in value)
                    {
                        MiniLayout item = new MiniLayout(ji);
                        ReferenceManager.miniLayouts.Add(item);
                    }
                }
            }

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Mini Layouts",
                    };
                }
            }

            private void AddItem(ReorderableList list)
            {
                waitWindowChanged = true;
                EditorApplication.update -= WaitWindowChanged;
                EditorApplication.update += WaitWindowChanged;
            }

            public override void Draw()
            {
                if (reorderableList == null)
                {
                    reorderableList = new ReorderableList(ReferenceManager.miniLayouts, typeof(MiniLayout), true, true, true, true);
                    reorderableList.drawHeaderCallback += DrawHeader;
                    reorderableList.drawElementCallback += DrawElement;
                    reorderableList.onAddCallback += AddItem;
                    reorderableList.onRemoveCallback += RemoveItem;
                    reorderableList.onReorderCallback += Reorder;
                    reorderableList.elementHeight = 48;
                }
                
                EditorGUILayout.Space();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                reorderableList.DoLayoutList();

                EditorGUILayout.EndScrollView();

                if (waitWindowChanged)
                {
                    EditorGUILayout.HelpBox("Set the focus on the window you want to add the mini layout.", MessageType.Info);

                    if (GUILayout.Button("Stop Pick"))
                    {
                        waitWindowChanged = false;
                        EditorApplication.update -= WaitWindowChanged;
                    }
                }
            }

            private void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
            {
                MiniLayout item = ReferenceManager.miniLayouts[index];

                EditorGUI.BeginChangeCheck();
                Rect r = new Rect(rect);
                float lineHeight = rect.height / 2;
                r.height = lineHeight - 2;
                item.name = EditorGUI.TextField(r, "Name", item.name);
                if (EditorGUI.EndChangeCheck()) ReferenceManager.Save();

                EditorGUI.BeginDisabledGroup(true);
                r.y += lineHeight;
                EditorGUI.LabelField(r, "Contains", item.content, EditorStyles.textField);
                EditorGUI.EndDisabledGroup();
            }

            private void DrawHeader(Rect rect)
            {
                GUI.Label(rect, "Mini Layouts");
            }

            private void RemoveItem(ReorderableList list)
            {
                ReferenceManager.miniLayouts.RemoveAt(list.index);
                ReferenceManager.Save();
            }

            private void Reorder(ReorderableList list)
            {
                ReferenceManager.Save();
            }

            private void WaitWindowChanged()
            {
                EditorWindow wnd = EditorWindow.focusedWindow;

                if (wnd == null) return;
                if (wnd.GetType() == ProjectSettingsWindowRef.type) return;

                EditorApplication.update -= WaitWindowChanged;
                waitWindowChanged = false;
                
                EditorWindow[] windows;
                string data = LayoutHelper.SaveLayout(wnd, out windows);
                if (windows == null)
                {
                    EditorUtility.DisplayDialog("Mini Layouts", "The selected window cannot be added to Mini Layouts.", "OK");
                    return;
                }
                
                MiniLayout item = new MiniLayout
                {
                    data = data,
                    name = wnd.titleContent.text
                };
                if (windows.Length > 1) item.name += "+" + (windows.Length - 1);
                item.content = string.Join(", ", windows.Select(w => w.titleContent.text).ToArray());

                ReferenceManager.miniLayouts.Add(item);
                ReferenceManager.Save();
                
                settingsWindow.Repaint();
            }
        }
    }
}