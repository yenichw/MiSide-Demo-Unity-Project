/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Bookmarks
    {
        private static IList activeItems;
        private static GUIStyle audioClipButtonStyle;
        private static GUIContent closeContent;
        private static Dictionary<SceneReferences, ReorderableList> sceneItemsList = new Dictionary<SceneReferences, ReorderableList>();
        private static GUIContent showContent;
        private static GUIStyle showContentStyle;
        private static ReorderableList projectItemsList;

        private void DrawListElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (index < 0 || index >= activeItems.Count) return;

            BookmarkItem item = activeItems[index] as BookmarkItem;
            DrawRow(item, rect);
        }

        private void DrawRow(BookmarkItem item, Rect rect)
        {
            DrawRowFirstButton(item, ref rect);
            DrawRowSecondButton(item, ref rect);

            ButtonEvent event1 = DrawRowPreview(item, ref rect);
            string tooltip = "Click - Select Object";
            if (item.target is DefaultAsset) tooltip += "\nDouble Click - Open Folder";

            Rect r = new Rect(rect);
            r.width -= 20;
            ButtonEvent event2 = GUILayoutUtils.Button(r, TempContent.Get(item.title, tooltip), EditorStyles.label);

            ProcessRowEvents(item, event1, event2);

            r = new Rect(rect);
            r.xMin = r.xMax - 20;
            r.y += 2;

            if (selectedFolderItems == null && item.canBeRemoved && GUI.Button(r, closeContent, Styles.transparentButton)) removeItem = item;
        }

        private void DrawRowFirstButton(BookmarkItem item, ref Rect rect)
        {
            showContent.tooltip = GetShowTooltip(item);
            Rect r = new Rect(rect) {width = 20};
            r.y += 2;
            rect.xMin += 20;
            if (!GUI.Button(r, showContent, showContentStyle)) return;

            if (item.target is Component) ComponentWindow.Show(item.target as Component);
            else if (item.target is SceneAsset)
            {
                string path = AssetDatabase.GetAssetPath(item.target);

                if (SceneManagerHelper.AskForSave(SceneManager.GetActiveScene()))
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            else if (item.target as GameObject)
            {
                if (item.isProjectItem)
                {
                    GameObjectUtils.OpenPrefab(AssetDatabase.GetAssetPath(item.target));
                }
                else
                {
                    Selection.activeGameObject = item.gameObject;
                    WindowsHelper.ShowInspector();
                }
            }
            else EditorUtility.OpenWithDefaultApp(AssetDatabase.GetAssetPath(item.target));
        }

        private ButtonEvent DrawRowPreview(BookmarkItem item, ref Rect rect)
        {
            if (item.preview == null || !item.previewLoaded) InitPreview(item);

            Rect r = new Rect(rect) {width = 20};
            rect.xMin += 20;
            ButtonEvent event1 = GUILayoutUtils.Button(r, TempContent.Get(item.preview), GUIStyle.none);
            return event1;
        }

        private void DrawRowSecondButton(BookmarkItem item, ref Rect rect)
        {
            Rect r = new Rect(rect)
            {
                width = 20
            };
            r.yMin += 2;

            if (item.isProjectItem)
            {
                if (item.target is AudioClip) PlayStopAudioClipButton(item, r);
            }
            else if (item.gameObject != null)
            {
                bool hidden = SceneVisibilityStateRef.IsGameObjectHidden(item.gameObject);
                if (GUI.Button(r, hidden ? hiddenContent : visibleContent, Styles.transparentButton))
                {
                    SceneVisibilityManagerRef.ToggleVisibility(SceneVisibilityManagerRef.GetInstance(), item.gameObject, true);
                }
            }

            rect.xMin += 20;
        }

        private void DrawTreeItems(IEnumerable<BookmarkItem> treeItems)
        {
            foreach (BookmarkItem item in treeItems)
            {
                Rect rect = GUILayoutUtility.GetRect(position.width, position.width, 20, 20);
                DrawRow(item, rect);
            }
        }

        private void DrawTreeItems(ref ReorderableList list, IList treeItems, string label = null)
        {
            if (list == null)
            {
                bool hasHeader = !string.IsNullOrEmpty(label);
                list = new ReorderableList(treeItems, typeof(BookmarkItem), true, hasHeader, false, false);
                list.onReorderCallback += OnReorder;
                list.elementHeight = 20;
                list.drawElementCallback += DrawListElement;
                list.drawHeaderCallback += rect => GUI.Label(rect, label);
            }

            activeItems = treeItems;
            list.DoLayoutList();
        }

        private string GetShowTooltip(BookmarkItem item)
        {
            string tooltip = "Show";

            if (item.isProjectItem)
            {
                if (item.target is GameObject) tooltip = "Open Prefab";
                else if (item.target is Component) tooltip = "Open Component Window";
                else if (item.target is DefaultAsset) tooltip = "Show In Explorer";
                else if (item.target is SceneAsset) tooltip = "Open Scene";
                else tooltip = "Open in default application";
            }
            else
            {
                if (item.target is GameObject) tooltip = "Open Inspector Window";
                else if (item.target is Component) tooltip = "Open Component Window";
            }

            return tooltip;
        }

        private void OnReorder(ReorderableList list)
        {
            Save();
        }

        private void PlayStopAudioClipButton(BookmarkItem item, Rect rect)
        {
            AudioClip audioClip = item.target as AudioClip;
            bool isPlayed = AudioUtilsRef.IsClipPlaying(audioClip);
            GUIContent playContent = isPlayed ? EditorIconContents.pauseButtonOn : EditorIconContents.playButtonOn;

            if (audioClipButtonStyle == null)
            {
                audioClipButtonStyle = new GUIStyle();
                audioClipButtonStyle.margin.top = 3;
            }

            if (GUI.Button(rect, playContent, audioClipButtonStyle))
            {
                if (isPlayed) AudioUtilsRef.StopClip(audioClip);
                else AudioUtilsRef.PlayClip(audioClip);
            }
        }

        private void ProcessRowEvents(BookmarkItem item, ButtonEvent event1, ButtonEvent event2)
        {
            Event e = Event.current;
            if (event1 == ButtonEvent.click || event2 == ButtonEvent.click)
            {
                if (e.button == 0)
                {
                    if (Selection.activeObject == item.target)
                    {
                        ProcessDoubleClick(item);
                    }
                    else
                    {
                        lastClickTime = EditorApplication.timeSinceStartup;
                        Selection.activeObject = item.target;
                        EditorGUIUtility.PingObject(item.target);
                    }

                    e.Use();
                }
                else if (e.button == 1)
                {
                    ShowContextMenu(item);
                    e.Use();
                }
            }
            else if (event1 == ButtonEvent.drag || event2 == ButtonEvent.drag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { item.target };

                DragAndDrop.StartDrag("Drag " + item.target);
                e.Use();
            }
        }
    }
}