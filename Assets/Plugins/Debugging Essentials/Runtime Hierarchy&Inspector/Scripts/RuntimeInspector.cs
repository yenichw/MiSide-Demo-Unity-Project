using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebuggingEssentials
{
    // [ExecuteInEditMode]
    public partial class RuntimeInspector : MonoBehaviour
    {
        public delegate void FloatMethod(float newValue);
        public static event FloatMethod onTimeScaleChanged;
        public static event SetActiveMethod onSetActive;

        public static RuntimeInspector instance;
        public static GameObject selectionIndicatorGO;
        public static GameObject selectedGO;
        public static NavigationCamera navigationCamera;
        public enum WindowType { Hierarchy, Inspector };
        public static bool show; 

        // [Header("VISIBLE")]
        public bool showOnStart;

        // [Header("CONTROLS")]
        public bool useSameEditorAsBuildShowKey = true;
        public AdvancedKey showToggleKeyEditor = new AdvancedKey(KeyCode.F10);
        public AdvancedKey showToggleKeyBuild = new AdvancedKey(KeyCode.F10);

        public AdvancedKey gamePauseKey = new AdvancedKey(KeyCode.P, true);
        public AdvancedKey focusCameraKey = new AdvancedKey(KeyCode.F);
        public AdvancedKey alignWithViewKey = new AdvancedKey(KeyCode.F, true, true);
        public AdvancedKey moveToViewKey = new AdvancedKey(KeyCode.F, true, false, true);
        public AdvancedKey followCameraKey = new AdvancedKey(KeyCode.G);

        // [Header("CAMERA")]
        public LayerMask selectLayerMask = 1 << 0;

        // [Header("SETTINGS")]
        public bool unfoldMultipleScenesOnStart;

        // [Header("REFERENCES")]
        public SO_Window windowData; 
        public SO_NavigationCamera navigationCameraData;

        Dictionary<CustomAssembly, DrawInfo> assemblyLookup = new Dictionary<CustomAssembly, DrawInfo>();
        Dictionary<CustomType, DrawInfo> typeLookup = new Dictionary<CustomType, DrawInfo>();
        Dictionary<Scene, DrawInfo> sceneLookup = new Dictionary<Scene, DrawInfo>();
        Dictionary<Transform, DrawInfo> tLookup = new Dictionary<Transform, DrawInfo>();
        static Dictionary<string, FastList<Type>> typeNameLookup = new Dictionary<string, FastList<Type>>(30000);
        
        FastList<Dictionary<object, DrawInfo>> memberLookups = new FastList<Dictionary<object, DrawInfo>>();

        List<GameObject> rootList = new List<GameObject>(1024);
        List<Component> components = new List<Component>(1024);

        Texture texArrowFolded;
        Texture texArrowUnfolded;
        GUISkin skin;

        const int memberLookupsCapacity = 128;

        Camera mainCam;

        Vector3 selectionIndicatorLocalPos;
        Quaternion selectionIndicatorLocalRot;
        Vector2 lastMousePos;
        Vector2 mousePos;

        Color colorInherited = new Color(1.0f, 0.5f, 0.2f, 1f);
        Color colorStatic = new Color(0.5f, 0.5f, 1f);
        Color colorPublic = new Color(0.25f, 1, 0.25f, 1);
        Color colorProtected = new Color(1, 1, 0.25f);
        Color colorPrivate = new Color(1, 0.25f, 0.25f, 1); 

        Event currentEvent;

        float oldTimeScale;

        WindowSettings inspectorWindow;
        WindowSettings hierarchyWindow;

        ScrollViewCullData hierarchyCull = new ScrollViewCullData();
        
        GUIStyle wrapButton;
        GUIStyle wrapTextField;

        DrawEnum drawEnum;

        //void Awake()
        //{
        //    CopyAndCreateTexture(sourceSkin.box.normal.background);

        //    CopyAndCreateTexture(sourceSkin.button.normal.background);
        //    CopyAndCreateTexture(sourceSkin.button.hover.background);
        //    CopyAndCreateTexture(sourceSkin.button.active.background);
        //    CopyAndCreateTexture(sourceSkin.button.onNormal.background);
        //    CopyAndCreateTexture(sourceSkin.button.onHover.background);

        //    CopyAndCreateTexture(sourceSkin.window.normal.background);
        //    CopyAndCreateTexture(sourceSkin.window.onNormal.background);

        //    UnityEditor.AssetDatabase.Refresh();
        //}

        //void CopyAndCreateTexture(Texture2D texRead, float colorMulti = 1)
        //{
        //    string path = UnityEditor.AssetDatabase.GetAssetPath(skin).Replace("Skin.guiskin", texRead.name + ".png");

        //    Texture2D tex = CopyAndRemoveAlphaFromTexture(texRead, colorMulti);

        //    path = Application.dataPath + path.Replace("Assets", "");
        //    // Debug.Log(path);

        //    byte[] bytes = tex.EncodeToPNG();
        //    System.IO.File.WriteAllBytes(path, bytes);
        //}
        public static void ResetStatic()
        {
            onTimeScaleChanged = null;
            selectionIndicatorGO = null;
            selectedGO = null;
            navigationCamera = null;
            namespaceTypesLookup.Clear();
            typeNameLookup.Clear();
            show = false;
        }

        void Awake()
        {
            instance = this;
            selectionIndicatorGO = transform.GetChild(0).gameObject;
            SetSelectionSphereRadius();

            skin = windowData.skin;
            texArrowFolded = windowData.texArrowFolded;
            texArrowUnfolded = windowData.texArrowUnfolded;
            inspectorWindow = windowData.inspectorWindow;
            hierarchyWindow = windowData.hierarchyWindow;

            SetActive(showOnStart);
            // DontDestroyOnLoad(gameObject);
            if (enableCameraOnStart)
            {
                useNavigationCamera.Value = true;
                EnableCamControl();
            }

            wrapButton = new GUIStyle(skin.button);
            wrapButton.wordWrap = true;
            wrapTextField = new GUIStyle(skin.textField);
            wrapTextField.wordWrap = true;

            drawEnum = new DrawEnum(skin);

            RuntimeConsole.Register(this);

            refreshHierarchySearch = 1;
            refreshInspectorSearch = 1;
        }

        void OnEnable()
        {
            instance = this;       
        }

        void OnDisable()
        {
            isSearchingScenes = isSearchingAssemblies = isSearchingMemory = isSearchingInspector = false;
        }

        void OnDestroy()
        {
            RuntimeConsole.Unregister(this);
            if (instance == this) instance = null;
        }

        [ConsoleCommand("search.HiearchyType")]
        public static void SearchHierarchyType(string type)
        {
            SetSearch(instance.hierarchyWindow, type, SearchMode.Type, 0, true);
        }

        [ConsoleCommand("search.HierarchyName")]
        public static void SearchHierarchyName(string name)
        {
            SetSearch(instance.hierarchyWindow, name, SearchMode.Name, 0, true);
        }

        public static void SetSearch(WindowSettings window, string text, SearchMode searchMode, int searchIndex, bool setActive)
        {
            if (setActive) instance.SetActive(true);

            window.searchList[searchIndex].useSearch = true;
            window.searchList[searchIndex].text = text;
            window.searchList[searchIndex].mode = searchMode;
        }

        void SetSelectionSphereRadius()
        {
            float scale = windowData.selectionSphereRadius;
            selectionIndicatorGO.transform.localScale = new Vector3(scale, scale, scale);
        }

        void EnableCamControl()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("Cannot find the Main Camera. Please tag your Main Camera with the 'MainCamera' tag.");
                useNavigationCamera.Value = false;
                return;
            }

            navigationCamera = mainCam.gameObject.AddComponent<NavigationCamera>();
            navigationCamera.data = navigationCameraData;
        }

        void DisableCamControl()
        {
            Destroy(navigationCamera);
        }

        public void SetActive(bool active)
        {
            hierarchyWindow.drag = inspectorWindow.drag = 0;
            if (RuntimeConsole.accessLevel != AccessLevel.Admin) active = false;

            show = active;
            selectionIndicatorGO.SetActive(show);
            gameObject.SetActive(show);

            if (onSetActive != null) onSetActive.Invoke(active);

            WindowManager.CheckMouseCursorState();
        }

        public void ManualUpdate()
        {
            // if (Input.GetKeyDown(KeyCode.Space))
            // scrollViewCull.active = !scrollViewCull.active;

            if (!WindowManager.instance.onlyUseViewToggleKeys)
            {
                if (EventInput.isGamePauseKeyDown)
                {
                    pauseGame.Value = !pauseGame.Value;
                    Pause();
                }
            }

            if (!show) return;

            if (mainCam == null) mainCam = Camera.main;
            selectionIndicatorGO.SetActive(show && selectedGO != null);

#if UNITY_EDITOR
            if (linkSelect.Value)
            {
                if (UnityEditor.Selection.activeGameObject != null && selectedGO != UnityEditor.Selection.activeGameObject) SelectGameObject(UnityEditor.Selection.activeGameObject, true);
            }
#endif

            if (useNavigationCamera.Value)
            {
                if (EventInput.isMouseButtonDown0) SelectGameObjectFromScreenRay();
            }

            if (EventInput.isMouseButtonUp0)
            {
                hierarchyWindow.drag = inspectorWindow.drag = 0;
            }

            needHierarchySearch = NeedSearch(hierarchyWindow.searchList);
            if (needHierarchySearch)
            {
                if (drawAssemblies.Value) SearchAssemblies();
                else if (drawMemory.Value) SearchMemory();
                else SearchScenes();
            }

            needInspectorSearch = NeedSearch(inspectorWindow.searchList);
            if (needInspectorSearch)
            {
                if (drawScenes.Value)
                {
                    if (selectedGO) SearchInsideGameObject(selectedGO);
                }
                else if (drawAssemblies.Value)
                {
                    if (selectedStaticType != null) SearchInspector(selectedStaticType);
                }
                else if (drawMemory.Value)
                {
                    if (selectedObject != null) SearchInspector(selectedObject);
                }
            }

            if (selectedGO && !WindowManager.instance.onlyUseViewToggleKeys)
            {
                if (EventInput.isControlKey && EventInput.isKeyDownD)
                {
                    Instantiate(selectedGO, selectedGO.transform.position, selectedGO.transform.rotation, selectedGO.transform.parent);
                }

                if (EventInput.isKeyDownDelete)
                {
                    Destroy(selectedGO);
                    selectedGO = null;
                }
            }
        }

        void LateUpdate()
        {
            if (selectedGO != null) UpdateSelectedIndicatorTransform();
        }

        static public void UpdateSelectedIndicatorTransform()
        {
            if (selectedGO != null)
            {
                selectionIndicatorGO.transform.position = selectedGO.transform.TransformPoint(instance.selectionIndicatorLocalPos);
                selectionIndicatorGO.transform.rotation = selectedGO.transform.rotation * instance.selectionIndicatorLocalRot;
            }
        }

        bool NeedSearch(List<Search> searchList)
        {
            bool needSearch = false;

            for (int i = 0; i < searchList.Count; i++)
            {
                Search search = searchList[i];
                if (search.useSearch && search.hasSearch) { needSearch = true; break; }
            }

            return needSearch;
        }

        public void MyOnGUI()
        {
            currentEvent = Event.current;

            if (Helper.IsShowKeyPressed(currentEvent, useSameEditorAsBuildShowKey, showToggleKeyEditor, showToggleKeyBuild))
            {
                if (!show && RuntimeConsole.accessLevel != AccessLevel.Admin) return;

                SetActive(!show);
                return;
            }

            if (!show) return;

            GUI.skin = skin;

            mousePos = currentEvent.mousePosition;

            skin.box.fontStyle = FontStyle.Bold;

            if (WindowManager.instance.useCanvas) GUI.backgroundColor = new Color(1, 1, 1, 0);
            else GUI.backgroundColor = windowData.color;

            hierarchyWindow.Update(311, 260);
            inspectorWindow.Update(270, 260);

            if (inspectorWindow.isDocked.Value)
            {
                inspectorWindow.rect.yMax = hierarchyWindow.rect.yMax;
                inspectorWindow.rect.yMin = hierarchyWindow.rect.yMin;
                inspectorWindow.position.y = hierarchyWindow.position.y;
                inspectorWindow.position.x = hierarchyWindow.position.x + (hierarchyWindow.rect.width / Screen.width);
            }

            Helper.DrawWindow(3242340, hierarchyWindow, DrawHierarchy);

            if (ShouldInspectorWindowShow())
            {
                Helper.DrawWindow(2342341, inspectorWindow, DrawInspector);
            }

            drawEnum.Draw(windowData.boxAlpha, windowData.backgroundAlpha);
        }

        public static bool ShouldInspectorWindowShow()
        {
            return ((selectedGO && instance.drawScenes.Value) || (instance.selectedStaticType != null && instance.drawAssemblies.Value) || (instance.selectedObject != null && instance.drawMemory.Value));
        }
    }
}
