/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public partial class Search : PopupWindow
    {
        private const int Width = 500;
        private const int MaxRecords = 50;
        private const int SearchByFolderPriority = 25;
        
        public static int searchMode = 0;

        private static Dictionary<string, Record> projectRecords;
        private static Dictionary<int, Record> sceneRecords;
        private static Dictionary<int, Record> windowRecords;
        
        private static Record[] bestRecords;
        private static int countBestRecords = 0;
        private static bool needUpdateBestRecords;
        
        private static string pathStartsWith;

        public static Search instance { get; private set; }

        static Search()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnPress += OnInvoke;

            binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidateScript;
            binding.OnPress += OnInvokeScript;
        }

        public static void OnInvoke()
        {
            Event e = Event.current;
            Vector2 position = e.mousePosition;

            if (focusedWindow != null) position += focusedWindow.position.position;

            Rect rect = new Rect(position + new Vector2(Width / -2, -30), new Vector2(Width, 140));

#if !UNITY_EDITOR_OSX
            if (rect.y < 5) rect.y = 5;
            else if (rect.yMax > Screen.currentResolution.height - 40) rect.y = Screen.currentResolution.height - 40 - rect.height;
#endif

            Show(rect);
            e.Use();
        }

        private static void OnInvokeScript()
        {
            OnInvoke();
            searchText = ":script";
            setSelectionIndex = 0;
            resetSelection = true;
            searchMode = 2;
        }

        private static bool OnValidate()
        {
            if (!Prefs.search) return false;
            Event e = Event.current;

            if (e.keyCode != Prefs.searchKeyCode) return false;
            if (e.modifiers != Prefs.searchModifiers) return false;
            
#if UNITY_2021_3_OR_NEWER
            if (EditorApplication.isPlaying && ShortcutIntegrationRef.GetIgnoreWhenPlayModeFocused()) return false;
#endif           

            if (Prefs.SearchDoNotShowOnWindows()) return false;
            return true;
        }

        private static bool OnValidateScript()
        {
            if (!Prefs.searchScript) return false;

            Event e = Event.current;
            return e.modifiers == Prefs.searchScriptModifiers && e.keyCode == Prefs.searchScriptKeyCode;
        }

        private static void SelectRecord(int index, int state)
        {
            bestRecords[index].Select(state);
            EventManager.BroadcastClosePopup();
        }

        [MenuItem("Assets/Search By Folder", false, SearchByFolderPriority)]
        private static void SearchByFolder()
        {
            OnInvoke();
            Rect rect = instance.position;
            rect.height += 20;
            instance.position = rect;
            searchMode = 2;
            pathStartsWith = AssetDatabase.GetAssetPath(Selection.activeObject);
            setSelectionIndex = 0;
            resetSelection = true;
        }

        public static void Show(Rect rect)
        {
            EventManager.BroadcastClosePopup();

            SceneView.RepaintAll();

            if (Prefs.searchPauseInPlayMode && EditorApplication.isPlaying) EditorApplication.isPaused = true;

            instance = CreateInstance<Search>();
            instance.position = rect;
            setSelectionIndex = -1;
            instance.ShowPopup();
            instance.Focus();
            focusOnTextField = true;
            searchMode = 0;
            searchText = "";
            pathStartsWith = null;

            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += b =>
            {
                instance.Close();
                b.Remove();
            };
        }

        private int TakeBestRecords(IEnumerable<Record> tempBestRecords)
        {
            bestRecords = tempBestRecords.Take(MaxRecords).ToArray();

            return bestRecords.Length;
        }

        private void UpdateBestRecords()
        {
            needUpdateBestRecords = false;
            bestRecordIndex = 0;
            countBestRecords = 0;
            scrollPosition = Vector2.zero;

            int minStrLen = 1;
            if (searchText == null || searchText.Length < minStrLen) return;

            string assetType;
            string search = SearchableItem.GetPattern(searchText, out assetType);

            IEnumerable <Record> tempBestRecords;

            if (searchMode == 0)
            {
                int currentMode = 0;
                tempBestRecords = new List<Record>();
                if (search.Length > 0)
                {
                    if (search[0] == '@') currentMode = 1;
                    else if (search[0] == '#') currentMode = 2;
                }

                if (currentMode != 0) search = search.Substring(1);

                if (Prefs.searchByWindow && currentMode == 0)
                {
                    var windows = windowRecords.Where(r => r.Value.Update(search, assetType)).Select(r => r.Value);
                    tempBestRecords = tempBestRecords.Concat(windows);
                }

                if (currentMode == 0 || currentMode == 1)
                {
                    var scenes = sceneRecords.Where(r => r.Value.Update(search, assetType)).Select(r => r.Value);
                    tempBestRecords = tempBestRecords.Concat(scenes);
                }
                if (Prefs.searchByProject && (currentMode == 0 || currentMode == 2))
                {
                    var tempProjectRecords = projectRecords.Where(r => r.Value.Update(search, assetType)).Select(r => r.Value);
                    tempBestRecords = tempBestRecords.Concat(tempProjectRecords);
                }
            }
            else if (searchMode == 1)
            {
                tempBestRecords = sceneRecords.Where(r => r.Value.Update(search, assetType)).Select(r => r.Value);
            }
            else
            {
                if (string.IsNullOrEmpty(pathStartsWith))
                {
                    tempBestRecords = projectRecords.Where(r => r.Value.Update(search, assetType)).Select(r => r.Value);
                }
                else
                {
                    tempBestRecords = projectRecords.Where(r => (r.Value as ProjectRecord).path.StartsWith(pathStartsWith) && r.Value.Update(search, assetType)).Select(r => r.Value);
                }
            }

            countBestRecords = TakeBestRecords(tempBestRecords);
            updateScroll = true;
        }

        [MenuItem("Assets/Search By Folder", true, SearchByFolderPriority)]
        private static bool ValidateSearchByFolder()
        {
            return Selection.activeObject is DefaultAsset;
        }
    }
}