/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InfinityCode.UltimateEditorEnhancer.EditorMenus;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public partial class ViewGallery : EditorWindow
    {
        private const int VerticalMargin = 50;
        private const int MaxFlatHeight = 150;

        public delegate void DrawCamerasDelegate(ViewGallery gallery, float rowHeight, float maxLabelWidth, ref int offsetY, ref int row);

        public static Action<GenericMenuEx> OnPrepareViewStatesMenu;
        
        public static bool closeOnSelect = false;
        public static bool isDirty = true;

        private static GUIStyle selectedStyle;
        private static GUIContent refreshContent;
        private static GUIContent settingsContent;

        public static CameraStateItem[] cameras;
        private static ViewStateItem[] views;
        private ViewItem[] filteredItems;

        public int countCols;
        private int countRows;
        public float itemWidth;
        public float itemHeight;
        public Vector2 lastSize;
        public float offsetX;
        private int countTemporaryCameras;
        private int countAutoViews;
        private string _filter;

        static ViewGallery()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnPress += OnInvoke;
        }

        private void CacheItems()
        {
            DestroyTextures();

            ArrayList sceneViews = SceneView.sceneViews;
            if (sceneViews == null || sceneViews.Count == 0)
            {
                sceneViews = new ArrayList();
                sceneViews.Add(GetWindow<SceneView>());
            }

            InitItems(sceneViews);

            CalcItemSize();
            RenderItems();

            isDirty = false;
        }

        private void CalcItemSize()
        {
            Vector2 size = lastSize = position.size;
            size.x -= 20; // margin horizontal
            size.y -= VerticalMargin; // margin vertical + labels height

            if (filteredItems == null)
            {
                if (size.y > MaxFlatHeight)
                {
                    size.y -= 70;
                    countCols = Mathf.Max(cameras.Length, views.Length);
                    countRows = cameras.Length > 0 ? 2 : 1;
                }
                else
                {
                    countCols = cameras.Length + views.Length;
                    countRows = 1;
                }
            }
            else
            {
                countCols = filteredItems.Length;
                countRows = 1;
            }

            if (countCols == 0) countCols = 1;

            itemWidth = size.x / countCols;
            itemHeight = itemWidth * 0.75f; // 4:3

            if (itemHeight * countRows < size.y)
            {
                for (int cols = countCols - 1; cols > 0; cols--)
                {
                    float w = size.x / cols;
                    float h = w * 0.75f;

                    int rows;

                    if (filteredItems == null) rows = Mathf.CeilToInt(cameras.Length / (float) cols) + Mathf.CeilToInt(views.Length / (float) cols);
                    else rows = Mathf.CeilToInt(filteredItems.Length / (float) cols);

                    if (w > itemWidth && h * rows < size.y)
                    {
                        itemWidth = w;
                        itemHeight = h;
                        countCols = cols;
                        countRows = rows;
                    }
                    else break;
                }

                itemHeight = Mathf.FloorToInt((itemHeight - 15) / 3) * 3;
                itemWidth = itemHeight / 3 * 4;
                
            }
            else
            {
                itemHeight = Mathf.FloorToInt(size.y / countRows / 3) * 3;
                itemWidth = itemHeight / 3 * 4;
            }

            offsetX = (lastSize.x - itemWidth * countCols) / (countCols + 1);
        }

        private static void CreateViewState(object userdata)
        {
            ViewStateItem vi = userdata as ViewStateItem;
            if (vi != null)
            {
                SceneViewActions.SaveViewState();
                return;
            }

            Camera cam;
            if (userdata is Camera) cam = userdata as Camera;
            else if (userdata is CameraStateItem) cam = (userdata as CameraStateItem).camera;
            else return;

            GameObject container = TemporaryContainer.GetContainer();
            if (container == null) return;

            string pattern = @"View State \((\d+)\)";

            int maxIndex = 1;
            ViewState[] viewStates = container.GetComponentsInChildren<ViewState>();
            for (int i = 0; i < viewStates.Length; i++)
            {
                string name = viewStates[i].gameObject.name;
                Match match = Regex.Match(name, pattern);
                if (match.Success)
                {
                    string strIndex = match.Groups[1].Value;
                    int index = int.Parse(strIndex);
                    if (index >= maxIndex) maxIndex = index + 1;
                }
            }

            string viewStateName = "View State (" + maxIndex + ")";
            InputDialog.Show("Enter title of View State", viewStateName, s =>
            {
                GameObject go = new GameObject(viewStateName);
                go.tag = "EditorOnly";
                ViewState viewState = go.AddComponent<ViewState>();

                Transform t = cam.transform;
                float dist = cam.orthographic ? 0 : 5 / Mathf.Tan((float) (SceneView.lastActiveSceneView.camera.fieldOfView * 0.5 * (Math.PI / 180.0)));
                viewState.pivot = t.position + t.forward * dist;
                viewState.rotation = t.rotation;
                viewState.size = 10;
                viewState.is2D = cam.orthographic;
                viewState.title = s;

                go.transform.SetParent(container.transform, true);
                EditorMenu.Close();
                RepaintAll();
            });
        }

        private void DestroyTextures()
        {
            if (cameras != null)
            {
                foreach (CameraStateItem cam in cameras)
                {
                    if (cam.texture != null) DestroyImmediate(cam.texture);
                }
            }

            if (views != null)
            {
                foreach (ViewStateItem view in views)
                {
                    if (view.texture != null) DestroyImmediate(view.texture);
                }
            }
        }

        private void DrawAllItems()
        {
            int offsetY = 25;
            int row = 0;
            float rowHeight = itemHeight + 25;
            float maxLabelWidth = lastSize.x / countCols - 10;

            DrawCameras(rowHeight, maxLabelWidth, ref offsetY, ref row);
            DrawViewStates(row, rowHeight, offsetY, maxLabelWidth);
        }

        private void DrawCameras(float rowHeight, float maxLabelWidth, ref int offsetY, ref int row)
        {
            GUI.Label(new Rect(new Vector2(offsetX, offsetY), new Vector2(lastSize.x, 20)), "Cameras:", EditorStyles.boldLabel);

            offsetY += 20;
            for (int i = 0; i < cameras.Length; i++)
            {
                int col = i % countCols;

                float x = col * itemWidth + (col + 1) * offsetX;
                Rect rect = new Rect(x, row * rowHeight + offsetY, itemWidth, itemHeight);
                CameraStateItem cameraState = cameras[i];

                if (cameraState.Draw(rect, maxLabelWidth))
                {
                    cameraState.Set();
                    TryCloseDelayed();
                }

                if (i != cameras.Length - 1 && col == countCols - 1) row++;
            }

            row++;
            offsetY += 5;
        }

        private void DrawFilteredItems()
        {
            int offsetY = 25;
            int row = 0;
            float rowHeight = itemHeight + 25;
            float maxLabelWidth = lastSize.x / countCols - 10;

            for (int i = 0; i < filteredItems.Length; i++)
            {
                int col = i % countCols;

                float x = col * itemWidth + (col + 1) * offsetX;
                Rect rect = new Rect(x, row * rowHeight + offsetY, itemWidth, itemHeight);
                ViewItem item = filteredItems[i];
                if (item == null) continue;

                if (item.Draw(rect, maxLabelWidth))
                {
                    item.Set();
                    TryCloseDelayed();
                }

                if (i != filteredItems.Length - 1 && col == countCols - 1) row++;
            }
        }

        private void DrawFlatItems()
        {
            int offsetY = 25;
            int row = 0;
            float rowHeight = itemHeight + 25;
            float maxLabelWidth = lastSize.x / countCols - 10;
            int index = 0;
            int totalItems = cameras.Length + views.Length;

            for (int i = 0; i < cameras.Length; i++)
            {
                int col = index % countCols;

                float x = col * itemWidth + (col + 1) * offsetX;
                Rect rect = new Rect(x, row * rowHeight + offsetY, itemWidth, itemHeight);
                ViewItem item = cameras[i];
                if (item == null) continue;

                if (item.Draw(rect, maxLabelWidth))
                {
                    item.Set();
                    TryCloseDelayed();
                }

                if (index != totalItems - 1 && col == countCols - 1) row++;
                index++;
            }

            for (int i = 0; i < views.Length; i++)
            {
                int col = index % countCols;

                float x = col * itemWidth + (col + 1) * offsetX;
                Rect rect = new Rect(x, row * rowHeight + offsetY, itemWidth, itemHeight);
                ViewItem item = views[i];
                if (item == null) continue;

                if (item.Draw(rect, maxLabelWidth))
                {
                    item.Set();
                    TryCloseDelayed();
                }

                if (index != totalItems - 1 && col == countCols - 1) row++;
                index++;
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (countTemporaryCameras > 0)
            {
                if (GUILayoutUtils.ToolbarButton("Cameras"))
                {
                    GenericMenuEx menu = GenericMenuEx.Start();

                    menu.Add("Remove All Temporary Cameras", RemoveAllTemporaryCameras);

                    foreach (CameraStateItem cam in cameras.Where(c => c.camera.GetComponentInParent<TemporaryContainer>() != null))
                    {
                        menu.Add("Remove " + cam.camera.gameObject.name, RemoveTemporaryCamera, cam);
                    }

                    menu.Show();
                }
            }

            if (GUILayoutUtils.ToolbarButton("View States"))
            {
                GenericMenuEx menu = GenericMenuEx.Start();

                menu.Add("Create/From Current View", SceneViewActions.SaveViewState);

                if (views.Length > countAutoViews)
                {
                    menu.Add("Remove/All View States", RemoveAllViewStates);
                    menu.AddSeparator("Remove/");

                    for (int i = countAutoViews; i < views.Length; i++)
                    {
                        ViewStateItem v = views[i];
                        menu.Add("Remove/" + v.title, RemoveViewState, v);
                    }
                }

                if (OnPrepareViewStatesMenu != null) OnPrepareViewStatesMenu(menu);

                menu.Show();
            }

            EditorGUI.BeginChangeCheck();
            _filter = GUILayoutUtils.ToolbarSearchField(_filter);
            if (EditorGUI.EndChangeCheck()) UpdateFilteredItems();

            if (GUILayoutUtils.ToolbarButton(refreshContent)) isDirty = true;
            if (GUILayoutUtils.ToolbarButton(settingsContent))
            {
                GenericMenuEx menu = GenericMenuEx.Start();
                menu.Add("Scene View State", Prefs.viewGallerySceneViewState, () =>
                {
                    Prefs.viewGallerySceneViewState = !Prefs.viewGallerySceneViewState;
                    Prefs.Save();
                    isDirty = true;
                });
                menu.Add("UI State", Prefs.viewGalleryUIState, () =>
                {
                    Prefs.viewGalleryUIState = !Prefs.viewGalleryUIState;
                    Prefs.Save();
                    isDirty = true;
                });
                menu.AddSeparator();
                menu.Add("Settings", Settings.OpenViewsSettings);
                menu.Show();
            }
            if (GUILayoutUtils.ToolbarButton(TempContent.Get("?", "Help"))) Links.OpenDocumentation("view-gallery");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawViewStates(int row, float rowHeight, int offsetY, float maxLabelWidth)
        {
            GUI.Label(new Rect(new Vector2(offsetX, row * rowHeight + offsetY), new Vector2(lastSize.x, 20)), "View States:", EditorStyles.boldLabel);
            offsetY += 20;

            for (int i = 0; i < views.Length; i++)
            {
                int col = i % countCols;

                float x = col * itemWidth + (col + 1) * offsetX;
                Rect rect = new Rect(x, row * rowHeight + offsetY, itemWidth, itemHeight);
                ViewStateItem view = views[i];
                if (view == null) continue;
                if (view.Draw(rect, maxLabelWidth))
                {
                    view.Set();
                    TryCloseDelayed();
                }

                if (i != views.Length - 1 && col == countCols - 1) row++;
            }
        }

        private void InitItems(ArrayList sceneViews)
        {
            cameras = ObjectHelper.FindObjectsOfType<Camera>().OrderBy(c => c.name).Select(c => new CameraStateItem(c)).ToArray();
            countTemporaryCameras = cameras.Count(c => c.camera.GetComponentInParent<TemporaryContainer>() != null);

            ViewState[] viewStates = ObjectHelper.FindObjectsOfType<ViewState>().OrderBy(v => v.gameObject.name).ToArray();

            int sceneCount = sceneViews.Count;
            countAutoViews = 0;
            List<ViewStateItem> tempViews = new List<ViewStateItem>();

            if (Prefs.viewGallerySceneViewState)
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    SceneView sceneView = sceneViews[i] as SceneView;
                    string t = "Scene View";
                    if (sceneCount > 1) t += " " + (i + 1);
                    countAutoViews++;
                    tempViews.Add(new ViewStateItem
                    {
                        title = t,
                        pivot = sceneView.pivot,
                        size = sceneView.size,
                        rotation = sceneView.rotation,
                        is2D = sceneView.in2DMode,
                        view = sceneView
                    });
                }
            }

            if (Prefs.viewGalleryUIState)
            {
                Canvas[] canvases = ObjectHelper.FindObjectsOfType<Canvas>().Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
                if (canvases.Length > 0)
                {
                    Bounds bounds = new Bounds();

                    Vector3[] fourCorners = new Vector3[4];
                    for (int i = 0; i < canvases.Length; i++)
                    {
                        RectTransform rt = canvases[i].GetComponent<RectTransform>();
                        rt.GetWorldCorners(fourCorners);

                        if (i == 0) bounds = new Bounds(fourCorners[0], Vector3.zero);
                        for (int k = 0; k < 4; k++) bounds.Encapsulate(fourCorners[k]);
                    }

                    tempViews.Add(new ViewStateItem
                    {
                        title = "UI",
                        is2D = true,
                        renderUI = true,
                        pivot = bounds.center,
                        size = bounds.extents.magnitude
                    });

                    countAutoViews++;
                }
            }
            

            tempViews.AddRange(viewStates.Select(t => new ViewStateItem(t)));
            views = tempViews.ToArray();

            if (!string.IsNullOrEmpty(_filter)) UpdateFilteredItems();
        }

        private void OnDestroy()
        {
            isDirty = true;
            closeOnSelect = false;
            DestroyTextures(); 
        }

        private void OnEnable()
        {
            isDirty = true;
            if (!wantsMouseMove) wantsMouseMove = true;

            if (refreshContent == null) refreshContent = new GUIContent(EditorIconContents.refresh.image, "Refresh");
            if (settingsContent == null) settingsContent = new GUIContent(EditorIconContents.settings.image, "Settings");
        }

        private void OnFocus()
        {
            isDirty = true;
        }

        private void OnGUI()
        {
            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle(Styles.selectedRow);
                selectedStyle.fixedHeight = 0;
            }

            if (position.size != lastSize) isDirty = true;
            else if (cameras == null || views == null) isDirty = true;
            else if (cameras.Any(c => c == null) || views.Any(v => v == null)) isDirty = true;

            if (isDirty) CacheItems();

            DrawToolbar();

            if (filteredItems == null)
            {
                if (position.height > VerticalMargin + MaxFlatHeight) DrawAllItems();
                else DrawFlatItems();
            }
            else DrawFilteredItems();
        }

        private static void OnInvoke()
        {
            OpenWindow();
        }

        private static bool OnValidate()
        {
            if (!Prefs.viewGalleryHotKey) return false;

            Event e = Event.current;
            if (e.modifiers != Prefs.viewGalleryModifiers) return false;
            if (e.keyCode != Prefs.viewGalleryKeyCode) return false;
            return true;
        }

        [MenuItem(WindowsHelper.MenuPath + "View Gallery", false, MenuItemOrder.ViewGallery)]
        public static void OpenWindow()
        {
            GetWindow<ViewGallery>(false, "View Gallery", true).wantsMouseMove = true;
        }

        private void RemoveAllTemporaryCameras()
        {
            if (!EditorUtility.DisplayDialog(
                "Confirmation",
                "Are you sure you want to remove all temporary cameras?",
                "Remove", "Cancel")) return;

            Camera[] tempCameras = cameras.Select(c => c.camera).Where(c => c.GetComponentInParent<TemporaryContainer>() != null).ToArray();

            for (int i = 0; i < tempCameras.Length; i++) DestroyImmediate(tempCameras[i].gameObject);
            isDirty = true;
        }

        private void RemoveAllViewStates()
        {
            if (!EditorUtility.DisplayDialog(
                "Confirmation", 
                "Are you sure you want to remove all View States?", 
                "Remove", "Cancel")) return;

            GameObject container = TemporaryContainer.GetContainer();
            if (container == null) return;

            ViewState[] viewStates = container.GetComponentsInChildren<ViewState>();
            for (int i = 0; i < viewStates.Length; i++) DestroyImmediate(viewStates[i].gameObject);

            isDirty = true;
        }

        private static void RemoveTemporaryCamera(object obj)
        {
            GameObject go = (obj as Camera).gameObject;

            if (!EditorUtility.DisplayDialog(
                "Confirmation",
                "Are you sure you want to remove " + go.name + " camera?",
                "Remove", "Cancel")) return;

            DestroyImmediate(go);
            isDirty = true;
        }

        private static void RemoveViewState(object userdata)
        {
            ViewStateItem item = userdata as ViewStateItem;

            if (!EditorUtility.DisplayDialog(
                "Confirmation",
                "Are you sure you want to remove " + item.title + "?",
                "Remove", "Cancel")) return;

            GameObject go = item.viewState.gameObject;
            if (go.tag == "EditorOnly" && go.GetComponents<Component>().Length == 2) DestroyImmediate(go);
            else DestroyImmediate(item.viewState);
            isDirty = true;
        }

        private static void RenameViewState(object userdata)
        {
            ViewStateItem item = userdata as ViewStateItem;
            InputDialog.Show("Rename View State", item.title, delegate(string s)
            {
                item.viewState.title = item.title = s;
            });
        }

        private void RenderItems()
        {
            if (itemWidth <= 0 || itemHeight <= 0) return;

            RenderTexture renderTexture = new RenderTexture((int) itemWidth, (int) itemHeight, 16, RenderTextureFormat.ARGB32);
            RenderTexture.active = renderTexture;
            RenderTexture lastAT = null;
            CameraClearFlags clearFlags = CameraClearFlags.Skybox;

            if (cameras != null)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    CameraStateItem camera = cameras[i];
                    if (camera.texture != null) DestroyImmediate(camera.texture);
                }
            }

            Canvas[] canvases = ObjectHelper.FindObjectsOfType<Canvas>();
            List<Canvas> modifiedCanvases = new List<Canvas>();

            try
            {
                foreach (Canvas canvas in canvases)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        modifiedCanvases.Add(canvas);
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    }
                }

                for (int i = 0; i < cameras.Length; i++)
                {
                    CameraStateItem cameraState = cameras[i];
                    if (cameraState == null || cameraState.camera == null) continue;
                    Camera camera = null;
                    try
                    {
                        camera = cameraState.camera;

                        clearFlags = camera.clearFlags;
                        if (clearFlags == CameraClearFlags.Depth || clearFlags == CameraClearFlags.Nothing) camera.clearFlags = CameraClearFlags.Skybox;
                        lastAT = camera.targetTexture;
                        camera.targetTexture = renderTexture;
                        camera.Render();

                        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                        texture.Apply();
                        cameraState.texture = texture;
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }
                    camera.targetTexture = lastAT;
                    camera.clearFlags = clearFlags;
                }

                for (int i = 0; i < views.Length; i++)
                {
                    ViewStateItem item = views[i];
                    if (item.texture != null) DestroyImmediate(item.texture);
                }

                SceneView sceneView = SceneView.lastActiveSceneView;
                Camera cam = sceneView.camera;
                ViewStateItem defaultView = new ViewStateItem(sceneView);
                lastAT = cam.activeTexture;
                clearFlags = cam.clearFlags;
                float nearClipPlane = cam.nearClipPlane;
                cam.nearClipPlane = 0.01f;
                CameraClearFlags activeClearFlags = cam.clearFlags;
                Color camBackgroundColor = cam.backgroundColor;
                if (clearFlags == CameraClearFlags.Depth || clearFlags == CameraClearFlags.Nothing) cam.clearFlags = activeClearFlags = CameraClearFlags.Skybox;
                cam.targetTexture = renderTexture;

                for (int i = 0; i < views.Length; i++)
                {
                    ViewStateItem item = views[i];
                    item.SetView(sceneView);

                    if (item.renderUI)
                    {
                        Camera canvasCamera = cam;
                        cam.clearFlags = CameraClearFlags.Color;
                        cam.backgroundColor = Color.gray;

                        for (int j = 0; j < modifiedCanvases.Count; j++)
                        {
                            Canvas canvas = modifiedCanvases[j];
                            canvas.worldCamera = canvasCamera;
                            canvas.planeDistance = cam.nearClipPlane * 1.1f;
                            canvas.scaleFactor = renderTexture.width / sceneView.position.width;
                        }
                    }
                    else
                    {
                        cam.clearFlags = activeClearFlags;
                        cam.backgroundColor = camBackgroundColor;
                        for (int j = 0; j < modifiedCanvases.Count; j++)
                        {
                            modifiedCanvases[j].worldCamera = null;
                        }
                    }

                    cam.Render();

                    Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                    texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    texture.Apply();
                    item.texture = texture;
                }

                defaultView.SetView(sceneView);

                cam.targetTexture = lastAT;
                cam.clearFlags = clearFlags;
                cam.nearClipPlane = nearClipPlane;

                RenderTexture.active = null;
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            foreach (Canvas canvas in modifiedCanvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                canvas.scaleFactor = 1;
            }
        }

        public static void RepaintAll()
        {
            isDirty = true;
            ViewGallery[] galleries = UnityEngine.Resources.FindObjectsOfTypeAll<ViewGallery>();
            Debug.Log(galleries.Length);
            foreach (ViewGallery gallery in galleries)
            {
                gallery.Repaint();
            }
        }

        private static void RestoreViewState(object userdata)
        {
            ViewStateItem viewItem = userdata as ViewStateItem;
            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.in2DMode = viewItem.is2D;
            sceneView.pivot = viewItem.pivot;
            sceneView.size = viewItem.size;

            if (!viewItem.is2D)
            {
                sceneView.rotation = viewItem.rotation;
                sceneView.camera.fieldOfView = 60;
            }

            GetWindow<SceneView>();
        }

        private void TryCloseDelayed()
        {
            if (closeOnSelect) EditorApplication.delayCall += Close;
        }

        private void UpdateFilteredItems()
        {
            if (string.IsNullOrEmpty(_filter))
            {
                filteredItems = null;
                CalcItemSize();
                RenderItems();
                return;
            }

            string pattern = SearchableItem.GetPattern(_filter);

            filteredItems = cameras.Select(c => c as ViewItem).Concat(views).Where(i => i.Match(pattern)).ToArray();

            CalcItemSize();
            RenderItems();
        }
    }
}