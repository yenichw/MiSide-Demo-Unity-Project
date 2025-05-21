/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.Behaviors;
using InfinityCode.UltimateEditorEnhancer.Tools;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public class History : ActionItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return 800; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.history, "History");
        }

        public override void Invoke()
        {
            List<SelectionHistory.SelectionRecord> selectionItems = SelectionHistory.records;

            GenericMenuEx menu = GenericMenuEx.Start();

            List<SceneHistoryItem> sceneRecords = ReferenceManager.sceneHistory;
            Scene activeScene = SceneManager.GetActiveScene();
            for (int i = 0; i < sceneRecords.Count; i++)
            {
                SceneHistoryItem r = sceneRecords[i];
                if (r.path == activeScene.path) continue;

                menu.Add("Scenes/" + r.name, () =>
                {
                    EditorSceneManager.OpenScene(r.path);
                    EditorMenu.Close();
                });
            }

            for (int i = 0; i < selectionItems.Count; i++)
            {
                int ci = i;
                string names = selectionItems[i].GetShortNames();
                if (string.IsNullOrEmpty(names)) continue;
                menu.Add("Selection/" + names, SelectionHistory.activeIndex == i, () =>
                {
                    SelectionHistory.SetIndex(ci);
                    EditorMenu.Close();
                });
            }

            List<ToolbarWindows.WindowRecord> recentWindows = ToolbarWindows.recent;
            for (int i = 0; i < recentWindows.Count; i++)
            {
                ToolbarWindows.WindowRecord r = recentWindows[i];
                menu.Add("Windows/" + r.title, () =>
                {
                    ToolbarWindows.RestoreRecentWindow(r);
                    EditorMenu.Close();
                });
            }

            menu.Show();
        }
    }
}