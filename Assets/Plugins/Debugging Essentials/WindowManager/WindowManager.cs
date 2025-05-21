using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebuggingEssentials
{
    [DefaultExecutionOrder(-99999999)]
    public class WindowManager : MonoBehaviour
    {
        public static WindowManager instance;
        public static EventHandling eventHandling = new EventHandling();
        static string tooltip = string.Empty;
        static int lastWindowId;
        static MouseCursor mouseCursor = new MouseCursor();
        static RenderTexture rtCanvas, rtOld;

        public SO_WindowManager windowData;

        public bool useDontDestroyOnLoadScene;
        public bool onlyUseViewToggleKeys;
        public bool useCanvas;
        public GameObject canvasGO;
        public RawImage rawImage;

        RuntimeInspector runtimeInspector;
        RuntimeConsole runtimeConsole;

        Vector2 size;
        int oldScreenWidth, oldScreenHeight;

        public class MouseCursor
        {
            public CursorLockMode lockState = CursorLockMode.None;
            public bool visible = true;

            public void GetCurrentState()
            {
                if (Cursor.lockState != CursorLockMode.None) lockState = Cursor.lockState;
                if (!Cursor.visible) visible = Cursor.visible;
            }

            public void RestoreState()
            {
                Cursor.lockState = lockState;
                Cursor.visible = visible;
            }

            public void SetActive()
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public static void CheckMouseCursorState()
        {
            mouseCursor.GetCurrentState();

            if (RuntimeInspector.show || RuntimeConsole.show)
            {
                mouseCursor.SetActive();
            }
            else
            {
                mouseCursor.RestoreState();
            }
        }

        public static void ResetAllStatic()
        {
            RuntimeInspector.ResetStatic();
            NavigationCamera.ResetStatic();
            GUIChangeBool.ResetStatic();
            DrawEnum.ResetStatic();
            CullGroup.ResetStatic();
            HtmlDebug.ResetStatic();
        }

        void Awake()
        {
            if (useDontDestroyOnLoadScene) DontDestroyOnLoad(gameObject);

            ResetAllStatic();
            
            instance = this;
            
            HtmlDebug.timeSinceStartup.Stop(); 
            HtmlDebug.timeSinceStartup.Start();

            UpdateUseCanvas(useCanvas);
        }

        void Init()
        {
            if (!runtimeInspector) runtimeInspector = GetComponentInChildren<RuntimeInspector>(true);

            if (!runtimeConsole) runtimeConsole = GetComponentInChildren<RuntimeConsole>(true);
            RuntimeConsole.instance = runtimeConsole;

            if (runtimeInspector && !runtimeInspector.gameObject.activeInHierarchy)
            {
                SetActiveDeactive(runtimeInspector.gameObject);
            }

            if (runtimeConsole && !runtimeConsole.gameObject.activeInHierarchy)
            {
                SetActiveDeactive(runtimeConsole.gameObject);
            }

            if (!runtimeConsole && runtimeInspector) RuntimeConsole.accessLevel = AccessLevel.Admin;

            if (runtimeInspector || runtimeConsole)
            {
                RuntimeInspector.InitAssemblies();
            }
        }

        void InitCanvasRenderTexture()
        {
            if (rtCanvas == null || Screen.width != oldScreenWidth || Screen.height != oldScreenHeight)
            {
                GraphicsHelper.Dispose(ref rtCanvas);

                rtCanvas = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rtCanvas.anisoLevel = 0;
                rtCanvas.filterMode = FilterMode.Point;
                rtCanvas.useMipMap = false;
                rtCanvas.Create();

                rawImage.texture = rtCanvas;

                oldScreenWidth = Screen.width;
                oldScreenHeight = Screen.height;
            }
        }

        public void UpdateUseCanvas(bool useCanvas)
        {
            canvasGO.SetActive(useCanvas);
            
            if (!useCanvas) GraphicsHelper.Dispose(ref rtCanvas);
        }

        public static void BeginRenderTextureGUI(bool clear = false)
        {
            if (!instance.useCanvas) return;

            if (Event.current.type == EventType.Repaint)
            {
                rtOld = RenderTexture.active;

                if (rtCanvas != null && RenderTexture.active != rtCanvas)
                {
                    RenderTexture.active = rtCanvas;
                    if (clear) GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
                }
            }
        }

        public static void EndRenderTextureGUI()
        {
            if (!instance.useCanvas) return;

            if (Event.current.type == EventType.Repaint)
            {
                RenderTexture.active = rtOld;
            }
        }

        void OnEnable() 
        {
            instance = this;
            Init();
        }

        void SetActiveDeactive(GameObject go)
        {
            go.SetActive(true);
            go.SetActive(false);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            
            GraphicsHelper.Dispose(ref rtCanvas);
        }

        void Update()
        {
            HtmlDebug.frameTime = HtmlDebug.timeSinceStartup.ElapsedMilliseconds / 1000f;
            HtmlDebug.currentFrame = Time.frameCount;

            if (HtmlDebug.instance) HtmlDebug.instance.UpdateLogs();
            if (RuntimeConsole.instance) RuntimeConsole.instance.ManualUpdate();
            if (RuntimeInspector.instance) RuntimeInspector.instance.ManualUpdate();
        }

        void LateUpdate()
        {
            EventInput.ResetInput();
        }

        void OnGUI()
        {
            if (useCanvas)
            {
                InitCanvasRenderTexture();
            }

            BeginRenderTextureGUI(true);

            float guiScale = windowData.guiScale.value;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(guiScale, guiScale, guiScale));
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.Layout) GUIChangeBool.ApplyUpdates();
            else
            {
                EventInput.GetInput();
            }

            if (runtimeInspector) runtimeInspector.MyOnGUI();
            if (runtimeConsole) runtimeConsole.MyOnGUI();
            
            if (DoWindowsContainMousePos())
            {
                eventHandling.OnEnter();
            }
            else eventHandling.OnExit();

            if (runtimeInspector)
            {
                if (runtimeInspector.windowData.showTooltip) DrawTooltip(EventInput.mousePos);
            }

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

            EndRenderTextureGUI();
        }

        public static bool DoWindowsContainMousePos()
        {
            if (instance.runtimeInspector)
            {
                WindowSettings hierarchyWindow = instance.runtimeInspector.windowData.hierarchyWindow;
                WindowSettings inspectorWindow = instance.runtimeInspector.windowData.inspectorWindow;

                if (RuntimeInspector.show && (hierarchyWindow.ContainsMousePos(EventInput.mousePos) || (RuntimeInspector.ShouldInspectorWindowShow() && inspectorWindow.ContainsMousePos(EventInput.mousePos)))) return true;
            }
            if (instance.runtimeConsole) 
            {
                WindowSettings consoleWindow = instance.runtimeConsole.windowData.consoleWindow;
                if (RuntimeConsole.show && consoleWindow.ContainsMousePos(EventInput.mousePos)) return true;
            }

            return false;
        }

        public static void SetToolTip(int windowId)
        {
            if (GUI.tooltip.Length > 0)
            {
                lastWindowId = windowId;
                tooltip = GUI.tooltip; 
            }
            else
            {
                if (windowId == lastWindowId) tooltip = string.Empty;
            }
        }

        void DrawTooltip(Vector2 mousePos)
        {
            if (tooltip.Length > 0)
            {
                size = GUI.skin.label.CalcSize(Helper.GetGUIContent(tooltip));
                size.x += 10;
                size.y += 2.5f;
                Rect rect = new Rect(mousePos.x, mousePos.y, size.x, size.y);
                rect.x += 5;
                rect.y += 20;

                rect = GUI.Window(423423, rect, DoWindow, GUIContent.none);
            }
        }

        void DoWindow(int windowId)
        {
            GUI.BringWindowToFront(windowId);
            Rect rect = new Rect(0, 0, size.x, size.y);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            GUI.Box(rect, GUIContent.none);
            GUI.backgroundColor = Color.white;
            rect.x += 5;
            GUI.Label(rect, tooltip);
        }

        public class EventHandling
        {
            FastList<EventSystem> eventSystemList = new FastList<EventSystem>();
            public bool hasEntered;

            public void OnEnter()
            {
                if (hasEntered) return;
                hasEntered = true;

                EventSystem eventSystem = EventSystem.current;
                if (eventSystem == null) return;

                if (eventSystem.enabled)
                {
                    eventSystem.enabled = false;
                    eventSystemList.Add(eventSystem);
                }
            }

            public void OnExit()
            {
                if (!hasEntered) return;
                hasEntered = false;

                for (int i = 0; i < eventSystemList.Count; i++)
                {
                    eventSystemList.items[i].enabled = true;
                }

                eventSystemList.Clear();
            }
        }
    }
}
