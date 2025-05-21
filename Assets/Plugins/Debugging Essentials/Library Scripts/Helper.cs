using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DebuggingEssentials
{
    public static class Helper
    {
        static public Color colCommandResultFailed = new Color(1, 0.549f, 0);
        static public Color colCommandResult = new Color(0.25f, 0.6f, 1);
        static Vector2 lastMousePos;
        static GUIContent guiContent = new GUIContent();

        public static string GetConsoleLogPath()
        {
#if UNITY_EDITOR
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
#if UNITY_EDITOR_WIN
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Unity/Editor/Editor.log";
#elif UNITY_STANDALONE_WIN
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/../LocalLow" + Application.companyName + "/" + Application.productName + "/output_log.txt";
#elif UNITY_EDITOR_OSX
            return "~/Library/Logs/Unity/Editor.log";
#elif UNITY_STANDALONE_OSX
            return "~/Library/Logs/" + Application.companyName + "/" + Application.productName + "/Player.log";            
#elif UNITY_STANDALONE_LINUX
            return "~/.config/unity3d/" + Application.companyName + "/" + Application.productName + "/Player.log";
#endif
#else 
            return Application.consoleLogPath;
#endif
#else
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture).Replace(' ', '_').Replace(':', '-');
            return Application.persistentDataPath + "/" + Application.productName + "-" + Application.version + "_(" + dateString + ").html";
#endif
        }

        public static string GetApplicationInfo() { return Application.productName + " " + Application.version + " (" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + ")"; }

        public static GUIContent GetGUIContent(string text, Texture image = null, string tooltip = "")
        {
            guiContent.text = text;
            guiContent.image = image;
            guiContent.tooltip = tooltip;
            return guiContent;
        }

        public static GUIContent GetGUIContent(Texture image = null, string tooltip = "")
        {
            guiContent.text = string.Empty;
            guiContent.image = image;
            guiContent.tooltip = tooltip;
            return guiContent;
        }

        public static GUIContent GetGUIContent(string text, string tooltip)
        {
            guiContent.text = text;
            guiContent.image = null;
            guiContent.tooltip = tooltip;
            return guiContent;
        }

        public static GUIContent GetGUIContent(Texture image)
        {
            guiContent.text = guiContent.tooltip = string.Empty;
            guiContent.image = image;
            return guiContent;
        }

        public static bool IsShowKeyPressed(Event currentEvent, bool useSameEditorAsBuildShowKey, AdvancedKey showToggleKeyEditor, AdvancedKey showToggleKeyBuild)
        {
            if (useSameEditorAsBuildShowKey || Application.isEditor)
            {
                return showToggleKeyEditor.GetKeyUp(currentEvent);
            }
            else
            {
                return showToggleKeyBuild.GetKeyUp(currentEvent);
            }
        }

        public static bool IsOnlyShowKeyPressed(Event currentEvent, bool useSameEditorAsBuildShowKey, AdvancedKey showToggleKeyEditor, AdvancedKey showToggleKeyBuild)
        {
            if (useSameEditorAsBuildShowKey || Application.isEditor)
            {
                return showToggleKeyEditor.OnlyGetKey(currentEvent);
            }
            else
            {
                return showToggleKeyBuild.OnlyGetKey(currentEvent);
            }
        }

        public static float FastFrac(float v)
        {
            return (v - (int)v) % 1;
        }

        public static int Repeat(int v, int count)
        {
            if (count == 0) return 0;
            v %= count;
            if (v < 0) v = count + v;
            return v;
        }

        public static string ToTimeFormat(float time)
        {
            string ms = (FastFrac(time) * 100).ToString("00");
            string ss = ((int)time % 60).ToString("00");
            string mm = ((int)(time / 60) % 60).ToString("00");
            string hh = ((int)time / 60 / 60).ToString("00");
            return (hh + ":" + mm + ":" + ss + "." + ms);
        }

        public static int CalcStringLines(string text)
        {
            int count = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') count++;
            }

            return count;
        }

        public static Color AnimateColor(Color col, float speed = 1)
        {
            return Color.Lerp(Color.white, col, Mathf.Abs(Mathf.Sin(Time.time * speed)));
        }

        public static void DrawWindow(int windowId, WindowSettings window, GUI.WindowFunction func)
        {
            Rect rect = window.rect;
            if (window.isMinimized) rect.height = 30;
            GUILayout.Window(windowId, rect, func, GUIContent.none);
        }

        public static bool Drag(WindowSettings window, WindowSettings inspectorWindow, WindowSettings hierarchyWindow, bool titleDrag, bool onlyRightCorner = false)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.Layout) return false;

            Rect lastRect = new Rect();
            if (titleDrag)
            {
                lastRect = GUILayoutUtility.GetLastRect();
                lastRect.position += window.rect.position;
            }

            Vector2 mousePos = EventInput.mousePos;

            if (currentEvent.type == EventType.MouseDown && !currentEvent.shift && currentEvent.button == 0)
            {
                if (titleDrag && lastRect.Contains(mousePos))
                {
                    window.drag = 1;
                }
                else if (!titleDrag && window.rect.Contains(mousePos))
                {
                    if (onlyRightCorner)
                    {
                        if (mousePos.x > window.rect.xMax - 15 && mousePos.y > window.rect.yMax - 15) window.drag |= 10;
                    }
                    else
                    {
                        if (mousePos.x > window.rect.xMax - 5) window.drag |= 2;
                        else if (mousePos.x < window.rect.xMin + 7.5f && !window.isDocked.Value) window.drag |= 4;
                        if (mousePos.y > window.rect.yMax - 7.5f) window.drag |= 8;
                        else if (mousePos.y < window.rect.yMin + 7.5f) window.drag |= 16;
                    }
                }

                lastMousePos = mousePos;
            }

            if (titleDrag) return false;

            bool didDrag = false;

            if (window.drag != 0)
            {
                Vector2 deltaMousePos = mousePos - lastMousePos;
                lastMousePos = mousePos;

                if (window.drag == 1) window.position += new Vector2(deltaMousePos.x / Screen.width, deltaMousePos.y / Screen.height);
                else
                {
                    if ((window.drag & 2) != 0)
                    {
                        window.rect.xMax += deltaMousePos.x * (onlyRightCorner && window.isDocked.Value ? 2 : 1);
                        if (deltaMousePos.x != 0) didDrag = true;
                    }
                    if ((window.drag & 4) != 0)
                    {
                        window.rect.xMin += deltaMousePos.x;
                        window.position.x += deltaMousePos.x / Screen.width;
                        if (deltaMousePos.x != 0) didDrag = true;
                    }
                    if ((window.drag & 8) != 0)
                    {
                        if (inspectorWindow != null && inspectorWindow.isDocked.Value) window = hierarchyWindow;
                        window.rect.yMax += deltaMousePos.y;
                        if (deltaMousePos.y != 0) didDrag = true;
                    }
                    if ((window.drag & 16) != 0)
                    {
                        if (inspectorWindow != null && inspectorWindow.isDocked.Value) window = hierarchyWindow;

                        window.rect.yMin += deltaMousePos.y;
                        window.position.y += deltaMousePos.y / Screen.height;
                        if (deltaMousePos.y != 0) didDrag = true;
                    }
                }
            }

            return didDrag;
        }

        public static bool DrawShowButton(SO_BaseWindow windowData, GUIContent content, GUIChangeBool show, Color color, float width = -1, bool onlyActivate = false)
        { 
            if (show.Value) GUI.backgroundColor = GetColor(windowData, color);
            else GUI.backgroundColor = GetColor(windowData, Color.grey);

            bool clicked;
            if (width == -1) clicked = GUILayout.Button(content, GUILayout.Height(20)); else clicked = GUILayout.Button(content, GUILayout.Width(width), GUILayout.Height(20));
            if (clicked)
            {
                if (onlyActivate) show.Value = true;
                else show.Value = !show.Value;
            }

            GUI.backgroundColor = GetColor(windowData, Color.white);
            GUI.color = Color.white;

            return clicked;
        }

        public static bool DrawButton(GUIContent guiContent, GUIStyle style, params GUILayoutOption[] options)
        {
            if (guiContent.image) style.contentOffset = new Vector2(16, 0);

            bool clicked = GUILayout.Button(guiContent.text, style, options);
            if (guiContent.image)
            {
                style.contentOffset = new Vector2(0, 0);
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += 6;
                rect.y += 3;
                rect.width = 15;
                rect.height = 15;
                GUI.DrawTexture(rect, guiContent.image);
            }

            return clicked;
        }

        public static Color GetColor(SO_BaseWindow windowData, Color color)
        {
            color.a *= windowData.backgroundAlpha;
            return color;
        }

        public static bool IsCulled(ref ScrollViewCullData cull, float height = 25)
        {
            cull.scrollWindowPosY += height;

            float scrollWindowHeight = cull.windowHeight - cull.rectStartScrollY;
            if (cull.scrollWindowPosY < cull.updatedScrollViewY || cull.scrollWindowPosY > cull.updatedScrollViewY + scrollWindowHeight)
            {
                cull.culledSpaceY += height;
                return true;
            }
            else
            {
                if (cull.culledSpaceY > 0)
                {
                    GUILayout.Space(cull.culledSpaceY);
                    cull.culledSpaceY = 0;
                }
                return false;
            }
        }

        public static int BinarySearch<T, U>(T[] array, int length, U value) where T : IComparable<U>
        {
            int first = 0;
            int last = length - 1;
            int mid = 0;

            do
            {
                mid = first + (last - first) / 2;

                int result = array[mid].CompareTo(value);
                if (result == -1) first = mid + 1;
                else if (result == 1) last = mid - 1;
                else return mid;

            }
            while (first <= last);

            return mid;
        }

        public static bool IsPrefab(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
#if !UNITY_2017 && !UNITY_2018_1 && !UNITY_2018_2
            return UnityEditor.PrefabUtility.IsPartOfAnyPrefab(obj);
#else
            return UnityEditor.PrefabUtility.GetPrefabType(obj) == UnityEditor.PrefabType.Prefab;
#endif
#else
            return false;
#endif
        }

#if !UNITY_EDITOR
        static MethodInfo findObjectFromInstanceIdMethod = null;
#endif

        public static UnityEngine.Object FindObjectFromInstanceID(int id)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.InstanceIDToObject(id);
#else
            if (findObjectFromInstanceIdMethod == null)
            {
                findObjectFromInstanceIdMethod = typeof(UnityEngine.Object).GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            }
            if (findObjectFromInstanceIdMethod != null)
            {
                return (UnityEngine.Object)findObjectFromInstanceIdMethod.Invoke(null, new object[] { id });
            }
            else return null;
#endif
        }
    }
}


