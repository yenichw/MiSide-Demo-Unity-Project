/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static CreateBrowserTarget createBrowserDefaultTarget = CreateBrowserTarget.sibling;
        public static CreateBrowserTarget createBrowserAlternativeTarget = CreateBrowserTarget.root;

        public static string createBrowserBlacklist = "";
        public static bool createBrowserPreviewIcons = true;
        public static bool createBrowserPreviewSelection = true;
        public static int createBrowserMaxFilterItems = 50;

        private static string[] createBrowserBlacklists;

        public class CreateBrowserManager : StandalonePrefManager<CreateBrowserManager>, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Create Browser", "Prefabs Folder Blacklist" }.Concat(ObjectPlacerManager.GetKeywords()); }
            }

            public override void Draw()
            {
                ObjectPlacerManager.Draw(null);

                createBrowserDefaultTarget = (CreateBrowserTarget)EditorGUILayout.EnumPopup("Default Target", createBrowserDefaultTarget);
                createBrowserAlternativeTarget = (CreateBrowserTarget)EditorGUILayout.EnumPopup("Alternative Target", createBrowserAlternativeTarget);
                createBrowserPreviewIcons = EditorGUILayout.Toggle("Preview Icons", createBrowserPreviewIcons);
                createBrowserPreviewSelection = EditorGUILayout.Toggle("Preview Selection", createBrowserPreviewSelection);
                createBrowserMaxFilterItems = EditorGUILayout.IntField("Max Filter Items", createBrowserMaxFilterItems);

                EditorGUILayout.LabelField("Prefabs Folder Blacklist", EditorStyles.label);
                if (createBrowserBlacklists == null) createBrowserBlacklists = createBrowserBlacklist.Split(new []{";"}, StringSplitOptions.RemoveEmptyEntries);

                int removeIndex = -1;
                EditorGUI.indentLevel++;
                for (int i = 0; i < createBrowserBlacklists.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(createBrowserBlacklists[i], EditorStyles.textField);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal();
                }

                if (removeIndex != -1)
                {
                    ArrayUtility.RemoveAt(ref createBrowserBlacklists, removeIndex);
                    createBrowserBlacklist = string.Join(";", createBrowserBlacklists);
                    GUI.changed = true;
                }

                GUILayout.Box("To add items, drag folders here", Styles.centeredHelpbox, GUILayout.Height(30));
                ProcessDragAndDropFolder();
                EditorGUI.indentLevel--;
            }

            public string GetMenuName()
            {
                return "Create Browser";
            }

            private static void ProcessDragAndDropFolder()
            {
                Event e = Event.current;
                if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;
                Rect rect = GUILayoutUtility.GetLastRect();
                if (!rect.Contains(e.mousePosition)) return;
                if (DragAndDrop.objectReferences.Length <= 0) return;

                foreach (Object o in DragAndDrop.objectReferences)
                {
                    if (!(o is DefaultAsset))
                    {
                        return;
                    }
                }

                if (e.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
                else
                {
                    DragAndDrop.AcceptDrag();
                    ArrayUtility.AddRange(ref createBrowserBlacklists, DragAndDrop.paths);
                    createBrowserBlacklist = string.Join(";", createBrowserBlacklists);
                    GUI.changed = true;
                }

                e.Use();
            }

            void IStateablePref.SetState(bool state)
            {
                GetManager<ObjectPlacerManager>().SetState(state);
            }
        }
    }
}