/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        private const string SearchFieldName = "UEESearchTextField";
        
        private static Action OnNextRepaint;
        
        public static int setSelectionIndex = -1;
        public static string searchText;
        
        private static int bestRecordIndex = 0;
        private static bool focusOnTextField = false;
        private static bool isDragStarted = false;
        private static bool resetSelection;
        private static Vector2 scrollPosition;
        private static string[] searchModeLabels = { "Everywhere", "By Hierarchy", "By Project" };
        private static bool updateScroll;
        private static bool isDirty = false;
        
        private void DrawBestRecords()
        {
            if (countBestRecords == 0) return;

            if (bestRecordIndex >= countBestRecords) bestRecordIndex = 0;
            else if (bestRecordIndex < 0) bestRecordIndex = countBestRecords - 1;

            if (updateScroll)
            {
                float bry = 20 * bestRecordIndex - scrollPosition.y;
                if (bry < 0) scrollPosition.y = 20 * bestRecordIndex;
                else if (bry > 80)
                {
                    if (bestRecordIndex != countBestRecords - 1) scrollPosition.y = 20 * bestRecordIndex - 80;
                    else scrollPosition.y = 20 * bestRecordIndex - 80;
                }
            }

            int selectedIndex = -1;
            int selectedState = -1;

            int y = 40;
            if (searchMode == 2 && !string.IsNullOrEmpty(pathStartsWith))
            {
                y += 20;
            }
            scrollPosition = GUI.BeginScrollView(new Rect(0, y, position.width, position.height - y), scrollPosition, new Rect(0, 0, position.width - y, countBestRecords * 20));

            for (int i = 0; i < countBestRecords; i++)
            {
                int state = bestRecords[i].Draw(i);
                if (state != 0)
                {
                    selectedIndex = i;
                    selectedState = state;
                }
            }

            GUI.EndScrollView();

            if (selectedIndex != -1) SelectRecord(selectedIndex, selectedState);
        }

        private void DrawFolderField()
        {
            if (searchMode != 2 || string.IsNullOrEmpty(pathStartsWith)) return;

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            EditorGUILayout.LabelField("Folder", pathStartsWith, EditorStyles.textField);
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void DrawHeader()
        {
            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, EditorStyles.toolbar);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

            if (Prefs.searchByProject)
            {
                GUILayout.Space(100);

                EditorGUI.BeginChangeCheck();
                searchMode = GUILayout.Toolbar(searchMode, searchModeLabels, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    needUpdateBestRecords = true;
                    focusOnTextField = true;
                }

                GUILayout.Space(80);
            }
            else
            {
                GUILayout.Label("Search", Styles.centeredLabel, GUILayout.Height(20));
            }

            if (GUILayoutUtils.ToolbarButton(TempContent.Get(EditorIconContents.settings.image, "Settings")))
            {
                Settings.OpenSearchSettings();
            }

            if (GUILayoutUtils.ToolbarButton("?"))
            {
                Links.OpenDocumentation("smart-search");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHint()
        {
            int y = 15;
            if (searchMode == 2 && !string.IsNullOrEmpty(pathStartsWith))
            {
                y += 10;
            }
            GUI.Label(new Rect(0, y, position.width, position.height), "Enter the name of the object you are looking for.\nSupports fuzzy search, and you can enter a query in part.\nTo search by object type, enter \"query:type\".\n\nUse Tab to quickly switch between searching\neverywhere, in the hierarchy, and in the project.", Styles.centeredLabel);
        }

        private void DrawNothingFound()
        {
            GUI.Label(new Rect(0, 0, position.width, position.height), "Nothing found.", Styles.centeredLabel);
        }

        private void DrawSearchField()
        {
            GUI.SetNextControlName(SearchFieldName);
            EditorGUI.BeginChangeCheck();
            searchText = GUILayoutUtils.ToolbarSearchField(searchText);
            isDirty = isDirty || EditorGUI.EndChangeCheck();

            if (Event.current.type == EventType.Repaint)
            {
                if (resetSelection) ResetSelection();

                if (OnNextRepaint != null)
                {
                    OnNextRepaint();
                    OnNextRepaint = null;
                }
            }

            if (focusOnTextField && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(SearchFieldName);
                focusOnTextField = false;
                if (!string.IsNullOrEmpty(searchText))
                {
                    resetSelection = true;
                }
            }
        }

        protected void OnDestroy()
        {
            bestRecords = null;
        }

        private void OnEnable()
        {
            bestRecords = new Record[MaxRecords];
            countBestRecords = 0;
            bestRecordIndex = 0;

            CacheScene();
            if (Prefs.searchByProject) CacheProject();
            if (Prefs.searchByWindow) CacheWindows();
        }

        protected override void OnGUI()
        {
            if (focusedWindow != instance)
            {
                if (isDragStarted)
                {
                    if (DragAndDrop.objectReferences.Length == 0) isDragStarted = false;
                    else Repaint();
                }

                if (!isDragStarted && focusedWindow != null && focusedWindow.GetType().Name != "ContextMenu")
                {
                    EventManager.BroadcastClosePopup();
                    return;
                }
            }

            if (EditorApplication.isCompiling)
            {
                EventManager.BroadcastClosePopup();
                return;
            }

            if (sceneRecords == null) CacheScene();
            if (projectRecords == null) CacheProject();
            if (windowRecords == null) CacheWindows();

            if (!ProcessEvents()) return;

            DrawHeader();

            isDirty = false;
            
            DrawSearchField();
            DrawFolderField();
            
            if (isDirty || needUpdateBestRecords) UpdateBestRecords();

            if (string.IsNullOrEmpty(searchText)) DrawHint();
            else if (countBestRecords == 0) DrawNothingFound();
            else DrawBestRecords();
        }

        private static bool ProcessEvents()
        {
            Event e = Event.current;
            updateScroll = false;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow)
                {
                    bestRecordIndex++;
                    updateScroll = true;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    bestRecordIndex--;
                    updateScroll = true;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    if (countBestRecords > 0)
                    {
                        Record bestRecord = bestRecords[bestRecordIndex];
                        int state = 1;
                        if (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command) state = 2;
                        else if (e.modifiers == EventModifiers.Shift) state = 3;
                        bestRecord.Select(state);

                        EventManager.BroadcastClosePopup();

                        return false;
                    }
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    EventManager.BroadcastClosePopup();
                    return false;
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (Prefs.searchByProject && e.keyCode == KeyCode.Tab)
                {
                    focusOnTextField = true;
                    searchMode++;
                    if (searchMode == 3) searchMode = 0;

                    bestRecordIndex = 0;
                    needUpdateBestRecords = true;
                    e.Use();
                }
            }

            return true;
        }
        
        private void ResetSelection()
        {
            TextEditor recycledEditor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
            if (recycledEditor != null)
            {
                if (setSelectionIndex == -1)
                {
                    recycledEditor.SelectNone();
                    recycledEditor.cursorIndex = searchText.Length;
                }
                else
                {
                    recycledEditor.SelectNone();
                    recycledEditor.cursorIndex = 0;
                }
            }

            resetSelection = false;
        }
    }
}