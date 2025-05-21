/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.SceneTools.QuickAccessActions;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [Serializable]
    public class QuickAccessItem
    {
        public string tooltip;
        public bool expanded = true;
        public string[] settings;
        public int[] intSettings;
        public QuickAccessItemType type = QuickAccessItemType.window;
        public SceneViewVisibleRules visibleRules = SceneViewVisibleRules.always;
        public QuickAccessItemIcon icon = QuickAccessItemIcon.editorIconContent;
        public float iconScale = 1;
        public string iconSettings;
        public bool useCustomWindowSize = false;
        public Vector2 customWindowSize = new Vector2Int(400, 300);
        public bool alignWindowToBar = true;
        public QuickAccessWindowMode windowMode = QuickAccessWindowMode.popup;

        [NonSerialized]
        private GUIContent _content;
        private Type _methodType;

        [NonSerialized]
        private ScriptableObject _scriptableObject;

        [NonSerialized]
        private string _typeName;

        [NonSerialized]
        private QuickAccessAction actionObject;

        [NonSerialized]
        private bool contentMissed;

        [NonSerialized]
        private bool methodTypeMissed;
        [NonSerialized]
        private bool scriptableObjectMissed;

        public GUIContent content
        {
            get
            {
                if (_content == null && !contentMissed)
                {
                    if (string.IsNullOrEmpty(iconSettings))
                    {
                        if (icon == QuickAccessItemIcon.text)
                        {
                            _content = new GUIContent("", tooltip);
                            return _content;
                        }
                        
                        contentMissed = true;
                        return null;
                    }
                    
                    if (icon == QuickAccessItemIcon.text)
                    {
                        string text = iconSettings.Substring(0, Mathf.Min(3, iconSettings.Length));
                        _content = new GUIContent(text, tooltip);
                    }
                    else if (icon == QuickAccessItemIcon.texture)
                    {
                        Texture t = AssetDatabase.LoadAssetAtPath<Texture>(iconSettings);
                        if (t != null) _content = new GUIContent(t, tooltip);
                        else contentMissed = true;
                    }
                    else
                    {
                        Debug.unityLogger.logEnabled = false;
                        GUIContent c = EditorGUIUtility.IconContent(iconSettings, tooltip);
                        if (c != null)
                        {
                            _content = new GUIContent
                            {
                                tooltip = tooltip,
                                image = c.image,
                                text = c.text
                            };
                        }
                        Debug.unityLogger.logEnabled = true;
                        contentMissed = _content == null;
                    }
                }
                return _content;
            }
        }

        public bool canHaveIcon
        {
            get { return isButton && type != QuickAccessItemType.action; }
        }

        public bool isButton
        {
            get { return type != QuickAccessItemType.space && type != QuickAccessItemType.flexibleSpace; }
        }

        public JsonObject json
        {
            get
            {
                return Json.Serialize(this) as JsonObject;
            }
        }

        public string typeName
        {
            get
            {
                if (string.IsNullOrEmpty(_typeName)) _typeName = ObjectNames.NicifyVariableName(type.ToString());
                return _typeName;
            }
            set { _typeName = value; }
        }

        public Type methodType
        {
            get
            {
                if (type != QuickAccessItemType.staticMethod) return null;
                if (settings == null || settings.Length == 0) return null;

                if (_methodType == null && !methodTypeMissed)
                {
                    if (!string.IsNullOrEmpty(settings[0])) _methodType = Type.GetType(settings[0]);
                    methodTypeMissed = _methodType == null;
                }

                return _methodType;
            }
            set
            {
                _methodType = null;
                methodTypeMissed = false;
                ResetContent();
                if (settings == null) settings = new string[2];
                if (value == null)
                {
                    settings[0] = null;
                    settings[1] = null;
                    return;
                }
                if (settings[0] == value.AssemblyQualifiedName) return;
                settings[0] = value.AssemblyQualifiedName;
                settings[1] = null;
            }
        }

        public ScriptableObject scriptableObject 
        {
            get
            {
                if (_scriptableObject == null && !scriptableObjectMissed)
                {
                    try
                    {
                        string path = settings[0];
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            _scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        }
                    }
                    catch
                    {
                        
                    }

                    scriptableObjectMissed = _scriptableObject != null;
                }

                return _scriptableObject;
            }
            set
            {
                _scriptableObject = value;
                scriptableObjectMissed = value == null;
                if (!scriptableObjectMissed)
                {
                    settings[0] = AssetDatabase.GetAssetPath(value); 
                }
            }
        }

        public QuickAccessItem()
        {

        }

        public QuickAccessItem(QuickAccessItemType type)
        {
            this.type = type;
            if (type == QuickAccessItemType.window) settings = new string[2];
        }

        public void DrawAction()
        {
            if (type != QuickAccessItemType.action) return;

            if (methodTypeMissed) return;
            if (settings == null || settings.Length == 0 || string.IsNullOrEmpty(settings[0])) return;

            if (actionObject == null)
            {
                Type t = TypeCache.GetTypesDerivedFrom<QuickAccessAction>().FirstOrDefault(i => i.FullName == settings[0]);
                
                if (t == null)
                {
                    methodTypeMissed = true;
                    return;
                }

                actionObject = Activator.CreateInstance(t) as QuickAccessAction;
            }

            if (actionObject.Validate()) actionObject.Draw();
        }

        public void Invoke()
        {
            if (!Validate()) return;

            if (type == QuickAccessItemType.window)
            {
                InvokeWindow();
            }
            else if (type == QuickAccessItemType.settings)
            {
                string path = settings[0];
                if (path.StartsWith("Preferences")) SettingsService.OpenUserPreferences(path);
                else SettingsService.OpenProjectSettings(path);
            }
            else if (type == QuickAccessItemType.staticMethod) InvokeStaticMethod();
            else if (type == QuickAccessItemType.menuItem) InvokeMenuItem();
            else if (type == QuickAccessItemType.scriptableObject) InvokeScriptableObject();
        }

        private  void InvokeMenuItem()
        {
            string menuPath = settings[0];
            if (string.IsNullOrEmpty(menuPath)) return;
            EditorApplication.ExecuteMenuItem(menuPath);

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) sceneView.Focus();
        }

        private void InvokeScriptableObject()
        {
            int activeWindowIndex = QuickAccess.activeWindowIndex;
            QuickAccess.CloseActiveWindow();

            if (activeWindowIndex == QuickAccess.invokeItemIndex) return;
            if (scriptableObject == null) return;

            Rect rect = new Rect();
            Vector2 pos = new Vector2(QuickAccess.invokeItemRect.xMax, QuickAccess.invokeItemRect.y + PinAndClose.Height + 40);
            rect.position = GUIUtility.GUIToScreenPoint(pos);
            rect.size = Prefs.defaultWindowSize;
            AutoSize autoSize = AutoSize.top;

            if (rect.center.y > Screen.currentResolution.height / 2)
            {
                autoSize = AutoSize.bottom;
                rect.y -= rect.size.y - PinAndClose.Height + 8;
            }

            ObjectWindow window = ObjectWindow.ShowPopup(new[] { scriptableObject }, rect);
            window.closeOnLossFocus = false;

            window.adjustHeight = autoSize;
            QuickAccess.activeWindow = window;
            QuickAccess.activeWindowIndex = QuickAccess.invokeItemIndex;
        }

        private void InvokeStaticMethod()
        {
            if (settings.Length < 2) return;

            string className = settings[0];
            string methodName = settings[1];
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName)) return;

            Type type = Type.GetType(className);
            if (type == null) return;

            MethodInfo method = type.GetMethod(methodName, Reflection.StaticLookup);
            if (method == null) return;

            method.Invoke(null, new object[0]);

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) sceneView.Focus();
        }

        private void InvokeWindow()
        {
            int lastWindowIndex = QuickAccess.activeWindowIndex;
            QuickAccess.CloseActiveWindow();

            if (lastWindowIndex == QuickAccess.invokeItemIndex) return;

            Type windowType = Type.GetType(settings[0]);
            if (windowType == null)
            {
                EditorUtility.DisplayDialog("Error", "Can't find the window class. Please delete the entry and add again.", "OK");
                return;
            }

            EditorWindow wnd = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            if (wnd == null) return;

            if (wnd.titleContent.text == windowType.FullName) wnd.titleContent.text = tooltip;

            if (windowMode == QuickAccessWindowMode.utility) wnd.ShowUtility();
            else if (windowMode == QuickAccessWindowMode.tab) wnd.Show();
            else wnd.ShowPopup();

            wnd.Focus();

            Rect rect = wnd.position;
            int toolbarOffset = 40;

            try
            {
                if (FullscreenEditor.IsFullscreen(SceneView.lastActiveSceneView))
                {
                    toolbarOffset -= 20;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Vector2 pos = new Vector2(QuickAccess.invokeItemRect.xMax, QuickAccess.invokeItemRect.y + PinAndClose.Height + toolbarOffset);
#if !(!UNITY_2021_1_OR_NEWER || UNITY_2021_2_OR_NEWER)
            pos += SceneView.lastActiveSceneView.position.position;
#endif

            rect.position = GUIUtility.GUIToScreenPoint(pos);
            rect.size = useCustomWindowSize? customWindowSize: Prefs.defaultWindowSize;

            if (rect.center.y > Screen.currentResolution.height * 0.7f)
            {
                rect.y -= rect.size.y - PinAndClose.Height + 8;
            }

            if (alignWindowToBar) wnd.position = rect;

            QuickAccess.activeWindow = wnd;
            QuickAccess.activeWindowIndex = QuickAccess.invokeItemIndex;

            if (windowMode == QuickAccessWindowMode.popup)
            {
                PinAndClose.Show(wnd, rect, wnd.Close, () =>
                {
                    EditorWindow nWnd = Object.Instantiate(wnd);
                    nWnd.Show();
                    Rect wRect = wnd.position;
                    wRect.yMin -= PinAndClose.Height;
                    nWnd.minSize = wnd.minSize;
                    nWnd.maxSize = wnd.maxSize;
                    nWnd.position = wRect;
                    QuickAccess.CloseActiveWindow();
                }, wnd.titleContent.text).closeOnLossFocus = false;
            }

            if (windowType == SceneHierarchyWindowRef.type)
            {
                HierarchyHelper.ExpandHierarchy(wnd, Selection.activeGameObject);
            }
            else if (windowType == ProjectBrowserRef.type)
            {
                Reflection.InvokeMethod(windowType, "SetOneColumn", wnd);
            }
            else if (windowType == typeof(ViewGallery))
            {
                if (Prefs.quickAccessBarCloseViewGallery)
                {
                    ViewGallery.closeOnSelect = true;
                }
            }
        }

        public void ResetContent()
        {
            _content = null;
            contentMissed = false;
            actionObject = null;
            methodTypeMissed = false;
            if (type == QuickAccessItemType.action && actionObject != null) actionObject.ResetContent();
        }

        private bool Validate()
        {
            return settings != null && settings.Length > 0;
        }

        public bool Visible(bool maximized)
        {
            if (visibleRules == SceneViewVisibleRules.always) return true;
            if (visibleRules == SceneViewVisibleRules.onMaximized) return maximized;
            if (visibleRules == SceneViewVisibleRules.onNormal) return !maximized;
            return false;
        }
    }
}