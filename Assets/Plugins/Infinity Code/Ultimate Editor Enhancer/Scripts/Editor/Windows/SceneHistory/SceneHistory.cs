/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Behaviors;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public class SceneHistory : EditorWindow
    {
        private static GUIContent closeContent;
        private static GUIContent showContent;
        private static GUIStyle showContentStyle;
        private static Texture2D noBookmarkTexture;

        private static Vector2 scrollPosition;
        private static string filter = "";
        private static List<SceneHistoryItem> filteredRecords;
        private static SceneHistoryItem selectedItem;
        private static int selectedIndex = 0;
        private static double lastClickTime;
        private static bool focusOnTextField = false;

        private static List<SceneHistoryItem> items
        {
            get { return ReferenceManager.sceneHistory; }
        }

        static SceneHistory()
        {
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        private static bool CheckPlaymode(string path)
        {
            if (EditorApplication.isPlaying)
            {
                if (EditorUtility.DisplayDialog("Opening the scene during play mode", "Opening the scene cannot be used during play mode.", "Stop play mode", "Cancel"))
                {
                    EditorApplication.isPlaying = false;
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                }
                return false;
            }

            return true;
        }

        private bool DrawRow(SceneHistoryItem item)
        {
            bool ret = false;

            EditorGUILayout.BeginHorizontal(selectedItem == item ? Styles.selectedRow : Styles.transparentRow);

            EditorGUI.BeginDisabledGroup(!item.exists);

            if (GUILayout.Button(showContent, showContentStyle, GUILayout.ExpandWidth(false), GUILayout.Height(12)))
            {
                if (SceneManagerHelper.AskForSave(SceneManager.GetActiveScene()))
                {
                    EditorSceneManager.OpenScene(item.path);
                    Close();
                }
            }

            GUIContent content = TempContent.Get(item.name, item.path);
            ButtonEvent ev = GUILayoutUtils.Button(content, EditorStyles.label, GUILayout.Height(20), GUILayout.MaxWidth(position.width - 30));
            if (ProcessRowEvents(item, ev))
            {
                Close();
            }

            bool hasBookmark = HasBookmark(item);

            Texture2D texture = noBookmarkTexture;
            string tooltip = "Add Bookmark";

            if (hasBookmark)
            {
                texture = (Texture2D)Icons.starYellow;
                tooltip = "Remove Bookmark";
            }

            if (GUILayout.Button(TempContent.Get(texture, tooltip), GUIStyle.none, GUILayout.Width(20)))
            {
                SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(item.path);
                if (hasBookmark) Bookmarks.Remove(asset);
                else Bookmarks.Add(asset);
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(closeContent, Styles.transparentButton, GUILayout.ExpandWidth(false), GUILayout.Height(12)))
            {
                ret = true;
            }

            EditorGUILayout.EndHorizontal();

            return ret;
        }

        private static bool HasBookmark(SceneHistoryItem item)
        {
            for (int i = 0; i < ReferenceManager.bookmarks.Count; i++)
            {
                ProjectBookmark bookmark = ReferenceManager.bookmarks[i];
                if (bookmark.target == null) continue;
                if (bookmark.path == item.path) return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            filteredRecords = null;
            filter = "";
        }

        private void OnEnable()
        {
            filteredRecords = null;
            if (items.Count > 0) selectedItem = items[selectedIndex];

            showContent = new GUIContent(Styles.isProSkin ? Icons.openNewWhite : Icons.openNewBlack, "Open Scene");
            closeContent = new GUIContent(Styles.isProSkin ? Icons.closeWhite : Icons.closeBlack, "Remove");

            foreach (SceneHistoryItem item in items)
            {
                item.CheckExists();
            }

            focusOnTextField = true;
        }

        private void OnGUI()
        {
            if (showContentStyle == null || showContentStyle.normal.background == null)
            {
                showContentStyle = new GUIStyle(Styles.transparentButton)
                {
                    margin =
                    {
                        top = 6,
                        left = 6
                    }
                };
            }

            if (noBookmarkTexture == null)
            {
                noBookmarkTexture = Styles.isProSkin ? (Texture2D) Icons.starWhite : (Texture2D) Icons.starBlack;
            }

            if (ProcessEvents())
            {
                Close();
                return;
            }
            Toolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int removeIndex = -1;

            if (filteredRecords == null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    SceneHistoryItem item = items[i];
                    if (DrawRow(item)) removeIndex = i; 
                }
            }
            else
            {
                for (int i = 0; i < filteredRecords.Count; i++)
                {
                    SceneHistoryItem item = filteredRecords[i];
                    if (DrawRow(item)) removeIndex = items.IndexOf(item);
                }
            }

            if (removeIndex != -1)
            {
                items.RemoveAt(removeIndex);
                ReferenceManager.Save();
                if (filteredRecords != null) UpdateFilteredRecords();
            }

            EditorGUILayout.EndScrollView();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (SceneManagerHelper.AskForSave(SceneManager.GetActiveScene()))
            {
                SelectionHistory.Clear();
                EditorSceneManager.OpenScene(selectedItem.path);
            }
        }

        private static void OnSceneClosed(Scene scene)
        {
            if (EditorApplication.isPlaying) return;

            SelectionHistory.Clear();
            string path = scene.path;
            if (string.IsNullOrEmpty(path)) return;

            SceneHistoryItem item = items.FirstOrDefault(i => i.path == path);
            if (item != null)
            {
                items.Remove(item);
                items.Insert(0, item);
                return;
            }

            item = new SceneHistoryItem
            {
                path = path,
                name = scene.name
            };
            item.CheckExists();
            items.Insert(0, item);

            while (items.Count > 19) items.RemoveAt(items.Count - 1);
            ReferenceManager.Save();
        }

        [MenuItem("File/Recent Scenes %#o", false, 155)]
        public static void OpenWindow()
        {
            GetWindow<SceneHistory>(true, "Recent Scenes");
        }

        private void ProcessContextMenu(SceneHistoryItem item)
        {
            selectedItem = item;
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Open", () => SelectCurrent());
            menu.Add("Open Additive", () => { EditorSceneManager.OpenScene(item.path, OpenSceneMode.Additive); });
            menu.AddSeparator();
            menu.Add("Remove", () =>
            {
                items.Remove(item);
                ReferenceManager.Save();
                if (filteredRecords != null) UpdateFilteredRecords();
            });
            menu.Show();
        }

        private bool ProcessEvents()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow) SelectNext();
                else if (e.keyCode == KeyCode.UpArrow) SelectPrev();
                else if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return) return SelectCurrent();
                else if (e.keyCode == KeyCode.Escape) return true;
            }

            return false;
        }

        private bool ProcessRowClick(SceneHistoryItem item)
        {
            if (selectedItem == item)
            {
                if (EditorApplication.timeSinceStartup - lastClickTime < 0.3)
                {
                    if (SelectCurrent())
                    {
                        Close();
                        return true;
                    }
                }

                lastClickTime = EditorApplication.timeSinceStartup;
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(item.path);
            }
            else
            {
                lastClickTime = EditorApplication.timeSinceStartup;
                selectedItem = item;
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(item.path);
                selectedIndex = items.IndexOf(item);
            }

            Event.current.Use();
            return false;
        }

        private bool ProcessRowEvents(SceneHistoryItem item, ButtonEvent ev)
        {
            Event e = Event.current;
            if (ev == ButtonEvent.press)
            {
                if (e.button == 0) return ProcessRowClick(item);
                selectedItem = item;
            }
            else if (ev == ButtonEvent.click)
            {
                if (e.button == 1) ProcessContextMenu(item);
            }
            else if (ev == ButtonEvent.drag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.paths = new[] { item.path };
                DragAndDrop.StartDrag("Drag " + item.name);
                e.Use();
            }

            return false;
        }

        private bool SelectCurrent()
        {
            if (selectedItem == null) return false;
            if (!selectedItem.exists) return false;
            if (!CheckPlaymode(selectedItem.path)) return false;

            if (SceneManagerHelper.AskForSave(SceneManager.GetActiveScene()))
            {
                SelectionHistory.Clear();
                EditorSceneManager.OpenScene(selectedItem.path);
                return true;
            }

            return false;
        }

        private void SelectPrev()
        {
            selectedIndex--;
            if (filteredRecords != null)
            {
                if (filteredRecords.Count == 0)
                {
                    selectedIndex = 0;
                    selectedItem = null;
                }
                else
                {
                    if (selectedIndex < 0) selectedIndex = filteredRecords.Count - 1;
                    selectedItem = filteredRecords[selectedIndex];
                }
            }
            else
            {
                if (selectedIndex < 0) selectedIndex = items.Count - 1;
                selectedItem = items[selectedIndex];
            }
            Event.current.Use();
            Repaint();
        }

        private void SelectNext()
        {
            selectedIndex++;
            if (filteredRecords != null)
            {
                if (filteredRecords.Count == 0)
                {
                    selectedIndex = 0;
                    selectedItem = null;
                }
                else
                {
                    if (selectedIndex >= filteredRecords.Count) selectedIndex = 0;
                    selectedItem = filteredRecords[selectedIndex];
                }
            }
            else
            {
                if (selectedIndex >= items.Count) selectedIndex = 0;
                selectedItem = items[selectedIndex];
            }

            Event.current.Use();
            Repaint();
        }

        private void Toolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("UEESceneHistoryTextField");
            filter = GUILayoutUtils.ToolbarSearchField(filter);
            if (EditorGUI.EndChangeCheck()) UpdateFilteredRecords();

            if (focusOnTextField && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("UEESceneHistoryTextField");
                focusOnTextField = false;
            }

            if (GUILayout.Button("?", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Links.OpenDocumentation("recent-scenes");

            EditorGUILayout.EndHorizontal();
        }

        private void UpdateFilteredRecords()
        {
            if (string.IsNullOrEmpty(filter))
            {
                filteredRecords = null;
                return;
            }

            string pattern = SearchableItem.GetPattern(filter);

            filteredRecords = items.Where(i => i.Match(pattern)).ToList();
            if (!filteredRecords.Contains(selectedItem))
            {
                if (filteredRecords.Count > 0) selectedItem = filteredRecords[0];
                else selectedItem = null;
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = filteredRecords.IndexOf(selectedItem);
            }
        }

        [MenuItem("File/Recent Scenes %#o", true, 155)]
        public static bool ValidateOpenWindow()
        {
            return items.Count > 0;
        }
    }
}